using Dapper;
using MySqlConnector;
using PC7866.Models;
using System.Text.Json;

namespace PC7866.Services.Database;

/// <summary>
/// ImplementaciÃ³n de acceso a MariaDB mediante Dapper.
/// Schema: referencias, parametros_ensayo, resultados, resultados_detalle.
/// </summary>
public class TestRepository : ITestRepository
{
    private readonly string _connectionString;

    public TestRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private MySqlConnection CreateConnection() => new(_connectionString);

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Referencias
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<IEnumerable<Referencia>> GetAllReferenciasAsync(bool soloActivas = false)
    {
        string sql = soloActivas
            ? "SELECT * FROM referencias WHERE b_activa = TRUE ORDER BY fecha_creacion DESC"
            : "SELECT * FROM referencias ORDER BY fecha_creacion DESC";

        using var conn = CreateConnection();
        var rows = await conn.QueryAsync(sql);
        return rows.Select(MapReferencia);
    }

    public async Task<Referencia?> GetReferenciaByIdAsync(int id)
    {
        const string sql = "SELECT * FROM referencias WHERE id = @Id";
        using var conn = CreateConnection();
        var row = await conn.QueryFirstOrDefaultAsync(sql, new { Id = id });
        return row is null ? null : MapReferencia(row);
    }

