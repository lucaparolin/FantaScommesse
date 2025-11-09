using FantaScommesse.Models.Entities;

namespace FantaScommesse.Repositories;

/// <summary>
/// Generic repository interface for basic CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
/// <typeparam name="TKey">Primary key type</typeparam>
public interface IRepository<T, TKey> where T : BaseAuditEntity
{
    Task<T?> GetByIdAsync(TKey id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<TKey> CreateAsync(T entity, string username);
    Task<bool> UpdateAsync(T entity, string username);
    Task<bool> DeleteAsync(TKey id);
}
