using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Infrastructure.Data;
using IngredientServer.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public class FoodIngredientRepository(ApplicationDbContext context, IUserContextService userContextService)
    : BaseRepository<FoodIngredient>(context, userContextService), IFoodIngredientRepository
{
    public async Task<IEnumerable<FoodIngredient>> GetByFoodIdAsync(int foodId)
    {
        return await Context.Set<FoodIngredient>()
            .Include(fi => fi.Ingredient)
            .Where(fi => fi.FoodId == foodId && fi.UserId == AuthenticatedUserId)
            .ToListAsync();
    }
}