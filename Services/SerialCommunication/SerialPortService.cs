using System.IO.Ports;
using System.Text;

namespace PC7866.Services.SerialCommunication;

/// <summary>
/// Implementación del servicio de comunicación serie
/// </summary>
public class SerialPortService : ISerialPortService
{
    // Algunas respuestas (p.ej. F/R) no terminan en <CR><LF>; si no llegan más datos
    // durante este tiempo se considera que esa respuesta (sin terminador) está completa.
    // El temporizador se reinicia con cada byte recibido, así que basta con cubrir el hueco entre
    // ráfagas del firmware (a 115200 baud una respuesta llega en <1 ms); 60 ms da margen de sobra
    // y evita el tiempo muerto de 150 ms que ralentizaba mucho el ensayo automático.
    private const int IdleCompletionMs = 0;

    private SerialPort? _serialPort;
    private readonly object _lock = new();
    private readonly object _bufferLock = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private TaskCompletionSource<string>? _responseTask;
    private readonly StringBuilder _receiveBuffer = new();
    private System.Threading.Timer? _idleTimer;
    private volatile bool _expectMultilineResponse;

    public bool IsOpen => _serialPort?.IsOpen ?? false;
    public string? CurrentPort => _serialPort?.PortName;

    public event EventHandler<string>? DataReceived;
    public event EventHandler<string>? ErrorOccurred;
    public event EventHandler? PortOpened;
    public event EventHandler? PortClosed;

    public string[] GetAvailablePorts()
    {
        return SerialPort.GetPortNames();
    }

    public bool Open(string portName, int baudRate = 115200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
    {
        lock (_lock)
        {
            try
            {
                if (IsOpen)
                {
                    Close();
                }

                _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
                {
                    ReadTimeout = 500,
                    WriteTimeout = 500,
                    Encoding = Encoding.ASCII,
                    NewLine = "\r\n",
                    DtrEnable = true,
                    RtsEnable = true
                };

                _serialPort.DataReceived += OnSerialDataReceived;
                _serialPort.ErrorReceived += OnSerialErrorReceived;

                _serialPort.Open();

                ClearBuffers();

                PortOpened?.Invoke(this, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Error opening port {portName}: {ex.Message}");
                return false;
            }
        }
    }

    public void Close()
    {
        lock (_lock)
        {
            try
            {
                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }

                    _serialPort.DataReceived -= OnSerialDataReceived;
                    _serialPort.ErrorReceived -= OnSerialErrorReceived;
                    _serialPort.Dispose();
                    _serialPort = null;
                }

                _idleTimer?.Dispose();
                _idleTimer = null;

                lock (_bufferLock)
                {
                    _receiveBuffer.Clear();
                }

                PortClosed?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Error closing port: {ex.Message}");
            }
        }
    }

    public async Task<string> SendCommandAsync(string command, int timeoutMs = 5000, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (!IsOpen || _serialPort == null)
            {
                throw new InvalidOperationException("Puerto serie no está abierto");
            }

            lock (_bufferLock)
            {
                _receiveBuffer.Clear();
            }
            // "DT" (diagnosis completa) es la única respuesta multilínea conocida: el resto de
            // comandos se da por completo en cuanto llega su primera línea.
            _expectMultilineResponse = command.Trim().Equals("DT", StringComparison.OrdinalIgnoreCase);
            _responseTask = new TaskCompletionSource<string>();

            await _serialPort.BaseStream.WriteAsync(Encoding.ASCII.GetBytes(command + "\r\n"), cancellationToken);
            await _serialPort.BaseStream.FlushAsync(cancellationToken);

            using var timeoutCts = new CancellationTokenSource(timeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var timeoutTask = Task.Delay(timeoutMs, linkedCts.Token);
            var responseTask = _responseTask.Task;

            var completedTask = await Task.WhenAny(responseTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                throw new TimeoutException($"Timeout esperando respuesta del comando: {command}");
            }

            return await responseTask;
        }
        finally
        {
            _responseTask = null;

            // Descarta cualquier byte residual (p.ej. una respuesta tardía tras un timeout)
            // para que no se mezcle con la respuesta del siguiente comando.
            _idleTimer?.Dispose();
            _idleTimer = null;
            lock (_bufferLock)
            {
                _receiveBuffer.Clear();
            }
            try { if (_serialPort?.IsOpen == true) _serialPort.DiscardInBuffer(); } catch { /* puerto ya cerrado */ }

            _semaphore.Release();
        }
    }

    public async Task SendDataAsync(string data, CancellationToken cancellationToken = default)
    {
        if (!IsOpen || _serialPort == null)
        {
            throw new InvalidOperationException("Puerto serie no está abierto");
        }

        await _serialPort.BaseStream.WriteAsync(Encoding.ASCII.GetBytes(data + "\r\n"), cancellationToken);
        await _serialPort.BaseStream.FlushAsync(cancellationToken);
    }

    public void ClearBuffers()
    {
        if (_serialPort?.IsOpen == true)
        {
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
        }

        _idleTimer?.Dispose();
        _idleTimer = null;

        lock (_bufferLock)
        {
            _receiveBuffer.Clear();
        }
    }

    private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            if (_serialPort == null || !_serialPort.IsOpen)
                return;

            string data = _serialPort.ReadExisting();

            lock (_bufferLock)
            {
                _receiveBuffer.Append(data);
                string bufferContent = _receiveBuffer.ToString();

                if (!_expectMultilineResponse && (bufferContent.Contains('\n') || bufferContent.Contains('\r')))
                {
                    // Completa en cuanto llega la primera línea: el firmware puede enviar bytes
                    // adicionales (ruido/estado) después que no forman parte de esta respuesta.
                    CompleteResponse(bufferContent);
                }
                else
                {
                    // Sin terminador todavía (p.ej. F/R) o respuesta multilínea (DT): se espera
                    // inactividad para dar la respuesta acumulada por completa.
                    _idleTimer?.Dispose();
                    _idleTimer = new System.Threading.Timer(_ => OnIdleTimeout(), null, IdleCompletionMs, System.Threading.Timeout.Infinite);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Error leyendo datos serie: {ex.Message}");
            _responseTask?.TrySetException(ex);
        }
    }

    /// <summary>
    /// Se dispara cuando no llegan más datos durante <see cref="IdleCompletionMs"/>: da por completa
    /// la respuesta acumulada aunque no contenga terminador CR/LF (caso de comandos F/R).
    /// </summary>
    private void OnIdleTimeout()
    {
        lock (_bufferLock)
        {
            if (_receiveBuffer.Length == 0)
                return;

            CompleteResponse(_receiveBuffer.ToString());
        }
    }

    /// <summary>Debe invocarse dentro de un lock sobre <see cref="_bufferLock"/>.</summary>
    private void CompleteResponse(string bufferContent)
    {
        string completeResponse = bufferContent.Trim();
        _receiveBuffer.Clear();
        _idleTimer?.Dispose();
        _idleTimer = null;

        DataReceived?.Invoke(this, completeResponse);

        _responseTask?.TrySetResult(completeResponse);
    }

    private void OnSerialErrorReceived(object sender, SerialErrorReceivedEventArgs e)
    {
        ErrorOccurred?.Invoke(this, $"Error en puerto serie: {e.EventType}");
    }

    public void Dispose()
    {
        Close();
        _semaphore?.Dispose();
        GC.SuppressFinalize(this);
    }
}
