using System.Linq.Expressions;
using IngredientServer.Core.Entities;

namespace IngredientServer.Core.Interfaces.Repositories;

public interface IBaseRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id); 
    Task<IEnumerable<T>> GetAllAsync(); 
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate); 
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id); 
    Task<bool> DeleteAsync(Expression<Func<T, bool>> predicate); 
    Task<bool> DeleteSafeAsync(Expression<Func<T, bool>> predicate)
    Task<bool> ExistsAsync(int id);
}