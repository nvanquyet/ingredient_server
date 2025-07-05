using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs;
using Microsoft.Extensions.Logging;

namespace IngredientServer.Core.Services;

public class NutritionTargetsService(IAIService aiService, IUserNutritionRepository repository, IUserContextService userContextService) : INutritionTargetsService
{
    public async Task<UserNutritionTargets> GetUserNutritionTargetsAsync(UserInformationDto userInformation)
    {
        var targets = await GetOrCreateNutritionTargetsAsync(userInformation, CancellationToken.None);
        return targets;
    }

    public async Task<UserNutritionTargets> GetDailyUserNutritionTargetsAsync(UserInformationDto userInformation)
    {
        return await GetUserNutritionTargetsAsync(userInformation);
    }

    public async Task<UserNutritionTargets> GetWeeklyUserNutritionTargetsAsync(UserInformationDto userInformation)
    {
        var target = await GetUserNutritionTargetsAsync(userInformation);
        target.TargetDailyCalories = target.TargetDailyCalories * 7;
        target.TargetDailyProtein = target.TargetDailyProtein * 7;
        target.TargetDailyCarbohydrates = target.TargetDailyCarbohydrates * 7;
        target.TargetDailyFat = target.TargetDailyFat * 7;
        target.TargetDailyFiber = target.TargetDailyFiber * 7;
        return target;
    }

    public async Task<UserNutritionTargets> GetOverviewUserNutritionTargetsAsync(UserInformationDto userInformation, int dayAmount)
    {
        var target = await GetUserNutritionTargetsAsync(userInformation);
        dayAmount = dayAmount <= 0 ? 1 : dayAmount;
        target.TargetDailyCalories = target.TargetDailyCalories * dayAmount;
        target.TargetDailyProtein = target.TargetDailyProtein * dayAmount;
        target.TargetDailyCarbohydrates = target.TargetDailyCarbohydrates * dayAmount;
        target.TargetDailyFat = target.TargetDailyFat * dayAmount;
        target.TargetDailyFiber = target.TargetDailyFiber * dayAmount;
        return target;
    }

    private async Task<UserNutritionTargets> GetOrCreateNutritionTargetsAsync(UserInformationDto userInformation, CancellationToken cancellationToken)
    {
        // Lấy existing targets
        var existingTargets = await repository.GetByUserIdAsync();

        if (existingTargets != null) return existingTargets;
        //USing AI to get targets
        var dailyTargets = await aiService.GetTargetDailyNutritionAsync(userInformation, cancellationToken);
        
        existingTargets = new UserNutritionTargets
        {
            UserId = userContextService.GetAuthenticatedUserId(),
            TargetDailyCalories = dailyTargets[0],
            TargetDailyProtein = dailyTargets[1],
            TargetDailyCarbohydrates = dailyTargets[2],
            TargetDailyFat = dailyTargets[3],
            TargetDailyFiber = dailyTargets[4],
        };

        // Lưu vào repository
        await repository.SaveNutrition(existingTargets);

        return existingTargets;
    }
}