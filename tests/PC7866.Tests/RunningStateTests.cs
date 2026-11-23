using PC7866.Models;
using PC7866.Services.SerialCommunication;
using PC7866.Services.StateMachine;
using PC7866.Services.StateMachine.States;

namespace PC7866.Tests;

/// <summary>
/// Ensayo automático (RunningState) con un puerto serie simulado. Con F1=0 V, F2=5 V y F3=0 V,
/// R_bruta = F0 / (5 − F0) × 390: F0=1 → 97,5 Ω; F0=4 → 1560 Ω; F0=0,5 → 43,33 Ω.
/// El orden de lecturas F0 por paso es: resistencia, luego cortocircuito.
/// </summary>
public class RunningStateTests
{
    private static ParametroEnsayo Paso(int n, float nominal = 97.5f, float tol = 5f, float minima = 0f) => new()
    {
        Id = n, NPasoEnsayo = n, NombreContacto = $"C{n}",
        ResistenciaNominal = nominal, Tolerancia = tol, ResistenciaMinima = minima,
        McpArribaChip = 0, McpArribaPin = 1, McpAbajoChip = 1, McpAbajoPin = 1,
        CanalMultiplexor = n
    };

    private static TestContext Contexto(FakeSerialPort puerto, float umbralModelo, params ParametroEnsayo[] pasos) => new()
    {
        Referencia = new Referencia { NumMcps = 2, ResistenciaCortocircuito = umbralModelo },
        Parametros = pasos.ToList(),
        Resultado = new Resultado(),
        SerialPort = puerto,
        Parser = new CommandParser(),
        TimeoutMs = 1000
    };

    private static Task<TestState> Ejecutar(TestContext ctx) => new RunningState().ExecuteAsync(ctx);

    [Fact]
    public async Task ResistenciaCorrecta_SinCortocircuito_EsOk()
    {
        var puerto = new FakeSerialPort("1.0", "4.9");
        var ctx = Contexto(puerto, 1000f, Paso(1));

        var estado = await Ejecutar(ctx);

        var d = Assert.Single(ctx.Resultado.Detalles);
        Assert.Equal(TestState.Completed, estado);
        Assert.Equal(EstadoMedicion.Ok, d.Estado);
        Assert.Equal(97.5f, d.ResistenciaMedida, 2);
        Assert.True(ctx.Resultado.ResultadoGlobal);
    }

    [Fact]
    public async Task UmbralDelModelo_PorDebajoDeLaResistencia_EsAbierto()
    {
        // 1560 Ω con umbral de modelo 1000 → infinito.
        var puerto = new FakeSerialPort("4.0", "4.99");
        var ctx = Contexto(puerto, 1000f, Paso(1, nominal: 1560f, tol: 10f));

        await Ejecutar(ctx);

        var d = Assert.Single(ctx.Resultado.Detalles);
        Assert.Equal(EstadoMedicion.Abierto, d.Estado);
        Assert.Equal(-1f, d.ResistenciaMedida);
        Assert.False(ctx.Resultado.ResultadoGlobal);
    }

    [Fact]
    public async Task UmbralDelModelo_PorEncimaDeLaResistencia_SeMideElValorReal()
    {
        // Mismo caso con umbral de modelo 5000 → se mide 1560 Ω (el cambio pedido por el cliente).
        var puerto = new FakeSerialPort("4.0", "4.99");
        var ctx = Contexto(puerto, 5000f, Paso(1, nominal: 1560f, tol: 10f));

        await Ejecutar(ctx);

        var d = Assert.Single(ctx.Resultado.Detalles);
        Assert.Equal(1560f, d.ResistenciaMedida, 1);
        Assert.Equal(EstadoMedicion.Ok, d.Estado);
    }

    [Fact]
    public async Task UmbralDelModeloCero_UsaMilOhmiosPorDefecto()
    {
        var puerto = new FakeSerialPort("4.0", "4.99");
        var ctx = Contexto(puerto, 0f, Paso(1, nominal: 1560f, tol: 10f));

        await Ejecutar(ctx);

        Assert.Equal(EstadoMedicion.Abierto, ctx.Resultado.Detalles[0].Estado);
    }

    [Fact]
    public async Task FaseCortocircuito_ResistenciaBajoElUmbral_EsCortocircuito()
    {
        var puerto = new FakeSerialPort("1.0", "0.5"); // R corto ≈ 43,3 Ω < 1000
        var ctx = Contexto(puerto, 1000f, Paso(1));

        await Ejecutar(ctx);

        var d = Assert.Single(ctx.Resultado.Detalles);
        Assert.Equal(EstadoMedicion.Cortocircuito, d.Estado);
        Assert.Equal(43.33f, d.ResistenciaCortocircuito, 1);
        Assert.False(ctx.Resultado.ResultadoGlobal);
    }

