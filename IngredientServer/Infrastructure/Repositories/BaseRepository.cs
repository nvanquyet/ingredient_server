using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Core.Services;
using IngredientServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public abstract class BaseRepository<T>(ApplicationDbContext context, IUserContextService userContextService)
    : IBaseRepository<T>
    where T : BaseEntity
{
    protected readonly ApplicationDbContext Context = context;

    protected int AuthenticatedUserId => userContextService.GetAuthenticatedUserId();

    public async Task<T?> GetByIdAsync(int id)
    {
        return await Context.Set<T>()
            .Where(e => e.Id == id && e.UserId == AuthenticatedUserId)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await Context.Set<T>()
            .Where(e => e.UserId == AuthenticatedUserId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        entity.UserId = AuthenticatedUserId; // Automatically set UserId from context
        Context.Set<T>().Add(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        // Verify ownership
        var existingEntity = await GetByIdAsync(entity.Id);
        if (existingEntity == null)
        {
            throw new UnauthorizedAccessException("Entity not found or access denied.");
        }

        entity.UserId = AuthenticatedUserId; // Ensure UserId is correct
        Context.Set<T>().Update(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
        {
            throw new UnauthorizedAccessException("Entity not found or access denied.");
            return false;
        }
        
        Context.Set<T>().Remove(entity);
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await Context.Set<T>()
            .AnyAsync(e => e.Id == id && e.UserId == AuthenticatedUserId);
    }
}