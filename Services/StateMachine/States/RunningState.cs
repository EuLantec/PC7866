using PC7866.Models;

namespace PC7866.Services.StateMachine.States;

/// <summary>
/// Estado: ejecución – ensayo en dos pasadas completas sobre los ParametrosEnsayo ordenados:
/// Fase A (resistencia): todos los pines "arriba" (McpArribaChip/Pin) como salida a 5V y todos
/// los "abajo" (McpAbajoChip/Pin) como salida a 0V (config global una única vez), luego por cada
/// paso se selecciona la pista (P) y se leen las analógicas filtradas (F0..F3) para calcular R.
/// Fase B (cortocircuito): todo arriba+abajo a salida 0V (masa); por cada paso, su "arriba" pasa
/// a 5V y su "abajo" a entrada (alta impedancia), se selecciona la pista y se comprueba si hay
/// caída de tensión (fuga → cortocircuito real), restaurando el pin antes de continuar.
/// Fórmula de resistencia (función lineal): R = Pendiente * (Vain / (Ve - Vain) * 390) + Offset
/// </summary>
public class RunningState : ITestState
{
    private const float R_REF = 390f;   // Ohm
    private const float R_OPEN_THRESHOLD = 1000f; // Ohm

    // Umbral de tensión (V) por debajo del cual se considera "caída" (cortocircuito real) en la
    // Fase B, al leer con "arriba" excitado a 5V. Asunción (sin confirmar por hardware): mitad de
    // la tensión de excitación nominal. Ajustar aquí si se define un valor distinto.
    private const float CORTOCIRCUITO_VOLTAGE_THRESHOLD = 2.5f; // V

    // Tiempo de asentamiento tras cambiar el estado eléctrico (M/S) antes de leer, para que el
    // relé/mux y la carga del cableado se estabilicen (sin esto, la primera lectura tras la
    // configuración global daba R≈0 y falso Cortocircuito).
    private const int SETTLE_DELAY_MS = 300;

    public TestState StateId => TestState.Running;

    public async Task<TestState> ExecuteAsync(TestContext context)
    {
        var pasos = context.Parametros.OrderBy(p => p.NPasoEnsayo).ToList();
        int total = pasos.Count;
        int numMcps = context.Referencia.NumMcps;

        // Bits agregados de TODOS los pasos: "arriba" (excitación 5V) y "abajo" (referencia 0V).
        var arribaBits = new bool[Pc7866Commands.OutputCount];
        var abajoBits  = new bool[Pc7866Commands.OutputCount];
        foreach (var p in pasos)
        {
            int bArriba = Pc7866Commands.McpBitIndex(p.McpArribaChip, p.McpArribaPin);
            if (bArriba >= 0) arribaBits[bArriba] = true;
            int bAbajo = Pc7866Commands.McpBitIndex(p.McpAbajoChip, p.McpAbajoPin);
            if (bAbajo >= 0) abajoBits[bAbajo] = true;
        }

        // Solo los chips con al menos un pin "arriba"/"abajo" configurado reciben comandos M/S.
        var chipsEnUso = ChipsEnUso(arribaBits, abajoBits, numMcps);

        // ── Fase A: configuración global + medición de resistencia ─────────────────────────
        if (!await ConfigurarFaseResistenciaAsync(context, arribaBits, chipsEnUso))
        {
            context.Resultado.ResultadoGlobal = false;
            return TestState.Error;
        }
        await Task.Delay(SETTLE_DELAY_MS, context.CancellationToken);

        var detallesPorPaso = new Dictionary<int, ResultadoDetalle>();
        for (int i = 0; i < total; i++)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                context.Resultado.ResultadoGlobal = false;
                return TestState.Aborted;
            }

            var paso = pasos[i];
            context.Progress?.Report(new TestProgressReport
            {
                CurrentStep = i + 1,
                TotalSteps  = total,
                Message     = $"[Resistencia {i + 1}/{total}] {paso.NombreContacto}",
                State       = TestState.Running
            });

