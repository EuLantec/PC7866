using PC7866.Models;

namespace PC7866.Services.StateMachine.States;

/// <summary>
/// Estado: ejecuciÃ³n â€“ recorre los ParametrosEnsayo uno a uno,
/// activa las salidas, lee analÃ³gicas filtradas y calcula la resistencia.
/// FÃ³rmula: R = Vain / (Ve - Vain) * 390
/// </summary>
public class RunningState : ITestState
{
    private const float R_REF = 390f;   // Ohm
    private const string CMD_PREFIX_S = "S"; // Activar salidas
    private const string CMD_F        = "F"; // Leer filtradas

    public TestState StateId => TestState.Running;

    public async Task<TestState> ExecuteAsync(TestContext context)
    {
        var pasos = context.Parametros.OrderBy(p => p.NPasoEnsayo).ToList();
        int total = pasos.Count;
        bool anyFail = false;

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

            var detalle = await EjecutarPasoAsync(paso, context);
            context.Resultado.Detalles.Add(detalle);

            if (!detalle.Resultado) anyFail = true;

            context.RaiseStepCompleted(paso, detalle);

            // PequeÃ±a pausa entre pasos
            await Task.Delay(150, context.CancellationToken);
        }

        // Apagar todas las salidas al terminar
        await context.SerialPort.SendCommandAsync("S000000000000", context.TimeoutMs, context.CancellationToken);

        context.Resultado.ResultadoGlobal = !anyFail;
        return TestState.Completed;
    }

    private static async Task<ResultadoDetalle> EjecutarPasoAsync(
        ParametroEnsayo paso, TestContext context)
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
            // 1. Construir y enviar comando de activaciÃ³n de salidas
            string cmdSalidas = Pc7866Commands.BuildOutputsCommand(paso.NSalida);
            string respS = await context.SerialPort.SendCommandAsync(
                cmdSalidas, context.TimeoutMs, context.CancellationToken);

            if (!respS.Trim().StartsWith("O", StringComparison.OrdinalIgnoreCase))
            {
                detalle.Resultado = false;
                return detalle;
            }

            // PequeÃ±a espera para que la seÃ±al se estabilice
            await Task.Delay(50, context.CancellationToken);

            // 2. Leer analÃ³gicas filtradas
            string respF = await context.SerialPort.SendCommandAsync(
                CMD_F, context.TimeoutMs, context.CancellationToken);

            var analogicas = context.Parser.ParseAnalogValues(respF);
            if (analogicas is null || analogicas.Length < 4)
            {
                detalle.Resultado = false;
                return detalle;
            }

            float vain = analogicas[0] - analogicas[1];  // Ch1 - Ch2
            float ve   = analogicas[2] - analogicas[3];  // Ch3 - Ch4

            // 3. Calcular resistencia: R = Vain / (Ve - Vain) * 390
            float resistencia = 0f;
            float denom = ve - vain;
            if (Math.Abs(denom) > 1e-6f)
                resistencia = (vain / denom) * R_REF - paso.Offset;

            detalle.ResistenciaMedida = resistencia;

            // 4. Evaluar resultado
            float diferencia = Math.Abs(resistencia - paso.ResistenciaNominal);
            detalle.Resultado = diferencia <= paso.Tolerancia;
        }
        catch (Exception ex)
        {
            detalle.Resultado = false;
            _ = ex; // registrado en nivel superior
        }

        return detalle;
    }

}

