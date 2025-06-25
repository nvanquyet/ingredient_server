using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;
public class FoodIngredientRepository(ApplicationDbContext context, IUserContextService userContextService)
    : BaseRepository<FoodIngredient>(context, userContextService), IFoodIngredientRepository
{
    // Get FoodIngredients by FoodId
    public async Task<IEnumerable<FoodIngredient>> GetByFoodIdAsync(int foodId)
    {
        return await Context.Set<FoodIngredient>()
            .Where(fi => fi.FoodId == foodId && fi.Food.UserId == AuthenticatedUserId)
            .Include(fi => fi.Ingredient)
            .ToListAsync();
    }
}
