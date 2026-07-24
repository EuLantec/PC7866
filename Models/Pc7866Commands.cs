namespace PC7866.Models;

/// <summary>
/// Comandos del protocolo PC7866 (case sensitive, siempre mayúsculas ASCII)
/// </summary>
public static class Pc7866Commands
{
    // ── Códigos de comando ────────────────────────────────────────────────────
    public const char CmdDiagnosis   = 'D';
    public const char CmdOutputs     = 'S';
    public const char CmdReadRaw     = 'R';
    public const char CmdReadFiltered = 'F';
    public const char CmdFilter      = 'I';
    public const char CmdSave        = 'G';
    public const char CmdReset       = 'Q';

    // ── Respuestas ────────────────────────────────────────────────────────────
    public const char RespOk  = 'O';
    public const char RespNok = 'N';

    // ── Subcomandos Diagnosis (D) ─────────────────────────────────────────────
    public const char DiagFull = 'T'; // DT – diagnosis completa
    public const char Diag1    = '1'; // D1 – MCP 0x20
    public const char Diag2    = '2'; // D2 – MCP 0x21
    public const char Diag3    = '3'; // D3 – MCP 0x22
    public const char Diag4    = '4'; // D4 – 0x48

    // ── Subcomandos Filter (I) ────────────────────────────────────────────────
    public const char FilterFlags  = '0'; // I0  – FLAGS
    public const char FilterCoef1  = '1'; // I1  – Coef 1
    public const char FilterCoef2  = '2'; // I2  – Coef 2
    public const char FilterCoef3  = '3'; // I3  – Coef 3
    public const char FilterCoef4  = '4'; // I4  – Coef 4
    public const char FilterCoef5  = '5'; // I5  – Coef 5
    public const char FilterCoef6  = '6'; // I6  – Coef 6
    public const char FilterCoef7  = '7'; // I7  – Coef 7
    public const char FilterCoef8  = '8'; // I8  – Coef 8
    public const char FilterCoef9  = '9'; // I9  – Coef 9
    public const char FilterCoef10 = ':'; // I:  – Coef 10 (ASCII 0x3A)

    /// <summary>Número de coeficientes de filtro (I1 – I10).</summary>
    public const int CoefCount = 10;

    /// <summary>Subcomando de coeficiente n (1-based). Devuelve el char ASCII correspondiente.</summary>
    public static char CoefSubCmd(int n) => (char)('0' + n); // '1'..':', n=1..10

    // ── Subcomandos Save (G) ──────────────────────────────────────────────────
    public const char SaveWrite    = 'G'; // GG – guardar en EEPROM
    public const char SaveRead     = 'L'; // GL – leer desde EEPROM
    public const char SaveView     = 'V'; // GV – visualizar valores RAM

    // ── Número de salidas ─────────────────────────────────────────────────────
    public const int OutputCount = 48;

    // ─────────────────────────────────────────────────────────────────────────
    // Builders de tramas
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Construye la trama S con 48 bits de salidas codificados como 12 hex ASCII.
    /// Las salidas se agrupan en 3 palabras de 16 bits: [47..32][31..16][15..0]
    /// </summary>
    public static string BuildOutputsCommand(bool[] outputs)
    {
        if (outputs.Length != OutputCount)
            throw new ArgumentException($"Se esperan {OutputCount} salidas");

        // Palabra 0 → salidas 0–15  (trama posición 8–11, la más a la derecha)
        // Palabra 1 → salidas 16–31 (trama posición 4–7)
        // Palabra 2 → salidas 32–47 (trama posición 0–3)
        ushort w0 = BitsToWord(outputs, 0);
        ushort w1 = BitsToWord(outputs, 16);
        ushort w2 = BitsToWord(outputs, 32);

        return $"S{w2:X4}{w1:X4}{w0:X4}";
    }

    /// <summary>Trama DT – Diagnosis total</summary>
    public static string DiagnosisTotal()      => $"{CmdDiagnosis}{DiagFull}";

    /// <summary>Trama Dx – Diagnosis individual (x = '1'..'4')</summary>
    public static string DiagnosisSingle(char subCmd) => $"{CmdDiagnosis}{subCmd}";

    /// <summary>Trama R – Lectura RAW</summary>
    public static string ReadRaw()             => $"{CmdReadRaw}";

    /// <summary>Trama F – Lectura filtrada</summary>
    public static string ReadFiltered()        => $"{CmdReadFiltered}";

    /// <summary>Trama Ix xxxx – Coeficiente o FLAGS</summary>
    public static string SetFilter(char subCmd, string hexValue) => $"{CmdFilter}{subCmd}{hexValue}";

    /// <summary>Trama GG / GL / GV</summary>
    public static string SaveCommand(char subCmd)  => $"{CmdSave}{subCmd}";

    /// <summary>Trama Q – Reset</summary>
    public static string Reset()              => $"{CmdReset}";

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static ushort BitsToWord(bool[] bits, int offset)
    {
        ushort w = 0;
        for (int i = 0; i < 16; i++)
            if (bits[offset + i]) w |= (ushort)(1 << i);
        return w;
    }

    /// <summary>
    /// Convierte un coeficiente float al formato hex de 5 dígitos ASCII que usa el protocolo.
    /// val = (uint16)(coef + 2.0) × 10000
    /// </summary>
    public static string CoefToHex(double coef)
    {
        uint val = (uint)((coef + 2.0) * 10000);
        return val.ToString("X4");
    }

    /// <summary>Convierte el hex de protocolo de vuelta a coeficiente</summary>
    public static double HexToCoef(string hex)
    {
        uint val = Convert.ToUInt32(hex, 16);
        return val / 10000.0 - 2.0;
    }

    /// <summary>Número de chips MCP23017 usados por la trama de salidas (0x20, 0x21, 0x22).</summary>
    public const int McpChipCount = 3;

    /// <summary>Número de pines por chip MCP23017.</summary>
    public const int McpPinCount = 16;

    /// <summary>
    /// Calcula el índice de bit (0-47) dentro de la trama S a partir del chip MCP23017
    /// (1 = 0x20, 2 = 0x21, 3 = 0x22) y el pin (0-15) dentro de ese chip.
    /// Devuelve -1 si el chip es 0 (no configurado) o los valores están fuera de rango.
    /// </summary>
    public static int McpBitIndex(int mcpChip, int mcpPin)
    {
        if (mcpChip < 1 || mcpChip > McpChipCount) return -1;
        if (mcpPin < 0 || mcpPin >= McpPinCount) return -1;
        return (mcpChip - 1) * McpPinCount + mcpPin;
    }
}
