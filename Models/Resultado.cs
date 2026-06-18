namespace PC7866.Models;

/// <summary>
/// Resultado global de un ensayo sobre una Referencia.
/// </summary>
public class Resultado
{
    public int      Id           { get; set; }
    public int?     ReferenciaId { get; set; }
    public DateTime FechaPrueba  { get; set; } = DateTime.Now;

    /// <summary>true = BUENO, false = MALO.</summary>
    public bool     ResultadoGlobal { get; set; }

    public string   Operario     { get; set; } = string.Empty;
    public string   Lote         { get; set; } = string.Empty;

    /// <summary>Detalles por paso de ensayo (cargados bajo demanda).</summary>
    public List<ResultadoDetalle> Detalles { get; set; } = new();
}
