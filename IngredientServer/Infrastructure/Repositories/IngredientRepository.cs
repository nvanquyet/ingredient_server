using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Infrastructure.Data;
using IngredientServer.Utils.DTOs.Ingredient;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public class IngredientRepository : BaseRepository<Ingredient>, IIngredientRepository
{
    public IngredientRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Ingredient?> GetByIdAndUserIdAsync(int id, int userId)
    {
        if (id <= 0 || userId <= 0)
            throw new ArgumentException("Invalid id or userId");

        return await GetByIdAsync(id, userId); // Tái sử dụng từ BaseRepository
    }

    public async Task<List<Ingredient>> GetByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 10)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid userId");
        if (pageNumber < 1)
            throw new ArgumentException("Invalid pageNumber");
        if (pageSize <= 0)
            throw new ArgumentException("Invalid pageSize");

        return await Context.Set<Ingredient>()
            .Where(i => i.UserId == userId)
            .OrderBy(i => i.ExpiryDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Ingredient>> GetExpiringItemsAsync(int userId, int days = 7)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid userId");
        if (days < 0)
            throw new ArgumentException("Invalid days");

        var cutoffDate = DateTime.UtcNow.Date.AddDays(days);
        return await Context.Set<Ingredient>()
            .Where(i => i.UserId == userId &&
                        i.ExpiryDate.Date <= cutoffDate &&
                        i.ExpiryDate.Date >= DateTime.UtcNow.Date)
            .OrderBy(i => i.ExpiryDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Ingredient>> GetExpiredItemsAsync(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid userId");

        return await Context.Set<Ingredient>()
            .Where(i => i.UserId == userId &&
                        i.ExpiryDate.Date < DateTime.UtcNow.Date)
            .OrderBy(i => i.ExpiryDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Ingredient>> GetFilteredAsync(IngredientFilterDto filter, int pageNumber = 1, int pageSize = 10)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));
        if (filter.UserId <= 0)
            throw new ArgumentException("Invalid UserId");
        if (pageNumber < 1)
            throw new ArgumentException("Invalid pageNumber");
        if (pageSize <= 0)
            throw new ArgumentException("Invalid pageSize");

        var query = Context.Set<Ingredient>()
            .Where(i => i.UserId == filter.UserId);

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

    public async Task<List<Ingredient>> GetSortedAsync(int userId, IngredientSortDto sort, int pageNumber = 1, int pageSize = 10)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid userId");
        if (sort == null)
            throw new ArgumentNullException(nameof(sort));
        if (pageNumber < 1)
            throw new ArgumentException("Invalid pageNumber");
        if (pageSize <= 0)
            throw new ArgumentException("Invalid pageSize");

        var query = Context.Set<Ingredient>()
            .Where(i => i.UserId == userId);

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

    public async Task<List<Ingredient>> GetByCategoryAsync(int userId, IngredientCategory category, int pageNumber = 1, int pageSize = 10)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid userId");
        if (pageNumber < 1)
            throw new ArgumentException("Invalid pageNumber");
        if (pageSize <= 0)
            throw new ArgumentException("Invalid pageSize");

        return await Context.Set<Ingredient>()
            .Where(i => i.UserId == userId && i.Category == category)
            .OrderBy(i => i.ExpiryDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> CountByUserIdAsync(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid userId");

        return await Context.Set<Ingredient>()
            .CountAsync(i => i.UserId == userId);
    }
}