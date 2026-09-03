using PC7866.Models;

namespace PC7866.Services.Database;

/// <summary>
/// Repositorio principal de la aplicación PC7866.
/// Gestiona Referencias, ParametrosEnsayo, Resultados y ResultadosDetalle.
/// </summary>
public interface ITestRepository : IDisposable
{
    // ── Referencias ──────────────────────────────────────────────────────────
    Task<IEnumerable<Referencia>> GetAllReferenciasAsync(bool soloActivas = false);
    Task<Referencia?> GetReferenciaByIdAsync(int id);
    Task<int>  InsertReferenciaAsync(Referencia referencia);
    Task       UpdateReferenciaAsync(Referencia referencia);
    Task       SetReferenciaActivaAsync(int id, bool activa);
    Task       DeleteReferenciaAsync(int id);

    // ── ParametrosEnsayo ─────────────────────────────────────────────────────
    Task<IEnumerable<ParametroEnsayo>> GetParametrosByReferenciaAsync(int referenciaId);
    Task<ParametroEnsayo?> GetParametroByIdAsync(int id);
    Task<int>  InsertParametroAsync(ParametroEnsayo parametro);
    Task       UpdateParametroAsync(ParametroEnsayo parametro);
    Task       DeleteParametroAsync(int id);

    // ── Resultados ───────────────────────────────────────────────────────────
    Task<IEnumerable<Resultado>> GetAllResultadosAsync();
    Task<IEnumerable<Resultado>> GetResultadosByReferenciaAsync(int referenciaId);
    Task<Resultado?> GetResultadoByIdAsync(int id);
    Task<int>  InsertResultadoAsync(Resultado resultado);

    // ── ResultadosDetalle ────────────────────────────────────────────────────
    Task<IEnumerable<ResultadoDetalle>> GetDetallesByResultadoAsync(int resultadoId);
    Task       InsertDetalleAsync(ResultadoDetalle detalle);

    // ── Utilidades ───────────────────────────────────────────────────────────
    Task<bool> TestConnectionAsync();
    Task       InitializeDatabaseAsync();
}
