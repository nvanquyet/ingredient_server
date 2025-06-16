using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Core.Services;
using IngredientServer.Infrastructure.Data;
using IngredientServer.Utils.DTOs.Ingredient;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public class IngredientRepository(ApplicationDbContext context, IUserContextService userContextService)
    : BaseRepository<Ingredient>(context, userContextService), IIngredientRepository
{
    public async Task<List<Ingredient>> GetAllUserIngredientsAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await Context.Set<Ingredient>()
            .Where(i => i.UserId == AuthenticatedUserId)
            .OrderBy(i => i.ExpiryDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Ingredient>> GetExpiringItemsAsync(int days = 7)
    {
        var cutoffDate = DateTime.UtcNow.Date.AddDays(days);
        return await Context.Set<Ingredient>()
            .Where(i => i.UserId == AuthenticatedUserId &&
                        i.ExpiryDate.Date <= cutoffDate &&
                        i.ExpiryDate.Date >= DateTime.UtcNow.Date)
            .OrderBy(i => i.ExpiryDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Ingredient>> GetExpiredItemsAsync()
    {
        return await Context.Set<Ingredient>()
            .Where(i => i.UserId == AuthenticatedUserId &&
                        i.ExpiryDate.Date < DateTime.UtcNow.Date)
            .OrderBy(i => i.ExpiryDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Ingredient>> GetFilteredAsync(IngredientFilterDto filter, int pageNumber = 1, int pageSize = 10)
    {
        var query = Context.Set<Ingredient>()
            .Where(i => i.UserId == AuthenticatedUserId);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            query = query.Where(i => i.Name.ToLower().Contains(filter.SearchTerm.ToLower()) ||
                                    (i.Description != null && i.Description.ToLower().Contains(filter.SearchTerm.ToLower())));

        if (filter.Category.HasValue)
            query = query.Where(i => i.Category == filter.Category.Value);

        if (filter.Unit.HasValue)
            query = query.Where(i => i.Unit == filter.Unit.Value);

        if (filter.IsExpired.HasValue)
        {
            if (filter.IsExpired.Value)
                query = query.Where(i => i.ExpiryDate.Date < DateTime.UtcNow.Date);
            else
                query = query.Where(i => i.ExpiryDate.Date >= DateTime.UtcNow.Date);
        }

        if (filter.IsExpiringSoon.HasValue && filter.IsExpiringSoon.Value)
        {
            var cutoffDate = DateTime.UtcNow.Date.AddDays(7);
            query = query.Where(i => i.ExpiryDate.Date <= cutoffDate &&
                                    i.ExpiryDate.Date >= DateTime.UtcNow.Date);
        }

        if (filter.ExpiryDateFrom.HasValue)
            query = query.Where(i => i.ExpiryDate.Date >= filter.ExpiryDateFrom.Value.Date);

        if (filter.ExpiryDateTo.HasValue)
            query = query.Where(i => i.ExpiryDate.Date <= filter.ExpiryDateTo.Value.Date);

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Ingredient>> GetSortedAsync(IngredientSortDto sort, int pageNumber = 1, int pageSize = 10)
    {
        var query = Context.Set<Ingredient>()
            .Where(i => i.UserId == AuthenticatedUserId);

        query = sort.SortBy.ToLower() switch
        {
            "name" => sort.SortOrder.ToLower() == "desc"
                ? query.OrderByDescending(i => i.Name)
                : query.OrderBy(i => i.Name),
            "expirydate" => sort.SortOrder.ToLower() == "desc"
                ? query.OrderByDescending(i => i.ExpiryDate)
                : query.OrderBy(i => i.ExpiryDate),
            "quantity" => sort.SortOrder.ToLower() == "desc"
                ? query.OrderByDescending(i => i.Quantity)
                : query.OrderBy(i => i.Quantity),
            "createdat" => sort.SortOrder.ToLower() == "desc"
                ? query.OrderByDescending(i => i.CreatedAt)
                : query.OrderBy(i => i.CreatedAt),
            "category" => sort.SortOrder.ToLower() == "desc"
                ? query.OrderByDescending(i => i.Category)
                : query.OrderBy(i => i.Category),
            _ => query.OrderBy(i => i.Name)
        };

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Ingredient>> GetByCategoryAsync(IngredientCategory category, int pageNumber = 1, int pageSize = 10)
    {
        return await Context.Set<Ingredient>()
            .Where(i => i.UserId == AuthenticatedUserId && i.Category == category)
            .OrderBy(i => i.ExpiryDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetUserIngredientCountAsync()
    {
        return await Context.Set<Ingredient>()
            .CountAsync(i => i.UserId == AuthenticatedUserId);
    }
}