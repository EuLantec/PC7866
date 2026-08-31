using PC7866.Models;
using PC7866.Services.SerialCommunication;

namespace PC7866.Services.StateMachine;

/// <summary>
/// Contexto compartido que fluye por todos los estados de la máquina de ensayo.
/// </summary>
public class TestContext
{
    public Referencia           Referencia        { get; set; } = null!;
    public List<ParametroEnsayo> Parametros       { get; set; } = new();
    public Resultado             Resultado        { get; set; } = null!;
    public ISerialPortService    SerialPort       { get; set; } = null!;
    public CommandParser         Parser           { get; set; } = null!;
    public int                   TimeoutMs        { get; set; } = 5000;
    public CancellationToken     CancellationToken { get; set; }
    public IProgress<TestProgressReport>? Progress { get; set; }

    /// <summary>Callback invocado con cada comando TX/RX enviado al microprocesador (para mostrarlo en el log de la UI).</summary>
    public Action<string>? CommandLogger { get; set; }

    /// <summary>
    /// Evento disparado tras cada medición para actualizar la UI con el resultado del paso.
    /// </summary>
    public event Action<ParametroEnsayo, ResultadoDetalle>? StepCompleted;

    internal void RaiseStepCompleted(ParametroEnsayo p, ResultadoDetalle d)
        => StepCompleted?.Invoke(p, d);
}

/// <summary>
/// Reporte de progreso del test
/// </summary>
public class TestProgressReport
{
    public int CurrentStep { get; set; }
    public int TotalSteps  { get; set; }
    public string Message  { get; set; } = string.Empty;
    public TestState State { get; set; }
    public bool? StepOk   { get; set; }
}

/// <summary>
/// Estados posibles de la máquina
/// </summary>
public enum TestState
{
    Idle,
    Initializing,
    Running,
    Completed,
    Error,
    Aborted
}

