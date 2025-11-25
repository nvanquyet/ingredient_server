using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IngredientServer.Infrastructure.Repositories;

public class UserNutritionRepository(ApplicationDbContext context, IUserContextService userContextService, ITimeService timeService) : BaseRepository<UserNutritionTargets>(context, userContextService, timeService), IUserNutritionRepository
{

    public async Task<UserNutritionTargets?> GetByUserIdAsync()
    {
        return await Context.Set<UserNutritionTargets>()
            .Where(e => e.UserId == userContextService.GetAuthenticatedUserId())
            .FirstOrDefaultAsync();
    }

    public async Task<UserNutritionTargets?> SaveNutrition(UserNutritionTargets targets)
    {
        var existingTargets = await GetByUserIdAsync();
        
        if (existingTargets != null)
        {
            return await UpdateAsync(targets);
        }
        else
        {
            return await AddAsync(targets);
        }
    }
}