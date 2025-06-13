using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace IngredientServer.Infrastructure.Repositories;

public class FoodRepository(ApplicationDbContext context) : BaseRepository<Food>(context), IFoodRepository
{
    public async Task<Food?> GetFoodDetailsAsync(int foodId, int userId)
    {
        return await Context.Set<Food>()
            .Where(f => f.Id == foodId && f.UserId == userId)
            .Include(f => f.FoodIngredients)
            .ThenInclude(fi => fi.Ingredient)
            .Include(f => f.MealFoods)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
}