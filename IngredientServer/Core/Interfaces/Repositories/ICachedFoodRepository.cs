using IngredientServer.Core.Entities;
using IngredientServer.Utils.DTOs.Entity;

namespace IngredientServer.Core.Interfaces.Repositories;

/// <summary>
/// Repository for cached food recipes (public cache)
/// </summary>
public interface ICachedFoodRepository
{
    /// <summary>
    /// Find cached food by search key
    /// </summary>
    Task<CachedFood?> FindBySearchKeyAsync(string searchKey);

    /// <summary>
    /// Add new cached food
    /// </summary>
    Task<CachedFood> AddAsync(CachedFood cachedFood);

    /// <summary>
    /// Update cached food (e.g., increment hit count)
    /// </summary>
    Task UpdateAsync(CachedFood cachedFood);

    /// <summary>
    /// Generate search key from food name and ingredients
    /// </summary>
    string GenerateSearchKey(string foodName, IEnumerable<FoodIngredientDto>? ingredients);
}