    [Fact]
    public async Task FaseCortocircuito_UmbralCero_DesactivaLaComprobacion()
    {
        var puerto = new FakeSerialPort("1.0", "0.5");
        var ctx = Contexto(puerto, 0f, Paso(1));

        await Ejecutar(ctx);

        Assert.Equal(EstadoMedicion.Ok, ctx.Resultado.Detalles[0].Estado);
    }

    [Fact]
    public async Task F1F2F3_SeLeenUnaSolaVezYF0EnCadaFase()
    {
        var puerto = new FakeSerialPort("1.0", "4.9", "1.0", "4.9");
        var ctx = Contexto(puerto, 1000f, Paso(1), Paso(2));

        await Ejecutar(ctx);

        Assert.Equal(2, ctx.Resultado.Detalles.Count);
        Assert.Equal(1, puerto.Count("F1"));
        Assert.Equal(1, puerto.Count("F2"));
        Assert.Equal(1, puerto.Count("F3"));
        Assert.Equal(4, puerto.Count("F0"));
    }

    [Fact]
    public async Task FalloDeInicializacion_TerminaEnError()
    {
        var puerto = new FakeSerialPort { ModeResponse = "N" };
        var ctx = Contexto(puerto, 1000f, Paso(1));

        var estado = await Ejecutar(ctx);

        Assert.Equal(TestState.Error, estado);
        Assert.Empty(ctx.Resultado.Detalles);
        Assert.False(ctx.Resultado.ResultadoGlobal);
    }

    [Fact]
    public async Task LecturaF0Invalida_PasoNok()
    {
        var puerto = new FakeSerialPort("");
        var ctx = Contexto(puerto, 1000f, Paso(1));

        var estado = await Ejecutar(ctx);

        Assert.Equal(TestState.Completed, estado);
        Assert.Equal(EstadoMedicion.Nok, ctx.Resultado.Detalles[0].Estado);
        Assert.False(ctx.Resultado.ResultadoGlobal);
    }

    [Fact]
    public async Task LecturaF0InvalidaEnFaseCortocircuito_EsCortocircuito()
    {
        var puerto = new FakeSerialPort("1.0", "");
        var ctx = Contexto(puerto, 1000f, Paso(1));

        await Ejecutar(ctx);

        Assert.Equal(EstadoMedicion.Cortocircuito, ctx.Resultado.Detalles[0].Estado);
    }

    [Fact]
    public async Task CanceladoDuranteElEnsayo_TerminaEnAbortedSinMedirMasPasos()
    {
        using var cts = new CancellationTokenSource();
        var puerto = new FakeSerialPort("1.0", "4.9", "1.0", "4.9");
        var ctx = Contexto(puerto, 1000f, Paso(1), Paso(2));
        ctx.CancellationToken = cts.Token;
        ctx.StepCompleted += (_, _) => cts.Cancel();

        var estado = await Ejecutar(ctx);

        Assert.Equal(TestState.Aborted, estado);
        Assert.Single(ctx.Resultado.Detalles);
        Assert.False(ctx.Resultado.ResultadoGlobal);
    }

    [Fact]
    public async Task PasoSinSelectores_EsNok()
    {
        var paso = Paso(1);
        paso.McpArribaChip = -1;
        var puerto = new FakeSerialPort();
        var ctx = Contexto(puerto, 1000f, paso);

        await Ejecutar(ctx);

        Assert.Equal(EstadoMedicion.Nok, ctx.Resultado.Detalles[0].Estado);
    }

    [Fact]
    public async Task TrasCadaPaso_SeRestauranLosPines()
    {
        var puerto = new FakeSerialPort("1.0", "4.9");
        var ctx = Contexto(puerto, 1000f, Paso(1));

        await Ejecutar(ctx);

        // Excitación: arriba (chip 0, pin 1) a 5 V; fase corto: abajo (chip 1) como entrada.
        Assert.Contains("S00001", puerto.Sent);
        Assert.Contains("M1E0001", puerto.Sent);
        // Restauración: arriba a 0 V, abajo otra vez como salida a 0 V (y todo en reposo al final).
        int i = puerto.Sent.IndexOf("M1E0001");
        var resto = puerto.Sent.Skip(i).ToList();
        Assert.Contains("S00000", resto);
        Assert.Contains("M1S0001", resto);
        Assert.Contains("S10000", resto);
        Assert.Equal("P00", puerto.Sent.First(c => c == "P00"));
    }
}
