using PC7866.Models;
using PC7866.Services.SerialCommunication;
using PC7866.Services.StateMachine.States;

namespace PC7866.Services.StateMachine;

/// <summary>
/// MÃ¡quina de estados que gestiona la ejecuciÃ³n automÃ¡tica de un ensayo resistivo.
/// </summary>
public class TestStateMachine
{
    private readonly Dictionary<TestState, ITestState> _states;
    private TestState _currentState = TestState.Idle;

    public TestState CurrentState => _currentState;
    public event EventHandler<TestState>? StateChanged;

    public TestStateMachine()
    {
        _states = new Dictionary<TestState, ITestState>
        {
            { TestState.Initializing, new InitializingState() },
            { TestState.Running,      new RunningState()      },
            { TestState.Completed,    new CompletedState()    }
        };
    }

    /// <summary>
    /// Ejecuta el ensayo completo sobre la referencia dada y devuelve el resultado.
    /// </summary>
    public async Task<Resultado> RunAsync(
        Referencia referencia,
        List<ParametroEnsayo> parametros,
        string operario,
        string lote,
        ISerialPortService serialPort,
        CommandParser parser,
        IProgress<TestProgressReport>? progress = null,
        Action<ParametroEnsayo, ResultadoDetalle>? stepCompleted = null,
        int timeoutMs = 5000,
        CancellationToken cancellationToken = default,
        Action<string>? commandLogger = null)
    {
        var resultado = new Resultado
        {
            ReferenciaId    = referencia.Id,
            FechaPrueba     = DateTime.Now,
            Operario        = operario,
            Lote            = lote,
            ResultadoGlobal = false
        };

        var context = new TestContext
        {
            Referencia        = referencia,
            Parametros        = parametros,
            Resultado         = resultado,
            SerialPort        = serialPort,
            Parser            = parser,
            TimeoutMs         = timeoutMs,
            CancellationToken = cancellationToken,
            Progress          = progress,
            CommandLogger     = commandLogger
        };

        if (stepCompleted is not null)
            context.StepCompleted += stepCompleted;

        _currentState = TestState.Initializing;
        StateChanged?.Invoke(this, _currentState);

        while (_currentState != TestState.Idle    &&
               _currentState != TestState.Error   &&
               _currentState != TestState.Aborted)
        {
            if (!_states.TryGetValue(_currentState, out var state)) break;
            _currentState = await state.ExecuteAsync(context);
            StateChanged?.Invoke(this, _currentState);
        }

        return resultado;
    }
}
