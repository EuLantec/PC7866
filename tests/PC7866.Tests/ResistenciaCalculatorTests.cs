using PC7866.Models;
using PC7866.Services.Measurement;

namespace PC7866.Tests;

public class ResistenciaCalculatorTests
{
    // Con Ve = 5 y Vain = 1: R = 1 / 4 × 390 = 97,5 Ω.
    [Fact]
    public void CalcularBruta_FormulaBasica()
    {
        double? r = ResistenciaCalculator.CalcularBruta(vain: 1, ve: 5, umbralAbierto: 1000);
        Assert.NotNull(r);
        Assert.Equal(97.5, r!.Value, 6);
    }

    [Theory]
    [InlineData(1, 1)]    // denominador 0
    [InlineData(2, 1)]    // denominador negativo (nodo flotando)
    [InlineData(0, 5)]    // R = 0
    [InlineData(-1, 5)]   // R negativa
    public void CalcularBruta_EntradasInvalidas_DevuelveAbierto(double vain, double ve)
        => Assert.Null(ResistenciaCalculator.CalcularBruta(vain, ve, 1000));

    // Vain = 4, Ve = 5 → R = 4 / 1 × 390 = 1560 Ω.
    [Fact]
    public void CalcularBruta_SuperaUmbral_DevuelveAbierto()
        => Assert.Null(ResistenciaCalculator.CalcularBruta(4, 5, umbralAbierto: 1000));

    [Fact]
    public void CalcularBruta_ConUmbralDelModeloMayor_NoEsAbierto()
    {
        double? r = ResistenciaCalculator.CalcularBruta(4, 5, umbralAbierto: 5000);
        Assert.Equal(1560, r!.Value, 6);
    }

    [Fact]
    public void CalcularBruta_ExactamenteEnElUmbral_NoEsAbierto()
        => Assert.NotNull(ResistenciaCalculator.CalcularBruta(4, 5, umbralAbierto: 1560));

    [Theory]
    [InlineData(5000, 5000)]
    [InlineData(0, 1000)]
    [InlineData(-3, 1000)]
    public void UmbralAbierto_UsaModeloOValorPorDefecto(double modelo, double esperado)
        => Assert.Equal(esperado, ResistenciaCalculator.UmbralAbierto(modelo));

    [Theory]
    [InlineData(100, 1, 0, 100)]
    [InlineData(100, 2, 5, 205)]
    [InlineData(100, 0, 5, 105)]   // pendiente 0 se trata como 1
    [InlineData(100, 1, -10, 90)]
    public void Calibrar_AplicaPendienteYOffset(double r, double pendiente, double offset, double esperado)
        => Assert.Equal(esperado, ResistenciaCalculator.Calibrar(r, pendiente, offset), 6);
}

public class ClasificacionTests
{
    [Fact]
    public void SinResistencia_EsAbierto()
        => Assert.Equal(EstadoMedicion.Abierto, ResistenciaCalculator.Clasificar(null, 0, 100, 5));

    [Fact]
    public void PorDebajoDeLaMinima_EsCortocircuito()
        => Assert.Equal(EstadoMedicion.Cortocircuito, ResistenciaCalculator.Clasificar(8, minima: 10, 100, 5));

    [Fact]
    public void MinimaCero_DesactivaLaComprobacionDeCortocircuito()
        => Assert.Equal(EstadoMedicion.Nok, ResistenciaCalculator.Clasificar(8, minima: 0, 100, 5));

    [Theory]
    [InlineData(100, EstadoMedicion.Ok)]
    [InlineData(105, EstadoMedicion.Ok)]   // justo en el borde de tolerancia
    [InlineData(95, EstadoMedicion.Ok)]
    [InlineData(105.1, EstadoMedicion.Nok)]
    [InlineData(94.9, EstadoMedicion.Nok)]
    public void DentroOFueraDeTolerancia(double r, EstadoMedicion esperado)
        => Assert.Equal(esperado, ResistenciaCalculator.Clasificar(r, minima: 0, nominal: 100, tolerancia: 5));
}
