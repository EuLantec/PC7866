using Dapper;
using MySqlConnector;
using PC7866.Models;
using System.Text.Json;

namespace PC7866.Services.Database;

/// <summary>
/// Implementacion de acceso a MariaDB mediante Dapper.
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

    /// <summary>
    /// Operaciones de consulta, alta, modificacion y eliminacion de referencias.
    /// </summary>

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
                (b_activa, referencia, descripcion, fecha_creacion, fecha_modificacion, imagen,
                 modelo_placa, num_mcps, inh1_pos, inh2_pos, inh3_pos, inh4_pos, muestras, retardo_ms)
            VALUES
                (@BActiva, @Referencia, @Descripcion, @FechaCreacion, @FechaModificacion, @Imagen,
                 @ModeloPlaca, @NumMcps, @Inh1Pos, @Inh2Pos, @Inh3Pos, @Inh4Pos, @Muestras, @RetardoMs);
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
            r.Imagen,
            r.ModeloPlaca,
            r.NumMcps,
            r.Inh1Pos,
            r.Inh2Pos,
            r.Inh3Pos,
            r.Inh4Pos,
            r.Muestras,
            r.RetardoMs
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
                imagen             = @Imagen,
                modelo_placa       = @ModeloPlaca,
                num_mcps           = @NumMcps,
                inh1_pos           = @Inh1Pos,
                inh2_pos           = @Inh2Pos,
                inh3_pos           = @Inh3Pos,
                inh4_pos           = @Inh4Pos,
                muestras           = @Muestras,
                retardo_ms         = @RetardoMs
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
            r.Imagen,
            r.ModeloPlaca,
            r.NumMcps,
            r.Inh1Pos,
            r.Inh2Pos,
            r.Inh3Pos,
            r.Inh4Pos,
            r.Muestras,
            r.RetardoMs
        });
    }

    public async Task SetReferenciaActivaAsync(int id, bool activa)
    {
        const string sql = "UPDATE referencias SET b_activa = @Activa, fecha_modificacion = NOW() WHERE id = @Id";
        using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new { Id = id, Activa = activa });
    }

    public async Task DeleteReferenciaAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var transaction = await conn.BeginTransactionAsync();

        try
        {
            await conn.ExecuteAsync("""
                UPDATE resultados_detalle rd
                INNER JOIN parametros_ensayo p ON p.id = rd.parametro_ensayo_id
                SET rd.parametro_ensayo_id = NULL
                WHERE p.referencia_id = @Id
                """, new { Id = id }, transaction);
            await conn.ExecuteAsync(
                "UPDATE resultados SET referencia_id = NULL WHERE referencia_id = @Id",
                new { Id = id }, transaction);
            await conn.ExecuteAsync(
                "DELETE FROM parametros_ensayo WHERE referencia_id = @Id",
                new { Id = id }, transaction);
            await conn.ExecuteAsync(
                "DELETE FROM referencias WHERE id = @Id",
                new { Id = id }, transaction);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Operaciones de consulta, alta, modificacion y eliminacion de parametros de ensayo.
    /// </summary>

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
                (referencia_id, nombre_contacto, n_paso_ensayo,
                 resistencia_nominal, tolerancia, pendiente_val, offset_val, resistencia_minima,
                 mcp_arriba_chip, mcp_arriba_pin, mcp_abajo_chip, mcp_abajo_pin, canal_multiplexor,
                 fecha_creacion, fecha_modificacion, pos_x, pos_y)
            VALUES
                (@ReferenciaId, @NombreContacto, @NPasoEnsayo,
                 @ResistenciaNominal, @Tolerancia, @Pendiente, @Offset, @ResistenciaMinima,
                 @McpArribaChip, @McpArribaPin, @McpAbajoChip, @McpAbajoPin, @CanalMultiplexor,
                 @FechaCreacion, @FechaModificacion, @PosX, @PosY);
            SELECT LAST_INSERT_ID();
            """;

        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            p.ReferenciaId,
            p.NombreContacto,
            p.NPasoEnsayo,
            p.ResistenciaNominal,
            p.Tolerancia,
            p.Pendiente,
            p.Offset,
            p.ResistenciaMinima,
            p.McpArribaChip,
            p.McpArribaPin,
            p.McpAbajoChip,
            p.McpAbajoPin,
            p.CanalMultiplexor,
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
                resistencia_nominal= @ResistenciaNominal,
                tolerancia         = @Tolerancia,
                pendiente_val      = @Pendiente,
                offset_val         = @Offset,
                resistencia_minima = @ResistenciaMinima,
                mcp_arriba_chip    = @McpArribaChip,
                mcp_arriba_pin     = @McpArribaPin,
                mcp_abajo_chip     = @McpAbajoChip,
                mcp_abajo_pin      = @McpAbajoPin,
                canal_multiplexor  = @CanalMultiplexor,
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
            p.ResistenciaNominal,
            p.Tolerancia,
            p.Pendiente,
            p.Offset,
            p.ResistenciaMinima,
            p.McpArribaChip,
            p.McpArribaPin,
            p.McpAbajoChip,
            p.McpAbajoPin,
            p.CanalMultiplexor,
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

    /// <summary>
    /// Operaciones de consulta y alta de resultados completos de ensayo.
    /// </summary>

    public async Task<IEnumerable<Resultado>> GetAllResultadosAsync()
    {
        const string sql = "SELECT * FROM resultados ORDER BY fecha_prueba DESC";
        using var conn = CreateConnection();
        var rows = await conn.QueryAsync(sql);
        var resultados = rows.Select(MapResultado).ToList();
        
        if (resultados.Count > 0)
        {
            // Cargar todos los detalles en una sola query
            var ids = string.Join(",", resultados.Select(r => r.Id));
            var sqlDetalles = $"SELECT * FROM resultados_detalle WHERE resultado_id IN ({ids}) ORDER BY n_paso_ensayo";
            var detalleRows = await conn.QueryAsync(sqlDetalles);
            var detallesDict = new Dictionary<int, List<ResultadoDetalle>>();
            foreach (var d in detalleRows.Select(MapResultadoDetalle))
            {
                if (!detallesDict.ContainsKey(d.ResultadoId))
                    detallesDict[d.ResultadoId] = new();
                detallesDict[d.ResultadoId].Add(d);
            }
            
            foreach (var r in resultados)
                if (detallesDict.TryGetValue(r.Id, out var detalles))
                    r.Detalles = detalles;
        }
        
        return resultados;
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
        var resultados = rows.Select(MapResultado).ToList();
        
        if (resultados.Count > 0)
        {
            // Cargar todos los detalles en una sola query
            var ids = string.Join(",", resultados.Select(r => r.Id));
            var sqlDetalles = $"SELECT * FROM resultados_detalle WHERE resultado_id IN ({ids}) ORDER BY n_paso_ensayo";
            var detalleRows = await conn.QueryAsync(sqlDetalles);
            var detallesDict = new Dictionary<int, List<ResultadoDetalle>>();
            foreach (var d in detalleRows.Select(MapResultadoDetalle))
            {
                if (!detallesDict.ContainsKey(d.ResultadoId))
                    detallesDict[d.ResultadoId] = new();
                detallesDict[d.ResultadoId].Add(d);
            }
            
            foreach (var r in resultados)
                if (detallesDict.TryGetValue(r.Id, out var detalles))
                    r.Detalles = detalles;
        }
        
        return resultados;
    }

    public async Task<Resultado?> GetResultadoByIdAsync(int id)
    {
        const string sql = "SELECT * FROM resultados WHERE id = @Id";
        using var conn = CreateConnection();
        var row = await conn.QueryFirstOrDefaultAsync(sql, new { Id = id });
        if (row is null) return null;
        
        var resultado = MapResultado(row);
        resultado.Detalles = (await GetDetallesByResultadoAsync(id)).ToList();
        return resultado;
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

    /// <summary>
    /// Operaciones de consulta y alta de detalles asociados a un resultado.
    /// </summary>

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
                 resistencia_medida, valor_raw_vain, valor_raw_ve, resultado, estado_medicion, timestamp_medicion)
            VALUES
                (@ResultadoId, @ParametroEnsayoId, @NombreContacto, @NPasoEnsayo,
                 @ResistenciaMedida, @ValorRawVain, @ValorRawVe, @Resultado, @Estado, @Timestamp)
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
            Estado = d.Estado.ToString(),
            d.Timestamp
        });
    }

    /// <summary>
    /// Operaciones auxiliares de conexion, inicializacion y mantenimiento del esquema.
    /// </summary>

    public async Task<bool> TestConnectionAsync()
    {
        await EnsureDatabaseExistsAsync();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        return true;
    }

    /// <summary>
    /// Crea la base de datos configurada si todavía no existe en el servidor.
    /// Se conecta sin seleccionar ninguna base (elimina "Database=" de la cadena
    /// de conexión) porque MariaDB rechaza la conexión si la base indicada no existe.
    /// </summary>
    private async Task EnsureDatabaseExistsAsync()
    {
        var builder = new MySqlConnectionStringBuilder(_connectionString);
        string dbName = builder.Database;
        if (string.IsNullOrWhiteSpace(dbName)) return;

        builder.Database = string.Empty;
        using var conn = new MySqlConnection(builder.ConnectionString);
        await conn.OpenAsync();
        await conn.ExecuteAsync(
            $"CREATE DATABASE IF NOT EXISTS `{dbName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");
    }

    public async Task InitializeDatabaseAsync()
    {
        await EnsureDatabaseExistsAsync();

        const string sqlReferencias = """
            CREATE TABLE IF NOT EXISTS referencias (
                id                  INT AUTO_INCREMENT PRIMARY KEY,
                b_activa            BOOLEAN  NOT NULL DEFAULT TRUE,
                referencia          VARCHAR(255) NOT NULL,
                descripcion         TEXT,
                fecha_creacion      DATETIME NOT NULL,
                fecha_modificacion  DATETIME NOT NULL,
                imagen              LONGBLOB,
                modelo_placa        VARCHAR(10) NOT NULL DEFAULT '',
                num_mcps            INT NOT NULL DEFAULT 6,
                inh1_pos            INT NULL,
                inh2_pos            INT NULL,
                inh3_pos            INT NULL,
                inh4_pos            INT NULL,
                muestras            INT NOT NULL DEFAULT 1,
                retardo_ms          INT NOT NULL DEFAULT 0
            );
            """;

        const string sqlParametros = """
            CREATE TABLE IF NOT EXISTS parametros_ensayo (
                id                  INT AUTO_INCREMENT PRIMARY KEY,
                referencia_id       INT  NOT NULL,
                nombre_contacto     VARCHAR(20)  NOT NULL,
                n_paso_ensayo       INT  NOT NULL,
                resistencia_nominal FLOAT NOT NULL DEFAULT 0,
                tolerancia          FLOAT NOT NULL DEFAULT 0,
                pendiente_val       FLOAT NOT NULL DEFAULT 1,
                offset_val          FLOAT NOT NULL DEFAULT 0,
                resistencia_minima  FLOAT NOT NULL DEFAULT 0,
                mcp_arriba_chip     INT NOT NULL DEFAULT 0,
                mcp_arriba_pin      INT NOT NULL DEFAULT 0,
                mcp_abajo_chip      INT NOT NULL DEFAULT 0,
                mcp_abajo_pin       INT NOT NULL DEFAULT 0,
                canal_multiplexor   INT NOT NULL DEFAULT 0,
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
                estado_medicion     VARCHAR(20) NOT NULL DEFAULT 'Nok',
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

        // Migraciones: nuevas columnas para deteccion de cortocircuito (umbral minimo y estado detallado)
        await conn.ExecuteAsync("ALTER TABLE parametros_ensayo ADD COLUMN IF NOT EXISTS resistencia_minima FLOAT NOT NULL DEFAULT 0;");
        await conn.ExecuteAsync("ALTER TABLE resultados_detalle ADD COLUMN IF NOT EXISTS estado_medicion VARCHAR(20) NOT NULL DEFAULT 'Nok';");

        // Migracion: pendiente de la funcion lineal de calculo de resistencia (R = Pendiente * R_bruta - Offset)
        await conn.ExecuteAsync("ALTER TABLE parametros_ensayo ADD COLUMN IF NOT EXISTS pendiente_val FLOAT NOT NULL DEFAULT 1;");

        // Migraciones: configuracion de placa (chip/pin MCP23017 arriba/abajo y canal de multiplexor)
        await conn.ExecuteAsync("ALTER TABLE parametros_ensayo ADD COLUMN IF NOT EXISTS mcp_arriba_chip INT NOT NULL DEFAULT 0;");
        await conn.ExecuteAsync("ALTER TABLE parametros_ensayo ADD COLUMN IF NOT EXISTS mcp_arriba_pin INT NOT NULL DEFAULT 0;");
        await conn.ExecuteAsync("ALTER TABLE parametros_ensayo ADD COLUMN IF NOT EXISTS mcp_abajo_chip INT NOT NULL DEFAULT 0;");
        await conn.ExecuteAsync("ALTER TABLE parametros_ensayo ADD COLUMN IF NOT EXISTS mcp_abajo_pin INT NOT NULL DEFAULT 0;");
        await conn.ExecuteAsync("ALTER TABLE parametros_ensayo ADD COLUMN IF NOT EXISTS canal_multiplexor INT NOT NULL DEFAULT 0;");

        // Migracion: se elimino el campo "salidas activas" (n_salida_json), reemplazado por los
        // selectores arriba/abajo (mcp_arriba_*/mcp_abajo_*).
        await conn.ExecuteAsync("ALTER TABLE parametros_ensayo DROP COLUMN IF EXISTS n_salida_json;");

        // Migraciones: configuracion de placa a nivel de Referencia (comando "I" del protocolo nuevo)
        await conn.ExecuteAsync("ALTER TABLE referencias ADD COLUMN IF NOT EXISTS modelo_placa VARCHAR(10) NOT NULL DEFAULT '';");
        await conn.ExecuteAsync("ALTER TABLE referencias ADD COLUMN IF NOT EXISTS num_mcps INT NOT NULL DEFAULT 6;");
        await conn.ExecuteAsync("ALTER TABLE referencias ADD COLUMN IF NOT EXISTS inh1_pos INT NULL;");
        await conn.ExecuteAsync("ALTER TABLE referencias ADD COLUMN IF NOT EXISTS inh2_pos INT NULL;");
        await conn.ExecuteAsync("ALTER TABLE referencias ADD COLUMN IF NOT EXISTS inh3_pos INT NULL;");
        await conn.ExecuteAsync("ALTER TABLE referencias ADD COLUMN IF NOT EXISTS inh4_pos INT NULL;");
        await conn.ExecuteAsync("ALTER TABLE referencias ADD COLUMN IF NOT EXISTS muestras INT NOT NULL DEFAULT 1;");
        await conn.ExecuteAsync("ALTER TABLE referencias ADD COLUMN IF NOT EXISTS retardo_ms INT NOT NULL DEFAULT 0;");

        // Migracion: el nombre de la referencia ya no es unico (el identificador real es modelo_placa)
        await conn.ExecuteAsync("ALTER TABLE referencias DROP INDEX IF EXISTS referencia;");
    }

    public void Dispose() { }

    /// <summary>
    /// Conversion de filas dinamicas de base de datos a modelos de dominio.
    /// </summary>

    private static Referencia MapReferencia(dynamic row) => new()
    {
        Id                 = (int)row.id,
        BActiva            = (bool)row.b_activa,
        ReferenciaNombre   = (string)row.referencia,
        Descripcion        = (string?)row.descripcion ?? string.Empty,
        FechaCreacion      = (DateTime)row.fecha_creacion,
        FechaModificacion  = (DateTime)row.fecha_modificacion,
        Imagen             = (byte[]?)row.imagen,
        ModeloPlaca        = HasColumn(row, "modelo_placa") ? (string?)row.modelo_placa ?? string.Empty : string.Empty,
        NumMcps            = HasColumn(row, "num_mcps")  ? (int)row.num_mcps  : Pc7866Commands.McpChipCount,
        Inh1Pos            = HasColumn(row, "inh1_pos")  ? (int?)row.inh1_pos : null,
        Inh2Pos            = HasColumn(row, "inh2_pos")  ? (int?)row.inh2_pos : null,
        Inh3Pos            = HasColumn(row, "inh3_pos")  ? (int?)row.inh3_pos : null,
        Inh4Pos            = HasColumn(row, "inh4_pos")  ? (int?)row.inh4_pos : null,
        Muestras           = HasColumn(row, "muestras")  ? (int)row.muestras  : 1,
        RetardoMs          = HasColumn(row, "retardo_ms") ? (int)row.retardo_ms : 0
    };

    private static ParametroEnsayo MapParametroEnsayo(dynamic row)
    {
        return new ParametroEnsayo
        {
            Id                 = (int)row.id,
            ReferenciaId       = (int)row.referencia_id,
            NombreContacto     = (string)row.nombre_contacto,
            NPasoEnsayo        = (int)row.n_paso_ensayo,
            ResistenciaNominal = (float)row.resistencia_nominal,
            Tolerancia         = (float)row.tolerancia,
            Pendiente          = HasColumn(row, "pendiente_val") ? (float)row.pendiente_val : 1f,
            Offset             = (float)row.offset_val,
            ResistenciaMinima  = HasColumn(row, "resistencia_minima") ? (float)row.resistencia_minima : 0f,
            McpArribaChip      = HasColumn(row, "mcp_arriba_chip") ? (int)row.mcp_arriba_chip : -1,
            McpArribaPin       = HasColumn(row, "mcp_arriba_pin") ? (int)row.mcp_arriba_pin : 0,
            McpAbajoChip       = HasColumn(row, "mcp_abajo_chip") ? (int)row.mcp_abajo_chip : -1,
            McpAbajoPin        = HasColumn(row, "mcp_abajo_pin") ? (int)row.mcp_abajo_pin : 0,
            CanalMultiplexor   = HasColumn(row, "canal_multiplexor") ? (int)row.canal_multiplexor : 0,
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

    private static ResultadoDetalle MapResultadoDetalle(dynamic row)
    {
        bool resultado = (bool)row.resultado;
        var estado = EstadoMedicion.Nok;
        if (HasColumn(row, "estado_medicion"))
        {
            string? estadoStr = (string?)row.estado_medicion;
            if (!string.IsNullOrEmpty(estadoStr) && Enum.TryParse(estadoStr, out EstadoMedicion parsed))
                estado = parsed;
            else
                estado = resultado ? EstadoMedicion.Ok : EstadoMedicion.Nok;
        }
        else
        {
            estado = resultado ? EstadoMedicion.Ok : EstadoMedicion.Nok;
        }

        return new ResultadoDetalle
        {
            Id                = (int)row.id,
            ResultadoId       = (int)row.resultado_id,
            ParametroEnsayoId = row.parametro_ensayo_id == null ? (int?)null : (int)row.parametro_ensayo_id,
            NombreContacto    = (string?)row.nombre_contacto ?? string.Empty,
            NPasoEnsayo       = (int)row.n_paso_ensayo,
            ResistenciaMedida = (float)row.resistencia_medida,
            ValorRawVain      = (int)row.valor_raw_vain,
            ValorRawVe        = (int)row.valor_raw_ve,
            Resultado         = resultado,
            Estado            = estado,
            Timestamp         = (DateTime)row.timestamp_medicion
        };
    }

    /// <summary>Comprueba si una fila dinámica (IDictionary subyacente) contiene una columna dada.</summary>
    private static bool HasColumn(dynamic row, string columnName)
        => ((IDictionary<string, object>)row).ContainsKey(columnName);
}
