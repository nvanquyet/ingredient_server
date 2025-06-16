using IngredientServer.Core.Entities;

namespace IngredientServer.Core.Interfaces.Repositories;

public interface IBaseRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id); 
    Task<IEnumerable<T>> GetAllAsync(int pageNumber = 1, int pageSize = 10); 
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id); 
    Task<bool> ExistsAsync(int id);
}