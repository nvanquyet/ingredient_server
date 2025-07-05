using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IngredientServer.Infrastructure.Repositories;

public class UserNutritionRepository(ApplicationDbContext context, IUserContextService userContextService, ILogger logger) : BaseRepository<UserNutritionTargets>(context, userContextService), IUserNutritionRepository
{
    private readonly IUserContextService _userContextService = userContextService;

    public async Task<UserNutritionTargets?> GetByUserIdAsync()
    {
        return await Context.Set<UserNutritionTargets>()
            .AsNoTracking()
            .Where(e => e.UserId == _userContextService.GetAuthenticatedUserId())
            .FirstOrDefaultAsync();
    }

    public async Task<UserNutritionTargets?> SaveNutrition(UserNutritionTargets targets)
    {
        logger.LogInformation("Fetching nutrition");
        var existingTargets = await GetByUserIdAsync();
        
        if (existingTargets != null)
        {
            logger.LogInformation("Update existing nutrition targets for user {UserId}", _userContextService.GetAuthenticatedUserId());
            return await UpdateAsync(targets);
        }
        else
        {
            logger.LogInformation("Add existing nutrition targets for user {UserId}", _userContextService.GetAuthenticatedUserId());
            return await AddAsync(targets);
        }
    }
}