    public async Task<int> InsertReferenciaAsync(Referencia r)
    {
        const string sql = """
            INSERT INTO referencias
                (b_activa, referencia, descripcion, fecha_creacion, fecha_modificacion, imagen)
            VALUES
                (@BActiva, @Referencia, @Descripcion, @FechaCreacion, @FechaModificacion, @Imagen);
            SELECT LAST_INSERT_ID();
            """;

        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            r.BActiva,
            Referencia    = r.ReferenciaNombre,
            r.Descripcion,
            r.FechaCreacion,
            r.FechaModificacion,
            r.Imagen
        });
    }

    public async Task UpdateReferenciaAsync(Referencia r)
    {
        const string sql = """
            UPDATE referencias SET
                b_activa           = @BActiva,
                referencia         = @Referencia,
                descripcion        = @Descripcion,
                fecha_modificacion = @FechaModificacion,
                imagen             = @Imagen
            WHERE id = @Id
            """;

        using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new
        {
            r.Id,
            r.BActiva,
            Referencia         = r.ReferenciaNombre,
            r.Descripcion,
            r.FechaModificacion,
            r.Imagen
        });
    }

    public async Task SetReferenciaActivaAsync(int id, bool activa)
    {
        const string sql = "UPDATE referencias SET b_activa = @Activa, fecha_modificacion = NOW() WHERE id = @Id";
        using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { Id = id, Activa = activa });
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // ParametrosEnsayo
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<IEnumerable<ParametroEnsayo>> GetParametrosByReferenciaAsync(int referenciaId)
    {
        const string sql = """
            SELECT * FROM parametros_ensayo
            WHERE referencia_id = @ReferenciaId
            ORDER BY n_paso_ensayo
            """;

        using var conn = CreateConnection();
        var rows = await conn.QueryAsync(sql, new { ReferenciaId = referenciaId });
        return rows.Select(MapParametroEnsayo);
    }

    public async Task<ParametroEnsayo?> GetParametroByIdAsync(int id)
    {
        const string sql = "SELECT * FROM parametros_ensayo WHERE id = @Id";
        using var conn = CreateConnection();
        var row = await conn.QueryFirstOrDefaultAsync(sql, new { Id = id });
        return row is null ? null : MapParametroEnsayo(row);
    }

    public async Task<int> InsertParametroAsync(ParametroEnsayo p)
    {
        const string sql = """
            INSERT INTO parametros_ensayo
                (referencia_id, nombre_contacto, n_paso_ensayo, n_salida_json,
                 resistencia_nominal, tolerancia, offset_val,
                 fecha_creacion, fecha_modificacion, pos_x, pos_y)
            VALUES
                (@ReferenciaId, @NombreContacto, @NPasoEnsayo, @NSalidaJson,
                 @ResistenciaNominal, @Tolerancia, @Offset,
                 @FechaCreacion, @FechaModificacion, @PosX, @PosY);
            SELECT LAST_INSERT_ID();
            """;

        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            p.ReferenciaId,
            p.NombreContacto,
            p.NPasoEnsayo,
            NSalidaJson        = JsonSerializer.Serialize(p.NSalida),
            p.ResistenciaNominal,
            p.Tolerancia,
            p.Offset,
            p.FechaCreacion,
            p.FechaModificacion,
            p.PosX,
            p.PosY
        });
    }

    public async Task UpdateParametroAsync(ParametroEnsayo p)
    {
        const string sql = """
            UPDATE parametros_ensayo SET
                nombre_contacto    = @NombreContacto,
                n_paso_ensayo      = @NPasoEnsayo,
                n_salida_json      = @NSalidaJson,
                resistencia_nominal= @ResistenciaNominal,
                tolerancia         = @Tolerancia,
                offset_val         = @Offset,
                fecha_modificacion = @FechaModificacion,
                pos_x              = @PosX,
                pos_y              = @PosY
            WHERE id = @Id
            """;

        using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new
        {
            p.Id,
            p.NombreContacto,
            p.NPasoEnsayo,
            NSalidaJson        = JsonSerializer.Serialize(p.NSalida),
            p.ResistenciaNominal,
            p.Tolerancia,
            p.Offset,
            p.FechaModificacion,
            p.PosX,
            p.PosY
        });
    }

    public async Task DeleteParametroAsync(int id)
    {
        const string sql = "DELETE FROM parametros_ensayo WHERE id = @Id";
        using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { Id = id });
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Resultados
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<IEnumerable<Resultado>> GetAllResultadosAsync()
    {
        const string sql = "SELECT * FROM resultados ORDER BY fecha_prueba DESC";
        using var conn = CreateConnection();
        var rows = await conn.QueryAsync(sql);
        return rows.Select(MapResultado);
    }

    public async Task<IEnumerable<Resultado>> GetResultadosByReferenciaAsync(int referenciaId)
    {
        const string sql = """
            SELECT * FROM resultados
            WHERE referencia_id = @ReferenciaId
            ORDER BY fecha_prueba DESC
            """;

        using var conn = CreateConnection();
        var rows = await conn.QueryAsync(sql, new { ReferenciaId = referenciaId });
        return rows.Select(MapResultado);
    }

    public async Task<Resultado?> GetResultadoByIdAsync(int id)
    {
        const string sql = "SELECT * FROM resultados WHERE id = @Id";
        using var conn = CreateConnection();
        var row = await conn.QueryFirstOrDefaultAsync(sql, new { Id = id });
        return row is null ? null : MapResultado(row);
    }

    public async Task<int> InsertResultadoAsync(Resultado r)
    {
        const string sql = """
            INSERT INTO resultados
                (referencia_id, fecha_prueba, resultado, operario, lote)
            VALUES
                (@ReferenciaId, @FechaPrueba, @ResultadoGlobal, @Operario, @Lote);
            SELECT LAST_INSERT_ID();
            """;

        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            r.ReferenciaId,
            r.FechaPrueba,
            r.ResultadoGlobal,
            r.Operario,
            r.Lote
        });
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // ResultadosDetalle
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<IEnumerable<ResultadoDetalle>> GetDetallesByResultadoAsync(int resultadoId)
    {
        const string sql = """
            SELECT * FROM resultados_detalle
            WHERE resultado_id = @ResultadoId
            ORDER BY n_paso_ensayo
            """;

        using var conn = CreateConnection();
        var rows = await conn.QueryAsync(sql, new { ResultadoId = resultadoId });
        return rows.Select(MapResultadoDetalle);
    }

    public async Task InsertDetalleAsync(ResultadoDetalle d)
    {
        const string sql = """
            INSERT INTO resultados_detalle
                (resultado_id, parametro_ensayo_id, nombre_contacto, n_paso_ensayo,
                 resistencia_medida, valor_raw_vain, valor_raw_ve, resultado, timestamp_medicion)
            VALUES
                (@ResultadoId, @ParametroEnsayoId, @NombreContacto, @NPasoEnsayo,
                 @ResistenciaMedida, @ValorRawVain, @ValorRawVe, @Resultado, @Timestamp)
            """;

        using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new
        {
            d.ResultadoId,
            d.ParametroEnsayoId,
            d.NombreContacto,
            d.NPasoEnsayo,
            d.ResistenciaMedida,
            d.ValorRawVain,
            d.ValorRawVe,
            d.Resultado,
            d.Timestamp
        });
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Utilidades
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<bool> TestConnectionAsync()
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        return true;
    }

    public async Task InitializeDatabaseAsync()
    {
        const string sqlReferencias = """
            CREATE TABLE IF NOT EXISTS referencias (
                id                  INT AUTO_INCREMENT PRIMARY KEY,
                b_activa            BOOLEAN  NOT NULL DEFAULT TRUE,
                referencia          VARCHAR(255) NOT NULL UNIQUE,
                descripcion         TEXT,
                fecha_creacion      DATETIME NOT NULL,
                fecha_modificacion  DATETIME NOT NULL,
                imagen              LONGBLOB
            );
            """;

        const string sqlParametros = """
            CREATE TABLE IF NOT EXISTS parametros_ensayo (
                id                  INT AUTO_INCREMENT PRIMARY KEY,
                referencia_id       INT  NOT NULL,
                nombre_contacto     VARCHAR(20)  NOT NULL,
                n_paso_ensayo       INT  NOT NULL,
                n_salida_json       LONGTEXT,
                resistencia_nominal FLOAT NOT NULL DEFAULT 0,
                tolerancia          FLOAT NOT NULL DEFAULT 0,
                offset_val          FLOAT NOT NULL DEFAULT 0,
                fecha_creacion      DATETIME NOT NULL,
                fecha_modificacion  DATETIME NOT NULL,
                pos_x               INT NOT NULL DEFAULT 0,
                pos_y               INT NOT NULL DEFAULT 0,
                FOREIGN KEY (referencia_id) REFERENCES referencias(id)
            );
            """;

        const string sqlResultados = """
            CREATE TABLE IF NOT EXISTS resultados (
                id              INT AUTO_INCREMENT PRIMARY KEY,
                referencia_id   INT  NOT NULL,
                fecha_prueba    DATETIME NOT NULL,
                resultado       BOOLEAN  NOT NULL,
                operario        VARCHAR(100),
                lote            VARCHAR(100),
                FOREIGN KEY (referencia_id) REFERENCES referencias(id)
            );
            """;

        const string sqlDetalle = """
            CREATE TABLE IF NOT EXISTS resultados_detalle (
                id                  INT AUTO_INCREMENT PRIMARY KEY,
                resultado_id        INT  NOT NULL,
                parametro_ensayo_id INT  NOT NULL,
                nombre_contacto     VARCHAR(20),
                n_paso_ensayo       INT  NOT NULL,
                resistencia_medida  FLOAT NOT NULL,
                valor_raw_vain      INT   NOT NULL DEFAULT 0,
                valor_raw_ve        INT   NOT NULL DEFAULT 0,
                resultado           BOOLEAN NOT NULL,
                timestamp_medicion  DATETIME NOT NULL,
                FOREIGN KEY (resultado_id) REFERENCES resultados(id)
            );
            """;

        using var conn = CreateConnection();
        await conn.ExecuteAsync(sqlReferencias);
        await conn.ExecuteAsync(sqlParametros);
        await conn.ExecuteAsync(sqlResultados);
        await conn.ExecuteAsync(sqlDetalle);

        // Migraciones: hacer nullable referencia_id y parametro_ensayo_id
        await conn.ExecuteAsync("ALTER TABLE resultados MODIFY COLUMN referencia_id INT NULL;");
        await conn.ExecuteAsync("ALTER TABLE resultados_detalle MODIFY COLUMN parametro_ensayo_id INT NULL;");
    }

    public void Dispose() { }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Mappers privados
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private static Referencia MapReferencia(dynamic row) => new()
    {
        Id                 = (int)row.id,
        BActiva            = (bool)row.b_activa,
        ReferenciaNombre   = (string)row.referencia,
        Descripcion        = (string?)row.descripcion ?? string.Empty,
        FechaCreacion      = (DateTime)row.fecha_creacion,
        FechaModificacion  = (DateTime)row.fecha_modificacion,
        Imagen             = (byte[]?)row.imagen
    };

    private static ParametroEnsayo MapParametroEnsayo(dynamic row)
    {
        bool[] salidas = new bool[48];
        string? json = (string?)row.n_salida_json;
        if (!string.IsNullOrEmpty(json))
            salidas = JsonSerializer.Deserialize<bool[]>(json) ?? salidas;

        return new ParametroEnsayo
        {
            Id                 = (int)row.id,
            ReferenciaId       = (int)row.referencia_id,
            NombreContacto     = (string)row.nombre_contacto,
            NPasoEnsayo        = (int)row.n_paso_ensayo,
            NSalida            = salidas,
            ResistenciaNominal = (float)row.resistencia_nominal,
            Tolerancia         = (float)row.tolerancia,
            Offset             = (float)row.offset_val,
            FechaCreacion      = (DateTime)row.fecha_creacion,
            FechaModificacion  = (DateTime)row.fecha_modificacion,
            PosX               = (int)row.pos_x,
            PosY               = (int)row.pos_y
        };
    }

    private static Resultado MapResultado(dynamic row) => new()
    {
        Id              = (int)row.id,
        ReferenciaId    = row.referencia_id == null ? (int?)null : (int)row.referencia_id,
        FechaPrueba     = (DateTime)row.fecha_prueba,
        ResultadoGlobal = (bool)row.resultado,
        Operario        = (string?)row.operario ?? string.Empty,
        Lote            = (string?)row.lote ?? string.Empty
    };

    private static ResultadoDetalle MapResultadoDetalle(dynamic row) => new()
    {
        Id                = (int)row.id,
        ResultadoId       = (int)row.resultado_id,
        ParametroEnsayoId = row.parametro_ensayo_id == null ? (int?)null : (int)row.parametro_ensayo_id,
        NombreContacto    = (string?)row.nombre_contacto ?? string.Empty,
        NPasoEnsayo       = (int)row.n_paso_ensayo,
        ResistenciaMedida = (float)row.resistencia_medida,
        ValorRawVain      = (int)row.valor_raw_vain,
        ValorRawVe        = (int)row.valor_raw_ve,
        Resultado         = (bool)row.resultado,
        Timestamp         = (DateTime)row.timestamp_medicion
    };
}
