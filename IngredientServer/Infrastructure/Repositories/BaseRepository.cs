using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public abstract class BaseRepository<T>(ApplicationDbContext context) : IBaseRepository<T>
    where T : BaseEntity
{
    protected readonly ApplicationDbContext Context = context;

    public async Task<T?> GetByIdAsync(int id, int userId)
    {
        return await Context.Set<T>()
            .Where(e => e.Id == id && e.UserId == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync(int userId, int pageNumber = 1, int pageSize = 10)
    {
        return await Context.Set<T>()
            .Where(e => e.UserId == userId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        Context.Set<T>().Add(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        Context.Set<T>().Update(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var entity = await GetByIdAsync(id, userId);
        if (entity == null) return false;
        Context.Set<T>().Remove(entity);
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id, int userId)
    {
        return await Context.Set<T>()
            .AnyAsync(e => e.Id == id && e.UserId == userId);
    }
}