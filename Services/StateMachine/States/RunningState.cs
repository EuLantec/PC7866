using PC7866.Models;

namespace PC7866.Services.StateMachine.States;

/// <summary>
/// Estado: ejecución – ensayo punto a punto sobre los ParametrosEnsayo ordenados.
/// Al inicio TODOS los MCP de la placa (se usen o no) se ponen como salida a 0V. Luego, por cada
/// paso: se pone su "arriba" (McpArribaChip/Pin) a 5V y su "abajo" (McpAbajoChip/Pin) a 0V, se
/// selecciona la pista (P) y se leen las analógicas filtradas (F0..F3) para calcular R; a
/// continuación su "abajo" pasa a entrada (alta impedancia) y se comprueba la caída de tensión
/// (cortocircuito real); finalmente se restaura el paso (arriba a 0V, abajo de nuevo como salida a
/// 0V) antes de pasar al siguiente.
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

        // 1. Poner TODOS los MCP de la placa (se usen o no) como salida a 0V.
        if (!await InicializarPlacaAsync(context, numMcps))
        {
            context.Resultado.ResultadoGlobal = false;
            return TestState.Error;
        }

        // 2. Punto a punto: por cada paso, medir resistencia y luego comprobar cortocircuito.
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
                Message     = $"[{i + 1}/{total}] {paso.NombreContacto}",
                State       = TestState.Running
            });

            var detalle = await MedirPasoAsync(paso, context);
            context.Resultado.Detalles.Add(detalle);
            context.RaiseStepCompleted(paso, detalle);

            await Task.Delay(150, context.CancellationToken);
        }

        // 3. Desconectar mux y dejar la placa en reposo (0V).
        await SendLoggedAsync(context, Pc7866Commands.SelectTrack(0));
        for (int chip = 0; chip < numMcps; chip++)
            await SendLoggedAsync(context, Pc7866Commands.BuildOutputCommand(chip, 0));

        context.Resultado.ResultadoGlobal = context.Resultado.Detalles.Count > 0 && context.Resultado.Detalles.All(d => d.Resultado);
        return TestState.Completed;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Configuración inicial
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Configura TODOS los MCP de la placa (se usen o no) como salida y los pone a 0V.</summary>
    private static async Task<bool> InicializarPlacaAsync(TestContext context, int numMcps)
    {
        for (int chip = 0; chip < numMcps; chip++)
        {
            string respM = await SendLoggedAsync(context,
                Pc7866Commands.BuildMcpModeCommand(chip, asOutput: true, 0xFFFF));
            if (!respM.Trim().StartsWith("O", StringComparison.OrdinalIgnoreCase)) return false;

            string respS = await SendLoggedAsync(context, Pc7866Commands.BuildOutputCommand(chip, 0));
            if (!respS.Trim().StartsWith("O", StringComparison.OrdinalIgnoreCase)) return false;
        }
        await Task.Delay(SETTLE_DELAY_MS, context.CancellationToken);
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Medición de un paso: resistencia + cortocircuito
    // ─────────────────────────────────────────────────────────────────────────

    private static async Task<ResultadoDetalle> MedirPasoAsync(ParametroEnsayo paso, TestContext context)
    {
        var detalle = new ResultadoDetalle
        {
            ParametroEnsayoId = paso.Id,
            NombreContacto    = paso.NombreContacto,
            NPasoEnsayo       = paso.NPasoEnsayo,
            Timestamp         = DateTime.Now
        };

        int bitArriba = Pc7866Commands.McpBitIndex(paso.McpArribaChip, paso.McpArribaPin);
        int bitAbajo  = Pc7866Commands.McpBitIndex(paso.McpAbajoChip, paso.McpAbajoPin);
        if (bitArriba < 0 || bitAbajo < 0) // paso sin selectores arriba/abajo configurados
        {
            detalle.Estado = EstadoMedicion.Nok;
            detalle.Resultado = false;
            return detalle;
        }

        int chipArriba = bitArriba / Pc7866Commands.McpPinCount;
        int pinArriba  = bitArriba % Pc7866Commands.McpPinCount;
        int chipAbajo  = bitAbajo  / Pc7866Commands.McpPinCount;
        int pinAbajo   = bitAbajo  % Pc7866Commands.McpPinCount;

        try
        {
            // ── Resistencia: "arriba" a 5V ("abajo" ya está a 0V), seleccionar pista y leer F0..F3 ──
            await SendLoggedAsync(context, Pc7866Commands.BuildOutputCommand(chipArriba, (ushort)(1 << pinArriba)));
            await Task.Delay(SETTLE_DELAY_MS, context.CancellationToken);

            string respP = await SendLoggedAsync(context, Pc7866Commands.SelectTrack(paso.CanalMultiplexor));
            if (!respP.Trim().StartsWith("O", StringComparison.OrdinalIgnoreCase))
            {
                detalle.Estado = EstadoMedicion.Nok;
                detalle.Resultado = false;
                return detalle;
            }
            await Task.Delay(50, context.CancellationToken);

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

            // Detección abierto/cortocircuito sobre la resistencia BRUTA (igual que el modo manual),
            // y solo a lecturas válidas se aplica la calibración lineal R = Pendiente * R_bruta + Offset.
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
            if (resistencia < 0f)
                detalle.Estado = EstadoMedicion.Abierto;
            else if (paso.ResistenciaMinima > 0f && resistencia < paso.ResistenciaMinima)
                detalle.Estado = EstadoMedicion.Cortocircuito;
            else
            {
                float diferencia = Math.Abs(resistencia - paso.ResistenciaNominal);
                detalle.Estado = diferencia <= paso.Tolerancia ? EstadoMedicion.Ok : EstadoMedicion.Nok;
            }

            // ── Cortocircuito: "abajo" pasa a entrada (alta impedancia), "arriba" sigue a 5V ──
            string respMIn = await SendLoggedAsync(context,
                Pc7866Commands.BuildMcpModeCommand(chipAbajo, asOutput: false, (ushort)(1 << pinAbajo)));
            if (respMIn.Trim().StartsWith("O", StringComparison.OrdinalIgnoreCase))
            {
                await Task.Delay(SETTLE_DELAY_MS, context.CancellationToken);
                await SendLoggedAsync(context, Pc7866Commands.SelectTrack(paso.CanalMultiplexor));
                await Task.Delay(50, context.CancellationToken);

                string respF = await SendLoggedAsync(context, Pc7866Commands.ReadFiltered(0));
                float? voltaje = context.Parser.ParseFilteredValue(respF);

                // Caída de tensión (o respuesta inválida) por debajo del umbral → cortocircuito real
                if (voltaje is null || voltaje.Value < CORTOCIRCUITO_VOLTAGE_THRESHOLD)
                    detalle.Estado = EstadoMedicion.Cortocircuito;
            }

            detalle.Resultado = detalle.Estado == EstadoMedicion.Ok;
        }
        catch (Exception ex)
        {
            detalle.Estado    = EstadoMedicion.Nok;
            detalle.Resultado = false;
            _ = ex; // registrado en nivel superior
        }
        finally
        {
            // ── Restaurar el paso: "arriba" a 0V, "abajo" de nuevo como salida a 0V ──
            await SendLoggedAsync(context, Pc7866Commands.BuildOutputCommand(chipArriba, 0));
            await SendLoggedAsync(context,
                Pc7866Commands.BuildMcpModeCommand(chipAbajo, asOutput: true, (ushort)(1 << pinAbajo)));
            await SendLoggedAsync(context, Pc7866Commands.BuildOutputCommand(chipAbajo, 0));
        }

        return detalle;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Envía un comando y lo reporta vía <see cref="TestContext.CommandLogger"/> (TX/RX) para el log de la UI.</summary>
    private static async Task<string> SendLoggedAsync(TestContext context, string command)
    {
        context.CommandLogger?.Invoke($"TX: {command}");
        string response = await context.SerialPort.SendCommandAsync(command, context.TimeoutMs, context.CancellationToken);
        context.CommandLogger?.Invoke($"RX: {response.Trim()}");
        return response;
    }
}

