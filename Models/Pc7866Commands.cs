namespace PC7866.Models;

/// <summary>
/// Comandos del protocolo de comunicación con el firmware ESP32 (ver definicion.md).
/// Trama: 1 byte de comando ASCII (mayúscula) + parámetros dependientes, terminada en &lt;CR&gt;&lt;LF&gt;.
/// </summary>
public static class Pc7866Commands
{
    // ── Códigos de comando ────────────────────────────────────────────────────
    public const char CmdDiagnosis     = 'D';
    public const char CmdMcpConfig     = 'M';
    public const char CmdOutputs       = 'S';
    public const char CmdSelectTrack   = 'P';
    public const char CmdReadRaw       = 'R';
    public const char CmdReadFiltered  = 'F';
    public const char CmdBoardConfig   = 'I';
    public const char CmdReset         = 'Q';

    // ── Respuestas ────────────────────────────────────────────────────────────
    public const char RespOk  = 'O';
    public const char RespNok = 'N';

    // ── Subcomandos Diagnosis (D) ─────────────────────────────────────────────
    public const char DiagFull        = 'T'; // DT – diagnosis completa
    public const char DiagAds         = '1'; // D1 – ADS1115, dirección 0x48
    public const char DiagMcpFirst    = '2'; // D2 – MCP 0x20 (consecutivos: D3=0x21 … D7=0x25)
    public const char DiagVersion     = 'V'; // DV – lectura de versión de compilación
    public const char DiagReadConfig  = 'G'; // DG – lee configuración I2C (misma cadena que el comando "I")
    public const char DiagTemperature = 'C'; // DC – lectura de temperatura

    /// <summary>Subcomando de diagnosis para el MCP de índice 0-5 (0x20-0x25) → 'D2'..'D7'.</summary>
    public static char DiagMcpSubCmd(int mcpChip0Based) => (char)(DiagMcpFirst + mcpChip0Based);

    // ── Modos del comando M (dirección de pines) ──────────────────────────────
    public const char McpModeInput  = 'E';
    public const char McpModeOutput = 'S';

    // ── Número de chips / pines MCP23017 ──────────────────────────────────────
    /// <summary>Número máximo de chips MCP23017 soportados por el protocolo (0x20 a 0x25).</summary>
    public const int McpChipCount = 6;

    /// <summary>Número de pines por chip MCP23017.</summary>
    public const int McpPinCount = 16;

    /// <summary>Número total de salidas gestionables (6 chips × 16 pines).</summary>
    public const int OutputCount = McpChipCount * McpPinCount;

    /// <summary>Número máximo de pista/track seleccionable con el comando P (0-48).</summary>
    public const int MaxTrackNumber = 48;

    /// <summary>Longitud fija de la cadena de referencia dentro del comando I.</summary>
    public const int BoardReferenceLength = 7;

    // ─────────────────────────────────────────────────────────────────────────
    // Builders de tramas
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Trama DT – Diagnosis total.</summary>
    public static string DiagnosisTotal() => $"{CmdDiagnosis}{DiagFull}";

    /// <summary>Trama Dx – Diagnosis individual (x = subcomando, p.ej. DiagAds, DiagMcpSubCmd(n), DiagVersion...).</summary>
    public static string DiagnosisSingle(char subCmd) => $"{CmdDiagnosis}{subCmd}";

    /// <summary>
    /// Trama M – Configura dirección (entrada/salida) de los pines de un MCP23017.
    /// </summary>
    /// <param name="mcpChip">Índice de chip 0-5 (0x20-0x25).</param>
    /// <param name="asOutput">true = configurar como salida ('S'); false = como entrada ('E').</param>
    /// <param name="mask">Máscara de 16 bits: 1 = aplicar el modo indicado a ese pin, 0 = no modificar.</param>
    public static string BuildMcpModeCommand(int mcpChip, bool asOutput, ushort mask)
    {
        ValidateMcpChip(mcpChip);
        char mode = asOutput ? McpModeOutput : McpModeInput;
        return $"{CmdMcpConfig}{McpChipDigit(mcpChip)}{mode}{mask:X4}";
    }

    /// <summary>
    /// Trama S – Modifica el estado (registro OLAT) de las salidas de un único MCP23017.
    /// </summary>
    /// <param name="mcpChip">Índice de chip 0-5 (0x20-0x25).</param>
    /// <param name="states">16 bits de estado a escribir (1 = HI, 0 = LO).</param>
    public static string BuildOutputCommand(int mcpChip, ushort states)
    {
        ValidateMcpChip(mcpChip);
        return $"{CmdOutputs}{McpChipDigit(mcpChip)}{states:X4}";
    }

    /// <summary>
    /// Construye una trama S por cada uno de los <paramref name="numMcps"/> chips configurados,
    /// a partir de un array de <see cref="OutputCount"/> bits (bit = chip*16 + pin).
    /// </summary>
    public static List<string> BuildOutputCommands(bool[] outputs, int numMcps)
    {
        if (outputs.Length != OutputCount)
            throw new ArgumentException($"Se esperan {OutputCount} salidas");

        numMcps = Math.Clamp(numMcps, 0, McpChipCount);
        var commands = new List<string>(numMcps);
        for (int chip = 0; chip < numMcps; chip++)
        {
            ushort word = BitsToWord(outputs, chip * McpPinCount);
            commands.Add(BuildOutputCommand(chip, word));
        }
        return commands;
    }

