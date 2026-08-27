namespace PC7866.Models;

/// <summary>
/// Referencia de producto a ensayar.
/// Cada vez que se modifican los parámetros se crea una nueva Referencia (versionado histórico).
/// </summary>
public class Referencia
{
    public int    Id                { get; set; }
    public bool   BActiva           { get; set; } = true;
    public string ReferenciaNombre  { get; set; } = string.Empty;
    public string Descripcion       { get; set; } = string.Empty;
    public DateTime FechaCreacion   { get; set; } = DateTime.Now;
    public DateTime FechaModificacion { get; set; } = DateTime.Now;

    /// <summary>Imagen en bytes (BLOB). Puede ser null si no se ha configurado.</summary>
    public byte[]? Imagen           { get; set; }

    // ── Configuración de placa (comando "I", ver definicion.md) ────────────────

    /// <summary>Número de MCP23017 participantes, consecutivos desde 0x20 (0-6).</summary>
    public int  NumMcps  { get; set; } = 6;

    /// <summary>Posición de pin (0-15) de INH1 en el MCP 0x20, o null para libre elección ('N').</summary>
    public int? Inh1Pos  { get; set; }

    /// <summary>Posición de pin (0-15) de INH2, o null para libre elección ('N').</summary>
    public int? Inh2Pos  { get; set; }

    /// <summary>Posición de pin (0-15) de INH3, o null para libre elección ('N').</summary>
    public int? Inh3Pos  { get; set; }

    /// <summary>Posición de pin (0-15) de INH4, o null para libre elección ('N').</summary>
    public int? Inh4Pos  { get; set; }

    /// <summary>Nº de muestras para el promedio analógico del canal 0 (0-99).</summary>
    public int  Muestras { get; set; } = 10;

    /// <summary>Retardo en ms antes de iniciar la lectura analógica tras un comando F o R.</summary>
    public int  RetardoMs { get; set; } = 20;

    /// <summary>Parámetros de ensayo asociados (cargados bajo demanda).</summary>
    public List<ParametroEnsayo> Parametros { get; set; } = new();

    public override string ToString() => ReferenciaNombre;
}
