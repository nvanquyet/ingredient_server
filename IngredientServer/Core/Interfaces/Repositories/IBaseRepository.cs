using IngredientServer.Core.Entities;

namespace IngredientServer.Core.Interfaces.Repositories;

public interface IBaseRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> ExistsAsync(int id);
    Task<T> CreateAsync(T entity);
    Task<bool> DeleteAsync(T entity);
    Task<bool> DeleteByIdAsync(int id);
}
