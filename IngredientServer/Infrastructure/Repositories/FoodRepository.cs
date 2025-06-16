using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Core.Services;
using IngredientServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public class FoodRepository(ApplicationDbContext context, IUserContextService userContextService)
    : BaseRepository<Food>(context, userContextService), IFoodRepository
{
    public async Task<Food?> GetFoodDetailsAsync(int foodId)
    {
        return await Context.Set<Food>()
            .Where(f => f.Id == foodId && f.UserId == AuthenticatedUserId)
            .Include(f => f.FoodIngredients)
                .ThenInclude(fi => fi.Ingredient)
            .Include(f => f.MealFoods)
                .ThenInclude(mf => mf.Meal)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<List<Food>> GetAllUserFoodsAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await Context.Set<Food>()
            .Where(f => f.UserId == AuthenticatedUserId)
            .OrderBy(f => f.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Food>> GetByFoodCategoryAsync(FoodCategory category, int pageNumber = 1, int pageSize = 10)
    {
        return await Context.Set<Food>()
            .Where(f => f.UserId == AuthenticatedUserId && f.Category == category)
            .OrderBy(f => f.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Food>> SearchFoodsAsync(string searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllUserFoodsAsync(pageNumber, pageSize);

        var lowercaseSearch = searchTerm.ToLower();
        return await Context.Set<Food>()
            .Where(f => f.UserId == AuthenticatedUserId &&
                       (f.Name.ToLower().Contains(lowercaseSearch) ||
                        (f.Recipe != null && f.Recipe.ToLower().Contains(lowercaseSearch))))
            .OrderBy(f => f.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Food>> GetFoodsByPreparationTimeAsync(int maxMinutes, int pageNumber = 1, int pageSize = 10)
    {
        return await Context.Set<Food>()
            .Where(f => f.UserId == AuthenticatedUserId && 
                       f.PreparationTimeMinutes.HasValue && 
                       f.PreparationTimeMinutes <= maxMinutes)
            .OrderBy(f => f.PreparationTimeMinutes)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Food>> GetRecentlyCreatedFoodsAsync(int days = 7, int pageNumber = 1, int pageSize = 10)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await Context.Set<Food>()
            .Where(f => f.UserId == AuthenticatedUserId && f.CreatedAt >= cutoffDate)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddIngredientToFoodAsync(int foodId, int ingredientId, decimal quantity)
    {
        // Verify food ownership
        var food = await GetByIdAsync(foodId);
        if (food == null)
            throw new UnauthorizedAccessException("Food not found or access denied.");

        // Verify ingredient ownership
        var ingredient = await Context.Set<Ingredient>()
            .Where(i => i.Id == ingredientId && i.UserId == AuthenticatedUserId)
            .FirstOrDefaultAsync();
        if (ingredient == null)
            throw new UnauthorizedAccessException("Ingredient not found or access denied.");

        // Check if ingredient is already in food
        var existingFoodIngredient = await Context.Set<FoodIngredient>()
            .Where(fi => fi.FoodId == foodId && fi.IngredientId == ingredientId)
            .FirstOrDefaultAsync();

        if (existingFoodIngredient != null)
        {
            // Update quantity if already exists
            existingFoodIngredient.Quantity = quantity;
            Context.Set<FoodIngredient>().Update(existingFoodIngredient);
        }
        else
        {
            // Add new food ingredient
            var foodIngredient = new FoodIngredient
            {
                FoodId = foodId,
                IngredientId = ingredientId,
                Quantity = quantity
            };
            Context.Set<FoodIngredient>().Add(foodIngredient);
        }

        await Context.SaveChangesAsync();
    }

    public async Task RemoveIngredientFromFoodAsync(int foodId, int ingredientId)
    {
        // Verify food ownership first
        var food = await GetByIdAsync(foodId);
        if (food == null)
            throw new UnauthorizedAccessException("Food not found or access denied.");

        var foodIngredient = await Context.Set<FoodIngredient>()
            .Where(fi => fi.FoodId == foodId && fi.IngredientId == ingredientId)
            .FirstOrDefaultAsync();

        if (foodIngredient == null)
            throw new InvalidOperationException("Ingredient not found in this food.");

        Context.Set<FoodIngredient>().Remove(foodIngredient);
        await Context.SaveChangesAsync();
    }

    public async Task<int> GetUserFoodCountAsync()
    {
        return await Context.Set<Food>()
            .CountAsync(f => f.UserId == AuthenticatedUserId);
    }

    public async Task<List<Food>> GetFoodsWithIngredientAsync(int ingredientId, int pageNumber = 1, int pageSize = 10)
    {
        return await Context.Set<Food>()
            .Where(f => f.UserId == AuthenticatedUserId && 
                       f.FoodIngredients.Any(fi => fi.IngredientId == ingredientId))
            .Include(f => f.FoodIngredients)
                .ThenInclude(fi => fi.Ingredient)
            .OrderBy(f => f.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }
}