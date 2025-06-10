using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Infrastructure.Data;
using IngredientServer.Utils.DTOs.Ingredient;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public class IngredientRepository : IIngredientRepository
{
    private readonly ApplicationDbContext _context;

    public IngredientRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Ingredient> AddAsync(Ingredient ingredient)
    {
        ingredient.CreatedAt = DateTime.UtcNow;
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();
        return ingredient;
    }

    public async Task<Ingredient?> UpdateAsync(Ingredient ingredient)
    {
        var existingIngredient = await _context.Ingredients
            .FirstOrDefaultAsync(i => i.Id == ingredient.Id);

        if (existingIngredient == null) return null;

        existingIngredient.Name = ingredient.Name;
        existingIngredient.Description = ingredient.Description;
        existingIngredient.Quantity = ingredient.Quantity;
        existingIngredient.Unit = ingredient.Unit;
        existingIngredient.Category = ingredient.Category;
        existingIngredient.ExpiryDate = ingredient.ExpiryDate;
        existingIngredient.ImageUrl = ingredient.ImageUrl;
        existingIngredient.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingIngredient;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var ingredient = await _context.Ingredients.FindAsync(id);
        if (ingredient == null) return false;

        _context.Ingredients.Remove(ingredient);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Ingredient?> GetByIdAsync(int id)
    {
        return await _context.Ingredients
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Ingredient?> GetByIdAndUserIdAsync(int id, int userId)
    {
        return await _context.Ingredients
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
    }

    public async Task<List<Ingredient>> GetAllAsync()
    {
        return await _context.Ingredients
            .Include(i => i.User)
            .ToListAsync();
    }

    public async Task<List<Ingredient>> GetByUserIdAsync(int userId)
    {
        return await _context.Ingredients
            .Where(i => i.UserId == userId)
            .OrderBy(i => i.ExpiryDate)
            .ToListAsync();
    }

    public async Task<List<Ingredient>> GetExpiringItemsAsync(int userId, int days = 7)
    {
        var cutoffDate = DateTime.Now.Date.AddDays(days);
        return await _context.Ingredients
            .Where(i => i.UserId == userId &&
                        i.ExpiryDate.Date <= cutoffDate &&
                        i.ExpiryDate.Date >= DateTime.Now.Date)
            .OrderBy(i => i.ExpiryDate)
            .ToListAsync();
    }

    public async Task<List<Ingredient>> GetExpiredItemsAsync(int userId)
    {
        return await _context.Ingredients
            .Where(i => i.UserId == userId && i.ExpiryDate.Date < DateTime.Now.Date)
            .OrderBy(i => i.ExpiryDate)
            .ToListAsync();
    }

    public async Task<List<Ingredient>> GetFilteredAsync(IngredientFilterDto filter)
    {
        var query = _context.Ingredients.AsQueryable();

        query = query.Where(i => i.UserId == filter.UserId);

        if (filter.Category.HasValue)
            query = query.Where(i => i.Category == filter.Category.Value);

        if (filter.Unit.HasValue)
            query = query.Where(i => i.Unit == filter.Unit.Value);

        if (filter.IsExpired.HasValue)
        {
            if (filter.IsExpired.Value)
                query = query.Where(i => i.ExpiryDate.Date < DateTime.Now.Date);
            else
                query = query.Where(i => i.ExpiryDate.Date >= DateTime.Now.Date);
        }

        if (filter.IsExpiringSoon.HasValue && filter.IsExpiringSoon.Value)
        {
            var cutoffDate = DateTime.Now.Date.AddDays(7);
            query = query.Where(i => i.ExpiryDate.Date <= cutoffDate &&
                                     i.ExpiryDate.Date >= DateTime.Now.Date);
        }

        if (filter.ExpiryDateFrom.HasValue)
            query = query.Where(i => i.ExpiryDate.Date >= filter.ExpiryDateFrom.Value.Date);

        if (filter.ExpiryDateTo.HasValue)
            query = query.Where(i => i.ExpiryDate.Date <= filter.ExpiryDateTo.Value.Date);

        if (!string.IsNullOrEmpty(filter.SearchTerm))
            query = query.Where(i => i.Name.Contains(filter.SearchTerm) ||
                                     (i.Description != null && i.Description.Contains(filter.SearchTerm)));

        return await query.ToListAsync();
    }

    public async Task<List<Ingredient>> GetSortedAsync(int userId, IngredientSortDto sort)
    {
        var query = _context.Ingredients.Where(i => i.UserId == userId);

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

        return await query.ToListAsync();
    }

    public async Task<List<Ingredient>> GetByCategoryAsync(int userId, IngredientCategory category)
    {
        return await _context.Ingredients
            .Where(i => i.UserId == userId && i.Category == category)
            .OrderBy(i => i.ExpiryDate)
            .ToListAsync();
    }

    public async Task<int> CountByUserIdAsync(int userId)
    {
        return await _context.Ingredients.CountAsync(i => i.UserId == userId);
    }

    public async Task<bool> ExistsAsync(int id, int userId)
    {
        return await _context.Ingredients
            .AnyAsync(i => i.Id == id && i.UserId == userId);
    }
}