            detallesPorPaso[paso.Id] = await MedirResistenciaAsync(paso, context);
            await Task.Delay(150, context.CancellationToken);
        }

        // ── Fase B: configuración global de masa + verificación de cortocircuito ────────────
        if (!await ConfigurarFaseCortocircuitoBaseAsync(context, chipsEnUso))
        {
            context.Resultado.ResultadoGlobal = false;
            return TestState.Error;
        }
        await Task.Delay(SETTLE_DELAY_MS, context.CancellationToken);

        for (int i = 0; i < total; i++)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                context.Resultado.ResultadoGlobal = false;
                return TestState.Aborted;
            }

            var paso = pasos[i];
            context.Progress?.Report(new TestProgressReport
            {
                CurrentStep = i + 1,
                TotalSteps  = total,
                Message     = $"[Cortocircuito {i + 1}/{total}] {paso.NombreContacto}",
                State       = TestState.Running
            });

            var detalle = detallesPorPaso[paso.Id];
            if (await ComprobarCortocircuitoAsync(paso, context))
                detalle.Estado = EstadoMedicion.Cortocircuito;
            detalle.Resultado = detalle.Estado == EstadoMedicion.Ok;

            context.Resultado.Detalles.Add(detalle);
            context.RaiseStepCompleted(paso, detalle);

            await Task.Delay(150, context.CancellationToken);
        }

        // Desconectar mux y dejar en reposo (0V) solo los chips que se usaron durante el ensayo
        await SendLoggedAsync(context, Pc7866Commands.SelectTrack(0));
        foreach (int chip in chipsEnUso)
            await SendLoggedAsync(context, Pc7866Commands.BuildOutputCommand(chip, 0));

        context.Resultado.ResultadoGlobal = context.Resultado.Detalles.Count > 0 && context.Resultado.Detalles.All(d => d.Resultado);
        return TestState.Completed;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Fase A – resistencia
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Configura, una única vez, todos los pines "arriba" como salida a 5V y "abajo" como salida a 0V.</summary>
    private static async Task<bool> ConfigurarFaseResistenciaAsync(
        TestContext context, bool[] arriba, List<int> chipsEnUso)
    {
        if (!await AplicarDireccionSalidaAsync(context, chipsEnUso)) return false;

        foreach (int chip in chipsEnUso)
        {
            string cmd = Pc7866Commands.BuildOutputCommand(chip, WordFromBits(arriba, chip));
            string resp = await SendLoggedAsync(context, cmd);
            if (!resp.Trim().StartsWith("O", StringComparison.OrdinalIgnoreCase)) return false;
        }
        return true;
    }

    private static async Task<ResultadoDetalle> MedirResistenciaAsync(ParametroEnsayo paso, TestContext context)
    {
        var detalle = new ResultadoDetalle
        {
            ParametroEnsayoId = paso.Id,
            NombreContacto    = paso.NombreContacto,
            NPasoEnsayo       = paso.NPasoEnsayo,
            Timestamp         = DateTime.Now
        };

        try
        {
            // 1. Seleccionar la pista de medida (multiplexores)
            string respP = await SendLoggedAsync(context, Pc7866Commands.SelectTrack(paso.CanalMultiplexor));
            if (!respP.Trim().StartsWith("O", StringComparison.OrdinalIgnoreCase))
            {
                detalle.Estado = EstadoMedicion.Nok;
                detalle.Resultado = false;
                return detalle;
            }

            // Pequeña espera para que la señal se estabilice tras conmutar el mux
            await Task.Delay(50, context.CancellationToken);

            // 2. Leer las 4 analógicas filtradas (una trama F por canal)
            var analogicas = new float[4];
            for (int ch = 0; ch < 4; ch++)
            {
                string respF = await SendLoggedAsync(context, Pc7866Commands.ReadFiltered(ch));

                float? valor = context.Parser.ParseFilteredValue(respF);
                if (valor is null)
                {
                    detalle.Estado = EstadoMedicion.Nok;
                    detalle.Resultado = false;
                    return detalle;
                }
                analogicas[ch] = valor.Value;
            }

            float vain = analogicas[0] - analogicas[1];  // Ch0 - Ch1
            float ve   = analogicas[2] - analogicas[3];  // Ch2 - Ch3

            // 3. Calcular resistencia. Detección abierto/cortocircuito sobre la resistencia BRUTA
            // (igual que el modo manual), y solo sobre lecturas válidas se aplica la calibración
            // lineal R = Pendiente * R_bruta + Offset. Así una pendiente grande no dispara falsos
            // "abierto" por superar el umbral de 1000 Ω (bug: manual daba bien, auto daba infinito).
            // Pendiente=0 no es válida (anularía la medida): filas antiguas se tratan como Pendiente=1.
            float pendiente = paso.Pendiente == 0f ? 1f : paso.Pendiente;
            float resistencia = -1f;
            float denom = ve - vain;
            float rBruta = 0f;
            if (Math.Abs(denom) > 1e-6f)
            {
                rBruta = (vain / denom) * R_REF;
                if (rBruta > 0f && rBruta <= R_OPEN_THRESHOLD)
                    resistencia = pendiente * rBruta + paso.Offset;
            }
            context.CommandLogger?.Invoke(
                $"CALC {paso.NombreContacto}: Vain={vain:F4} Ve={ve:F4} denom={denom:F4} " +
                $"rBruta={rBruta:F4} Pendiente={pendiente} Offset={paso.Offset} R={resistencia:F4}");

            detalle.ResistenciaMedida = resistencia;

            // 4. Evaluar resultado: abierto → cortocircuito por umbral de R (mientras se confirma
            //    el criterio definitivo, coexiste con el test eléctrico real de la Fase B) → tolerancia
            if (resistencia < 0f)
            {
                detalle.Estado = EstadoMedicion.Abierto;
            }
            else if (paso.ResistenciaMinima > 0f && resistencia < paso.ResistenciaMinima)
            {
                detalle.Estado = EstadoMedicion.Cortocircuito;
            }
            else
            {
                float diferencia = Math.Abs(resistencia - paso.ResistenciaNominal);
                detalle.Estado = diferencia <= paso.Tolerancia ? EstadoMedicion.Ok : EstadoMedicion.Nok;
            }
            detalle.Resultado = detalle.Estado == EstadoMedicion.Ok;
        }
        catch (Exception ex)
        {
            detalle.Estado    = EstadoMedicion.Nok;
            detalle.Resultado = false;
            _ = ex; // registrado en nivel superior
        }

        return detalle;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Fase B – cortocircuito
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Configura, una única vez, todos los pines "arriba" y "abajo" como salida a 0V (todo a masa).</summary>
    private static async Task<bool> ConfigurarFaseCortocircuitoBaseAsync(TestContext context, List<int> chipsEnUso)
    {
        if (!await AplicarDireccionSalidaAsync(context, chipsEnUso)) return false;

        foreach (int chip in chipsEnUso)
        {
            string cmd = Pc7866Commands.BuildOutputCommand(chip, 0);
            string resp = await SendLoggedAsync(context, cmd);
            if (!resp.Trim().StartsWith("O", StringComparison.OrdinalIgnoreCase)) return false;
        }
        return true;
    }

    /// <summary>
    /// Excita el pin "arriba" del paso a 5V, pone su "abajo" en alta impedancia (entrada), lee la
    /// pista y determina si hay caída de tensión (cortocircuito real), restaurando el pin al final.
    /// </summary>
    private static async Task<bool> ComprobarCortocircuitoAsync(ParametroEnsayo paso, TestContext context)
    {
        int bitArriba = Pc7866Commands.McpBitIndex(paso.McpArribaChip, paso.McpArribaPin);
        int bitAbajo  = Pc7866Commands.McpBitIndex(paso.McpAbajoChip, paso.McpAbajoPin);
        if (bitArriba < 0 || bitAbajo < 0) return false; // paso sin selectores arriba/abajo configurados

        int chipArriba = bitArriba / Pc7866Commands.McpPinCount;
        int pinArriba  = bitArriba % Pc7866Commands.McpPinCount;
        int chipAbajo  = bitAbajo  / Pc7866Commands.McpPinCount;
        int pinAbajo   = bitAbajo  % Pc7866Commands.McpPinCount;

        bool cortocircuito = false;
        try
        {
            // 1. "Abajo" como entrada (alta impedancia)
            string respMIn = await SendLoggedAsync(context,
                Pc7866Commands.BuildMcpModeCommand(chipAbajo, asOutput: false, (ushort)(1 << pinAbajo)));

            // 2. "Arriba" a 5V
            string respS = await SendLoggedAsync(context,
                Pc7866Commands.BuildOutputCommand(chipArriba, (ushort)(1 << pinArriba)));

            if (respMIn.Trim().StartsWith("O", StringComparison.OrdinalIgnoreCase) &&
                respS.Trim().StartsWith("O", StringComparison.OrdinalIgnoreCase))
            {
                // Asentamiento tras excitar "arriba"/liberar "abajo" antes de conmutar el mux
                await Task.Delay(SETTLE_DELAY_MS, context.CancellationToken);

                // 3. Seleccionar pista y leer tensión
                await SendLoggedAsync(context, Pc7866Commands.SelectTrack(paso.CanalMultiplexor));
                await Task.Delay(50, context.CancellationToken);

                string respF = await SendLoggedAsync(context, Pc7866Commands.ReadFiltered(0));
                float? voltaje = context.Parser.ParseFilteredValue(respF);

                // Caída de tensión (o respuesta inválida) por debajo del umbral → cortocircuito real
                cortocircuito = voltaje is null || voltaje.Value < CORTOCIRCUITO_VOLTAGE_THRESHOLD;
            }
        }
        catch (Exception ex)
        {
            _ = ex; // no concluyente: se deja como no-cortocircuito, la Fase A ya cubre el fallo de comunicación
        }
        finally
        {
            // 4. Restaurar: "arriba" a 0V y "abajo" de nuevo como salida a 0V
            await SendLoggedAsync(context, Pc7866Commands.BuildOutputCommand(chipArriba, 0));
            await SendLoggedAsync(context,
                Pc7866Commands.BuildMcpModeCommand(chipAbajo, asOutput: true, (ushort)(1 << pinAbajo)));
            await SendLoggedAsync(context, Pc7866Commands.BuildOutputCommand(chipAbajo, 0));
        }

        return cortocircuito;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Determina qué chips MCP (0-based) tienen al menos un pin "arriba"/"abajo" configurado en algún paso.</summary>
    private static List<int> ChipsEnUso(bool[] arriba, bool[] abajo, int numMcps)
    {
        var chips = new List<int>();
        for (int chip = 0; chip < numMcps; chip++)
            if ((WordFromBits(arriba, chip) | WordFromBits(abajo, chip)) != 0)
                chips.Add(chip);
        return chips;
    }

    /// <summary>Configura como salida (M) los 16 pines completos de cada chip en uso.</summary>
    private static async Task<bool> AplicarDireccionSalidaAsync(TestContext context, List<int> chipsEnUso)
    {
        foreach (int chip in chipsEnUso)
        {
            string resp = await SendLoggedAsync(context,
                Pc7866Commands.BuildMcpModeCommand(chip, asOutput: true, 0xFFFF));
            if (!resp.Trim().StartsWith("O", StringComparison.OrdinalIgnoreCase)) return false;
        }
        return true;
    }

    /// <summary>Envía un comando y lo reporta vía <see cref="TestContext.CommandLogger"/> (TX/RX) para el log de la UI.</summary>
    private static async Task<string> SendLoggedAsync(TestContext context, string command)
    {
        context.CommandLogger?.Invoke($"TX: {command}");
        string response = await context.SerialPort.SendCommandAsync(command, context.TimeoutMs, context.CancellationToken);
        context.CommandLogger?.Invoke($"RX: {response.Trim()}");
        return response;
    }

    /// <summary>Extrae los 16 bits del chip indicado (0-based) de un array de <see cref="Pc7866Commands.OutputCount"/> bits.</summary>
    private static ushort WordFromBits(bool[] bits, int chip)
    {
        ushort w = 0;
        int baseIdx = chip * Pc7866Commands.McpPinCount;
        for (int i = 0; i < Pc7866Commands.McpPinCount; i++)
            if (bits[baseIdx + i]) w |= (ushort)(1 << i);
        return w;
    }
}

