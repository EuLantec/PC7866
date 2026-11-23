using PC7866.Services.SerialCommunication;

namespace PC7866.Tests;

public class CommandParserTests
{
    private readonly CommandParser _parser = new();

    [Theory]
    [InlineData("2.5000", 2.5f)]
    [InlineData("  1.2500\r\n", 1.25f)]
    [InlineData("3.3000xyz", 3.3f)]   // ruido final
    public void ParseFilteredValue_RespuestasValidas(string respuesta, float esperado)
        => Assert.Equal(esperado, _parser.ParseFilteredValue(respuesta)!.Value, 4);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("N")]
    [InlineData("ERR")]
    public void ParseFilteredValue_RespuestasInvalidas_DevuelveNull(string respuesta)
        => Assert.Null(_parser.ParseFilteredValue(respuesta));

    [Theory]
    [InlineData("12345", 12345)]
    [InlineData("00042", 42)]
    [InlineData("00042ruido", 42)]
    public void ParseRawValue_RespuestasValidas(string respuesta, int esperado)
        => Assert.Equal(esperado, _parser.ParseRawValue(respuesta));

    [Theory]
    [InlineData("")]
    [InlineData("N")]
    public void ParseRawValue_RespuestasInvalidas_DevuelveNull(string respuesta)
        => Assert.Null(_parser.ParseRawValue(respuesta));
}
