namespace PC7866.Models;

/// <summary>
/// Resultado detallado de un paso individual dentro de un Resultado de ensayo.
/// </summary>
public class ResultadoDetalle
{
    public int   Id                  { get; set; }
    public int   ResultadoId         { get; set; }

    /// <summary>Referencia al ParametroEnsayo ejecutado.</summary>
    public int?  ParametroEnsayoId   { get; set; }

    /// <summary>Nombre del contacto para trazabilidad.</summary>
    public string NombreContacto     { get; set; } = string.Empty;

    /// <summary>Número de paso ejecutado.</summary>
    public int   NPasoEnsayo         { get; set; }

    /// <summary>Resistencia medida calculada en Ohmios.</summary>
    public float ResistenciaMedida   { get; set; }

    /// <summary>Valor RAW del canal analógico 1 (Vain).</summary>
    public int   ValorRawVain        { get; set; }

    /// <summary>Valor RAW del canal analógico 2 (Ve).</summary>
    public int   ValorRawVe          { get; set; }

    /// <summary>true = OK, false = NOK.</summary>
    public bool  Resultado           { get; set; }

    public DateTime Timestamp        { get; set; } = DateTime.Now;
}
