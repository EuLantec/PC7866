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

    /// <summary>Array de 48 booleanos que indican qué salidas se activan.</summary>
    public bool[]   NSalida          { get; set; } = new bool[48];

    /// <summary>Resistencia nominal esperada en Ohmios.</summary>
    public float    ResistenciaNominal { get; set; }

    /// <summary>Tolerancia aceptada en Ohmios.</summary>
    public float    Tolerancia       { get; set; }

    /// <summary>Offset a restar a la resistencia medida.</summary>
    public float    Offset           { get; set; }

    /// <summary>Umbral mínimo de resistencia (Ω); por debajo se considera cortocircuito.</summary>
    public float    ResistenciaMinima { get; set; }

    /// <summary>Chip MCP23017 (1-3, equivale a 0x20/0x21/0x22) del selector "aguas arriba". 0 = no configurado.</summary>
    public int      McpArribaChip    { get; set; }

    /// <summary>Pin (0-15) dentro del chip MCP23017 del selector "aguas arriba".</summary>
    public int      McpArribaPin     { get; set; }

    /// <summary>Chip MCP23017 (1-3) del selector "aguas abajo". 0 = no configurado.</summary>
    public int      McpAbajoChip     { get; set; }

    /// <summary>Pin (0-15) dentro del chip MCP23017 del selector "aguas abajo".</summary>
    public int      McpAbajoPin      { get; set; }

    /// <summary>
    /// Canal del multiplexor usado para conectar el punto común de medida a este contacto.
    /// Almacenado como referencia/documentación de cableado; su codificación en bits de
    /// salida aún no está definida en el protocolo (ver cambio-proyecto.md).
    /// </summary>
    public int      CanalMultiplexor { get; set; }

    public DateTime FechaCreacion    { get; set; } = DateTime.Now;
    public DateTime FechaModificacion { get; set; } = DateTime.Now;

    /// <summary>Posición X del indicador visual sobre la imagen.</summary>
    public int PosX { get; set; }

    /// <summary>Posición Y del indicador visual sobre la imagen.</summary>
    public int PosY { get; set; }
}
