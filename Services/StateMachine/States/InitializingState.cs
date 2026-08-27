using PC7866.Models;

namespace PC7866.Services.StateMachine.States;

/// <summary>
/// Estado: inicialización – verifica que el puerto esté abierto, envía la configuración de
/// placa (comando "I") correspondiente a la Referencia seleccionada y prepara el resultado.
/// </summary>
public class InitializingState : ITestState
{
    public TestState StateId => TestState.Initializing;

    public async Task<TestState> ExecuteAsync(TestContext context)
    {
        context.Progress?.Report(new TestProgressReport
        {
            CurrentStep = 0,
            TotalSteps  = context.Parametros.Count,
            Message     = "Inicializando ensayo…",
            State       = TestState.Initializing
        });

        if (!context.SerialPort.IsOpen)
        {
            context.Resultado.ResultadoGlobal = false;
            return TestState.Error;
        }

        var referencia = context.Referencia;
        string cmdConfig = Pc7866Commands.BuildBoardConfigCommand(
            referencia.NumMcps, referencia.Inh1Pos, referencia.Inh2Pos, referencia.Inh3Pos, referencia.Inh4Pos,
            referencia.ReferenciaNombre, referencia.Muestras, referencia.RetardoMs);

        string respI = await context.SerialPort.SendCommandAsync(cmdConfig, context.TimeoutMs, context.CancellationToken);
        if (!respI.Trim().StartsWith("O", StringComparison.OrdinalIgnoreCase))
        {
            context.Resultado.ResultadoGlobal = false;
            return TestState.Error;
        }

        context.Resultado.FechaPrueba = DateTime.Now;
        context.Resultado.Detalles.Clear();

        return TestState.Running;
    }
}
