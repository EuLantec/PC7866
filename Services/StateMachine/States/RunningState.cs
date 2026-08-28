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
/// Fórmula de resistencia: R = Vain / (Ve - Vain) * 390 - Offset
/// </summary>
public class RunningState : ITestState
{
    private const float R_REF = 390f;   // Ohm
    private const float R_OPEN_THRESHOLD = 1000f; // Ohm

    // Umbral de tensión (V) por debajo del cual se considera "caída" (cortocircuito real) en la
    // Fase B, al leer con "arriba" excitado a 5V. Asunción (sin confirmar por hardware): mitad de
    // la tensión de excitación nominal. Ajustar aquí si se define un valor distinto.
    private const float CORTOCIRCUITO_VOLTAGE_THRESHOLD = 2.5f; // V

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

        // ── Fase A: configuración global + medición de resistencia ─────────────────────────
        if (!await ConfigurarFaseResistenciaAsync(context, arribaBits, abajoBits, numMcps))
        {
            context.Resultado.ResultadoGlobal = false;
            return TestState.Error;
        }

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
        if (!await ConfigurarFaseCortocircuitoBaseAsync(context, arribaBits, abajoBits, numMcps))
        {
            context.Resultado.ResultadoGlobal = false;
            return TestState.Error;
        }

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

        // Desconectar mux y dejar todas las salidas configuradas en reposo (0V)
        await context.SerialPort.SendCommandAsync(Pc7866Commands.SelectTrack(0), context.TimeoutMs, context.CancellationToken);
        foreach (string cmd in Pc7866Commands.BuildOutputCommands(new bool[Pc7866Commands.OutputCount], numMcps))
            await context.SerialPort.SendCommandAsync(cmd, context.TimeoutMs, context.CancellationToken);

        context.Resultado.ResultadoGlobal = context.Resultado.Detalles.Count > 0 && context.Resultado.Detalles.All(d => d.Resultado);
        return TestState.Completed;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Fase A – resistencia
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Configura, una única vez, todos los pines "arriba" como salida a 5V y "abajo" como salida a 0V.</summary>
    private static async Task<bool> ConfigurarFaseResistenciaAsync(
        TestContext context, bool[] arriba, bool[] abajo, int numMcps)
    {
        if (!await AplicarDireccionSalidaAsync(context, arriba, abajo, numMcps)) return false;

        foreach (string cmd in Pc7866Commands.BuildOutputCommands(arriba, numMcps))
        {
            string resp = await context.SerialPort.SendCommandAsync(cmd, context.TimeoutMs, context.CancellationToken);
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
            string respP = await context.SerialPort.SendCommandAsync(
                Pc7866Commands.SelectTrack(paso.CanalMultiplexor), context.TimeoutMs, context.CancellationToken);
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
                string respF = await context.SerialPort.SendCommandAsync(
                    Pc7866Commands.ReadFiltered(ch), context.TimeoutMs, context.CancellationToken);

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

            // 3. Calcular resistencia: R = Vain / (Ve - Vain) * 390 - Offset
            // Guardar -1 como indicador de abierta (infinito)
            float resistencia = -1f;
            float denom = ve - vain;
            if (Math.Abs(denom) > 1e-6f)
            {
                float rCalculada = (vain / denom) * R_REF - paso.Offset;
                resistencia = (rCalculada <= 0f || rCalculada > R_OPEN_THRESHOLD)
                    ? -1f
                    : rCalculada;
            }

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
    private static async Task<bool> ConfigurarFaseCortocircuitoBaseAsync(
        TestContext context, bool[] arriba, bool[] abajo, int numMcps)
    {
        if (!await AplicarDireccionSalidaAsync(context, arriba, abajo, numMcps)) return false;

        foreach (string cmd in Pc7866Commands.BuildOutputCommands(new bool[Pc7866Commands.OutputCount], numMcps))
        {
            string resp = await context.SerialPort.SendCommandAsync(cmd, context.TimeoutMs, context.CancellationToken);
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
            string respMIn = await context.SerialPort.SendCommandAsync(
                Pc7866Commands.BuildMcpModeCommand(chipAbajo, asOutput: false, (ushort)(1 << pinAbajo)),
                context.TimeoutMs, context.CancellationToken);

            // 2. "Arriba" a 5V
            string respS = await context.SerialPort.SendCommandAsync(
                Pc7866Commands.BuildOutputCommand(chipArriba, (ushort)(1 << pinArriba)),
                context.TimeoutMs, context.CancellationToken);

            if (respMIn.Trim().StartsWith("O", StringComparison.OrdinalIgnoreCase) &&
                respS.Trim().StartsWith("O", StringComparison.OrdinalIgnoreCase))
            {
                // 3. Seleccionar pista y leer tensión
                await context.SerialPort.SendCommandAsync(
                    Pc7866Commands.SelectTrack(paso.CanalMultiplexor), context.TimeoutMs, context.CancellationToken);
                await Task.Delay(50, context.CancellationToken);

                string respF = await context.SerialPort.SendCommandAsync(
                    Pc7866Commands.ReadFiltered(0), context.TimeoutMs, context.CancellationToken);
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
            await context.SerialPort.SendCommandAsync(
                Pc7866Commands.BuildOutputCommand(chipArriba, 0), context.TimeoutMs, context.CancellationToken);
            await context.SerialPort.SendCommandAsync(
                Pc7866Commands.BuildMcpModeCommand(chipAbajo, asOutput: true, (ushort)(1 << pinAbajo)),
                context.TimeoutMs, context.CancellationToken);
            await context.SerialPort.SendCommandAsync(
                Pc7866Commands.BuildOutputCommand(chipAbajo, 0), context.TimeoutMs, context.CancellationToken);
        }

        return cortocircuito;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Configura como salida (M) todos los bits marcados en "arriba" u "abajo", por cada MCP.</summary>
    private static async Task<bool> AplicarDireccionSalidaAsync(
        TestContext context, bool[] arriba, bool[] abajo, int numMcps)
    {
        for (int chip = 0; chip < numMcps; chip++)
        {
            ushort mask = (ushort)(WordFromBits(arriba, chip) | WordFromBits(abajo, chip));
            if (mask == 0) continue;

            string resp = await context.SerialPort.SendCommandAsync(
                Pc7866Commands.BuildMcpModeCommand(chip, asOutput: true, mask), context.TimeoutMs, context.CancellationToken);
            if (!resp.Trim().StartsWith("O", StringComparison.OrdinalIgnoreCase)) return false;
        }
        return true;
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