    /// <summary>Trama Pnn – Selección de pista en los multiplexores analógicos (0 = desconectado, 1-48).</summary>
    public static string SelectTrack(int trackNumber)
    {
        if (trackNumber < 0 || trackNumber > MaxTrackNumber)
            throw new ArgumentOutOfRangeException(nameof(trackNumber), $"Debe estar entre 0 y {MaxTrackNumber}");
        return $"{CmdSelectTrack}{trackNumber:D2}";
    }

    /// <summary>Trama Rn – Lectura RAW del canal ADS indicado (0-3).</summary>
    public static string ReadRaw(int channel)
    {
        ValidateAdsChannel(channel);
        return $"{CmdReadRaw}{channel}";
    }

    /// <summary>Trama Fn – Lectura filtrada (voltios) del canal ADS indicado (0-3).</summary>
    public static string ReadFiltered(int channel)
    {
        ValidateAdsChannel(channel);
        return $"{CmdReadFiltered}{channel}";
    }

    /// <summary>
    /// Trama I – Configuración de placa: número de MCP, posiciones de INH1-4, referencia,
    /// número de muestras para el promedio analógico y retardo (ms) antes de leer tras F/R.
    /// Se guarda automáticamente en NVS por el firmware al recibirse.
    /// </summary>
    /// <param name="numMcps">Número de MCP23017 participantes, consecutivos desde 0x20 (0-6).</param>
    /// <param name="inh1">Posición de pin (0-15) de INH1, o null para libre elección ('N').</param>
    /// <param name="inh2">Posición de pin (0-15) de INH2, o null para libre elección ('N').</param>
    /// <param name="inh3">Posición de pin (0-15) de INH3, o null para libre elección ('N').</param>
    /// <param name="inh4">Posición de pin (0-15) de INH4, o null para libre elección ('N').</param>
    /// <param name="reference">Referencia de placa, informativa; se trunca/rellena a <see cref="BoardReferenceLength"/> caracteres.</param>
    /// <param name="samples">Nº de muestras para el promedio analógico del canal 0 (0-99).</param>
    /// <param name="delayMs">Retardo en ms antes de iniciar la lectura tras un comando F o R (0-999).</param>
    public static string BuildBoardConfigCommand(
        int numMcps, int? inh1, int? inh2, int? inh3, int? inh4,
        string reference, int samples, int delayMs)
    {
        if (numMcps < 0 || numMcps > McpChipCount)
            throw new ArgumentOutOfRangeException(nameof(numMcps), $"Debe estar entre 0 y {McpChipCount}");

        string a    = numMcps.ToString();
        string b    = InhPositionToChar(inh1);
        string c    = InhPositionToChar(inh2);
        string d    = InhPositionToChar(inh3);
        string e    = InhPositionToChar(inh4);
        string reff = (reference ?? string.Empty).PadRight(BoardReferenceLength)[..BoardReferenceLength];
        string nn   = Math.Clamp(samples, 0, 99).ToString("D2");
        string ttt  = Math.Clamp(delayMs, 0, 999).ToString("D3");

        return $"{CmdBoardConfig}{a}{b}{c}{d}{e}{reff}{nn}{ttt}";
    }

    /// <summary>Trama Q – Reset (soft-reset) del microcontrolador.</summary>
    public static string Reset() => $"{CmdReset}";

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static void ValidateMcpChip(int mcpChip)
    {
        if (mcpChip < 0 || mcpChip >= McpChipCount)
            throw new ArgumentOutOfRangeException(nameof(mcpChip), $"Debe estar entre 0 y {McpChipCount - 1}");
    }

    private static void ValidateAdsChannel(int channel)
    {
        if (channel < 0 || channel > 3)
            throw new ArgumentOutOfRangeException(nameof(channel), "Canal ADS debe estar entre 0 y 3");
    }

    /// <summary>Dígito ASCII ('0'-'5') de dirección de chip usado en las tramas M/S/D.</summary>
    public static char McpChipDigit(int mcpChip) => (char)('0' + mcpChip);

    /// <summary>Dirección i2c real (0x20-0x25) del chip a partir de su índice 0-5.</summary>
    public static int McpI2cAddress(int mcpChip) => 0x20 + mcpChip;

    private static string InhPositionToChar(int? pos)
        => pos is null ? "N" : Math.Clamp(pos.Value, 0, 15).ToString("X1");

    private static ushort BitsToWord(bool[] bits, int offset)
    {
        ushort w = 0;
        for (int i = 0; i < 16; i++)
            if (bits[offset + i]) w |= (ushort)(1 << i);
        return w;
    }

    /// <summary>
    /// Calcula el índice de bit (0 a <see cref="OutputCount"/>-1) dentro del array de salidas a
    /// partir del chip MCP23017 (1-6, equivale a 0x20-0x25, convención de <see cref="ParametroEnsayo"/>)
    /// y el pin (0-15) dentro de ese chip. Devuelve -1 si el chip es 0 (no configurado) o los valores
    /// están fuera de rango.
    /// </summary>
    public static int McpBitIndex(int mcpChip, int mcpPin)
    {
        if (mcpChip < 1 || mcpChip > McpChipCount) return -1;
        if (mcpPin < 0 || mcpPin >= McpPinCount) return -1;
        return (mcpChip - 1) * McpPinCount + mcpPin;
    }
}
