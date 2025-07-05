using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public class UserNutritionRepository(ApplicationDbContext context, IUserContextService userContextService) : BaseRepository<UserNutritionTargets>(context, userContextService), IUserNutritionRepository
{
    public async Task<UserNutritionTargets?> GetByUserIdAsync()
    {
        return await Context.Set<UserNutritionTargets>()
            .AsNoTracking()
            .Where(e => e.UserId == userContextService.GetAuthenticatedUserId())
            .FirstOrDefaultAsync();
    }

    public async Task<UserNutritionTargets?> SaveNutrition(UserNutritionTargets targets)
    {
        var existingTargets = await GetByUserIdAsync();
        if (existingTargets != null)
        {
            await AddAsync(targets);
        }
        else
        {
            await UpdateAsync(targets);
        }
    }
}