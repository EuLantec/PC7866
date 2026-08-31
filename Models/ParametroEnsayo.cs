namespace PC7866.Models;

/// <summary>
/// Parámetros de un paso individual de ensayo asociado a una Referencia.
/// </summary>
public class ParametroEnsayo
{
    public int      Id               { get; set; }
    public int      ReferenciaId     { get; set; }

    /// <summary>Nombre del contacto a ensayar (p.ej. "C1", "PE", "L1-L2").</summary>
    public string   NombreContacto   { get; set; } = string.Empty;

    /// <summary>Número de paso dentro de la secuencia de ensayo.</summary>
    public int      NPasoEnsayo      { get; set; }

    /// <summary>Array de <see cref="Pc7866Commands.OutputCount"/> (96) booleanos que indican qué salidas de los MCP23017 se activan (bit = chip*16 + pin).</summary>
    public bool[]   NSalida          { get; set; } = new bool[Pc7866Commands.OutputCount];

    /// <summary>Resistencia nominal esperada en Ohmios.</summary>
    public float    ResistenciaNominal { get; set; }

    /// <summary>Tolerancia aceptada en Ohmios.</summary>
    public float    Tolerancia       { get; set; }

    /// <summary>Offset a restar a la resistencia medida.</summary>
    public float    Offset           { get; set; }

    /// <summary>Umbral mínimo de resistencia (Ω); por debajo se considera cortocircuito.</summary>
    public float    ResistenciaMinima { get; set; }

    /// <summary>Chip MCP23017 (0-5, equivale a 0x20-0x25) del selector "aguas arriba". -1 = no configurado.</summary>
    public int      McpArribaChip    { get; set; } = -1;

    /// <summary>Pin (1-16) dentro del chip MCP23017 del selector "aguas arriba".</summary>
    public int      McpArribaPin     { get; set; }

    /// <summary>Chip MCP23017 (0-5) del selector "aguas abajo". -1 = no configurado.</summary>
    public int      McpAbajoChip     { get; set; } = -1;

    /// <summary>Pin (1-16) dentro del chip MCP23017 del selector "aguas abajo".</summary>
    public int      McpAbajoPin      { get; set; }

    /// <summary>
    /// Número de pista (0-48) enviado con el comando "P" para conectar el punto común de
    /// medida (multiplexores 74HC4067) a este contacto.
    /// </summary>
    public int      CanalMultiplexor { get; set; }

    public DateTime FechaCreacion    { get; set; } = DateTime.Now;
    public DateTime FechaModificacion { get; set; } = DateTime.Now;

    /// <summary>Posición X del indicador visual sobre la imagen.</summary>
    public int PosX { get; set; }

    /// <summary>Posición Y del indicador visual sobre la imagen.</summary>
    public int PosY { get; set; }
}
