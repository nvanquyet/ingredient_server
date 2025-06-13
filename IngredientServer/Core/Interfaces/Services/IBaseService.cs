using IngredientServer.Core.Entities;

namespace IngredientServer.Core.Interfaces.Services;

public interface IBaseService<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, int userId);
    Task<IEnumerable<T>> GetAllAsync(int userId, int pageNumber = 1, int pageSize = 10);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id, int userId);
    Task<bool> ExistsAsync(int id, int userId);
}