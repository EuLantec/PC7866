using PC7866.Models;
using PC7866.Services.SerialCommunication;

namespace PC7866.Tests;

/// <summary>
/// Puerto serie simulado: registra los comandos enviados y responde "O" a M/S/P.
/// F0 consume una cola de lecturas; F1..F3 devuelven valores fijos (por defecto 0 V, 5 V, 0 V).
/// </summary>
public sealed class FakeSerialPort : ISerialPortService
{
    private readonly Queue<string> _f0 = new();
    private readonly string[] _fijos = { "0.0000", "5.0000", "0.0000" };

    public List<string> Sent { get; } = new();

    /// <summary>Respuesta a los comandos M (por defecto "O").</summary>
    public string ModeResponse { get; set; } = "O";

    public FakeSerialPort(params string[] lecturasF0)
    {
        foreach (var l in lecturasF0) _f0.Enqueue(l);
    }

    public int Count(string command) => Sent.Count(c => c == command);

    public bool IsOpen => true;
    public string? CurrentPort => "FAKE";
    public event EventHandler<string>? DataReceived { add { } remove { } }
    public event EventHandler<string>? ErrorOccurred { add { } remove { } }
    public event EventHandler? PortOpened { add { } remove { } }
    public event EventHandler? PortClosed { add { } remove { } }

    public string[] GetAvailablePorts() => new[] { "FAKE" };
    public bool Open(string portName, int baudRate = 9600, int dataBits = 8,
        System.IO.Ports.Parity parity = System.IO.Ports.Parity.None,
        System.IO.Ports.StopBits stopBits = System.IO.Ports.StopBits.One) => true;
    public void Close() { }
    public void ClearBuffers() { }
    public Task SendDataAsync(string data, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Dispose() { }

    public Task<string> SendCommandAsync(string command, int timeoutMs = 5000, CancellationToken cancellationToken = default)
    {
        Sent.Add(command);
        string resp = command[0] switch
        {
            'M' => ModeResponse,
            'S' or 'P' => "O",
            'F' when command == "F0" => _f0.Count > 0 ? _f0.Dequeue() : string.Empty,
            'F' => _fijos[command[1] - '1'],
            _ => "O"
        };
        return Task.FromResult(resp);
    }
}
