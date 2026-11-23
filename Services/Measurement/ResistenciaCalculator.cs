using PC7866.Models;

namespace PC7866.Services.Measurement;

/// <summary>Cálculo y clasificación de la resistencia de un contacto (lógica pura, sin hardware).</summary>
public static class ResistenciaCalculator
{
    public const double RRef = 390.0;               // Ohm
    public const double UmbralAbiertoPorDefecto = 1000.0; // Ohm
    public const double DenomEpsilon = 1e-9;

    /// <summary>Umbral de abierto: R cortocircuito del modelo, o el valor por defecto si no está definido (≤ 0).</summary>
    public static double UmbralAbierto(double resistenciaCortocircuitoModelo)
        => resistenciaCortocircuitoModelo > 0 ? resistenciaCortocircuitoModelo : UmbralAbiertoPorDefecto;

    /// <summary>
    /// R bruta = Vain / (Ve − Vain) × 390. Devuelve null (abierto, ∞) si el denominador no es
    /// positivo o si R ≤ 0 o R &gt; umbralAbierto.
    /// </summary>
    public static double? CalcularBruta(double vain, double ve, double umbralAbierto)
    {
        double denom = ve - vain;
        if (denom <= DenomEpsilon) return null;
        double r = vain / denom * RRef;
        return (r > 0 && r <= umbralAbierto) ? r : null;
    }

    /// <summary>Calibración lineal R = Pendiente × R_bruta + Offset (Pendiente 0 se trata como 1).</summary>
    public static double Calibrar(double rBruta, double pendiente, double offset)
        => (pendiente == 0 ? 1 : pendiente) * rBruta + offset;

    /// <summary>Clasifica una resistencia ya calibrada; null significa abierto.</summary>
    public static EstadoMedicion Clasificar(double? resistencia, double minima, double nominal, double tolerancia)
    {
        if (resistencia is null) return EstadoMedicion.Abierto;
        if (minima > 0 && resistencia < minima) return EstadoMedicion.Cortocircuito;
        return Math.Abs(resistencia.Value - nominal) <= tolerancia ? EstadoMedicion.Ok : EstadoMedicion.Nok;
    }
}
