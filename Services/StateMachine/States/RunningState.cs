using PC7866.Models;

namespace PC7866.Services.StateMachine.States;

/// <summary>
/// Estado: ejecución – recorre los ParametrosEnsayo uno a uno, selecciona la pista de medida
/// (comando P), activa las salidas (comando S por cada MCP configurado), lee las analógicas
/// filtradas (F0..F3) y calcula la resistencia.
/// Fórmula: R = Vain / (Ve - Vain) * 390
/// </summary>
public class RunningState : ITestState
{
    private const float R_REF = 390f;   // Ohm
    private const float R_OPEN_THRESHOLD = 1000f; // Ohm

    public TestState StateId => TestState.Running;

    public async Task<TestState> ExecuteAsync(TestContext context)
    {
        var pasos = context.Parametros.OrderBy(p => p.NPasoEnsayo).ToList();
        int total = pasos.Count;
        bool anyFail = false;
        int numMcps = context.Referencia.NumMcps;

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

            var detalle = await EjecutarPasoAsync(paso, context, numMcps);
            context.Resultado.Detalles.Add(detalle);

            if (!detalle.Resultado) anyFail = true;

            context.RaiseStepCompleted(paso, detalle);

            // Pequeña pausa entre pasos
            await Task.Delay(150, context.CancellationToken);
        }

        // Desconectar mux y apagar todas las salidas configuradas al terminar
        await context.SerialPort.SendCommandAsync(Pc7866Commands.SelectTrack(0), context.TimeoutMs, context.CancellationToken);
        foreach (string cmd in Pc7866Commands.BuildOutputCommands(new bool[Pc7866Commands.OutputCount], numMcps))
            await context.SerialPort.SendCommandAsync(cmd, context.TimeoutMs, context.CancellationToken);

        context.Resultado.ResultadoGlobal = !anyFail;
        return TestState.Completed;
    }

    private static async Task<ResultadoDetalle> EjecutarPasoAsync(
        ParametroEnsayo paso, TestContext context, int numMcps)
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
                detalle.Resultado = false;
                return detalle;
            }

            // 2. Activar salidas (una trama S por cada MCP configurado)
            foreach (string cmdSalidas in Pc7866Commands.BuildOutputCommands(paso.NSalida, numMcps))
            {
                string respS = await context.SerialPort.SendCommandAsync(
                    cmdSalidas, context.TimeoutMs, context.CancellationToken);

                if (!respS.Trim().StartsWith("O", StringComparison.OrdinalIgnoreCase))
                {
                    detalle.Resultado = false;
                    return detalle;
                }
            }

            // Pequeña espera para que la señal se estabilice
            await Task.Delay(50, context.CancellationToken);

            // 3. Leer las 4 analógicas filtradas (una trama F por canal)
            var analogicas = new float[4];
            for (int ch = 0; ch < 4; ch++)
            {
                string respF = await context.SerialPort.SendCommandAsync(
                    Pc7866Commands.ReadFiltered(ch), context.TimeoutMs, context.CancellationToken);

                float? valor = context.Parser.ParseFilteredValue(respF);
                if (valor is null)
                {
                    detalle.Resultado = false;
                    return detalle;
                }
                analogicas[ch] = valor.Value;
            }

            float vain = analogicas[0] - analogicas[1];  // Ch0 - Ch1
            float ve   = analogicas[2] - analogicas[3];  // Ch2 - Ch3

            // 4. Calcular resistencia: R = Vain / (Ve - Vain) * 390
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

            // 5. Evaluar resultado: abierto → cortocircuito (< umbral mínimo) → tolerancia
            if (resistencia < 0f)
            {
                detalle.Estado    = EstadoMedicion.Abierto;
                detalle.Resultado = false;
            }
            else if (paso.ResistenciaMinima > 0f && resistencia < paso.ResistenciaMinima)
            {
                detalle.Estado    = EstadoMedicion.Cortocircuito;
                detalle.Resultado = false;
            }
            else
            {
                float diferencia = Math.Abs(resistencia - paso.ResistenciaNominal);
                bool ok = diferencia <= paso.Tolerancia;
                detalle.Estado    = ok ? EstadoMedicion.Ok : EstadoMedicion.Nok;
                detalle.Resultado = ok;
            }
        }
        catch (Exception ex)
        {
            detalle.Estado     = EstadoMedicion.Nok;
            detalle.Resultado  = false;
            _ = ex; // registrado en nivel superior
        }

        return detalle;
    }

}

