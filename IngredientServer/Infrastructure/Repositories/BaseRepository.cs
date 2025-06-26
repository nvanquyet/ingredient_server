using System.Linq.Expressions;
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

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await Context.Set<T>()
            .Where(e => e.Id == id && e.UserId == AuthenticatedUserId)
            .FirstOrDefaultAsync();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await Context.Set<T>()
            .Where(e => e.UserId == AuthenticatedUserId)
            .ToListAsync();
    }
    
    public virtual async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
    {
        return await Context.Set<T>()
            .Where(predicate)
            .ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        entity.UserId = AuthenticatedUserId; // Automatically set UserId from context
        Context.Set<T>().Add(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
        entity.UserId = AuthenticatedUserId;
        Context.Set<T>().Update(entity);
        try
        {
            await Context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Log lỗi chi tiết
            Console.WriteLine($"UpdateAsync Error: {ex.InnerException?.Message}");
            throw new UnauthorizedAccessException("Entity not found or access denied.", ex);
        }
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(int id)
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
    
    public virtual async Task<bool> DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        var entities = await Context.Set<T>()
            .Where(predicate)
            .ToListAsync();

        if (!entities.Any())
        {
            throw new UnauthorizedAccessException("Entities not found or access denied.");
        }

        Context.Set<T>().RemoveRange(entities);
        await Context.SaveChangesAsync();
        return true;
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await Context.Set<T>()
            .AnyAsync(e => e.Id == id && e.UserId == AuthenticatedUserId);
    }
}