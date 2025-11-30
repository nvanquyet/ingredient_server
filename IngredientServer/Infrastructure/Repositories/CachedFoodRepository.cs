using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Infrastructure.Data;
using IngredientServer.Utils.DTOs.Entity;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public class CachedFoodRepository(ApplicationDbContext context) : ICachedFoodRepository
{
    public async Task<CachedFood?> FindBySearchKeyAsync(string searchKey)
    {
        return await context.Set<CachedFood>()
            .FirstOrDefaultAsync(cf => cf.SearchKey == searchKey);
    }

    public async Task<CachedFood> AddAsync(CachedFood cachedFood)
    {
        cachedFood.CreatedAt = DateTime.UtcNow;
        cachedFood.UpdatedAt = DateTime.UtcNow;
        cachedFood.LastAccessedAt = DateTime.UtcNow;
        
        await context.Set<CachedFood>().AddAsync(cachedFood);
        await context.SaveChangesAsync();
        
        return cachedFood;
    }

    public async Task UpdateAsync(CachedFood cachedFood)
    {
        cachedFood.UpdatedAt = DateTime.UtcNow;
        context.Set<CachedFood>().Update(cachedFood);
        await context.SaveChangesAsync();
    }

    public string GenerateSearchKey(string foodName, IEnumerable<FoodIngredientDto>? ingredients)
    {
        // Normalize food name: lowercase, trim, remove extra spaces
        var normalizedName = foodName.Trim().ToLowerInvariant()
            .Replace(" ", "_")
            .Replace("-", "_");

        // Sort ingredients by NAME (not ID) because IDs are user-specific
        // Format: "ingredient_name|quantity|unit"
        var ingredientKeys = ingredients?
            .Where(i => !string.IsNullOrWhiteSpace(i.IngredientName))
            .OrderBy(i => i.IngredientName.ToLowerInvariant())
            .Select(i => $"{i.IngredientName.ToLowerInvariant().Trim()}|{i.Quantity}|{i.Unit}")
            .ToList() ?? new List<string>();

        // Combine: "food_name|ingredient1_name|qty|unit|ingredient2_name|qty|unit|..."
        var searchKey = $"{normalizedName}|{string.Join("|", ingredientKeys)}";
        
        // Limit length to 500 characters (database constraint)
        if (searchKey.Length > 500)
        {
            // Use hash for very long keys
            var hash = searchKey.GetHashCode().ToString("X");
            searchKey = $"{normalizedName}|hash_{hash}";
        }

        return searchKey;
    }
}

