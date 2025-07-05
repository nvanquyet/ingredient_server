using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public class UserNutritionRepository(ApplicationDbContext context, IUserContextService userContextService) : BaseRepository<UserNutritionTargets>(context, userContextService), IUserNutritionRepository
{
    public async Task<UserNutritionTargets?> GetByUserIdAsync(int userId)
    {
        return await Context.Set<UserNutritionTargets>()
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .FirstOrDefaultAsync();
    }

}