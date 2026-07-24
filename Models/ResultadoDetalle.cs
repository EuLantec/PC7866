namespace PC7866.Models;

/// <summary>
/// Estado diagnóstico de una medición de resistencia.
/// </summary>
public enum EstadoMedicion
{
    /// <summary>Dentro de tolerancia.</summary>
    Ok,
    /// <summary>Fuera de tolerancia (pero no abierto ni cortocircuito).</summary>
    Nok,
    /// <summary>Resistencia medida por debajo del umbral mínimo definido.</summary>
    Cortocircuito,
    /// <summary>Circuito abierto (resistencia infinita / fuera de rango superior).</summary>
    Abierto
}

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

    /// <summary>Estado diagnóstico detallado (Ok, Nok, Cortocircuito, Abierto).</summary>
    public EstadoMedicion Estado     { get; set; } = EstadoMedicion.Nok;

    public DateTime Timestamp        { get; set; } = DateTime.Now;
}
