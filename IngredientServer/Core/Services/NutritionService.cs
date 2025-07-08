using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Entity;
using Microsoft.Extensions.Logging;

namespace IngredientServer.Core.Services;

public class NutritionService(
    INutritionTargetsService nutritionTargetsService,
    IMealRepository mealRepository,
    IMealFoodRepository mealFoodRepository,
    IUserContextService userContextService,
    ILogger<NutritionService> logger)
    : INutritionService
{
    public async Task<DailyNutritionSummaryDto> GetDailyNutritionSummaryAsync(
        UserNutritionRequestDto userNutritionRequestDto, bool usingAIAssistant = false)
    {
        var mealBreakdown = new List<NutritionDto>();
        var result = new DailyNutritionSummaryDto()
        {
            Date = userNutritionRequestDto.CurrentDate,
            TotalCalories = 0,
            TotalProtein = 0,
            TotalCarbs = 0,
            TotalFat = 0,
            TotalFiber = 0
        };

        var currentUserId = userContextService.GetAuthenticatedUserId();

        logger.LogInformation("Getting daily nutrition summary for date {Date}, userId: {UserId}, usingAI: {UseAI}",
            userNutritionRequestDto.CurrentDate.ToString("yyyy-MM-dd"), currentUserId, usingAIAssistant);

        var requiredMealTypes = new List<MealType>
            { MealType.Breakfast, MealType.Lunch, MealType.Dinner, MealType.Snack, MealType.Other };

        var existingMeals =
            await mealRepository.GetByDateAsync(userNutritionRequestDto.CurrentDate.ToString("yyyy-MM-dd"));
        var mealList = existingMeals?.ToList() ?? new List<Meal>();

        logger.LogInformation("Found {MealCount} meals in database for date {Date}",
            mealList.Count, userNutritionRequestDto.CurrentDate.ToString("yyyy-MM-dd"));

        var filteredMeals = mealList
            .Where(m => m.MealDate.Date == userNutritionRequestDto.CurrentDate.Date)
            .ToList();

        if (!filteredMeals.Any())
        {
            logger.LogWarning("No meals found for the requested date {Date}", userNutritionRequestDto.CurrentDate);
        }

        var groupedMeals = filteredMeals
            .GroupBy(m => m.MealType)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(m => m.MealDate).First());

        logger.LogInformation("Grouped meals into {GroupCount} meal types", groupedMeals.Count);

        var finalMeals = new List<Meal>();

        foreach (var mealType in requiredMealTypes)
        {
            if (groupedMeals.ContainsKey(mealType))
            {
                finalMeals.Add(groupedMeals[mealType]);
            }
            else
            {
                finalMeals.Add(new Meal
                {
                    MealType = mealType,
                    MealDate = userNutritionRequestDto.CurrentDate,
                    TotalCalories = 0,
                    TotalProtein = 0,
                    TotalCarbs = 0,
                    TotalFat = 0,
                    TotalFiber = 0,
                    UserId = currentUserId
                });
            }
        }

        var orderedMeals = finalMeals.OrderBy(m => (int)m.MealType).ToList();

        foreach (var meal in orderedMeals)
        {
            var foodsInMeal = meal.Id > 0
                ? await mealFoodRepository.GetByMealIdAsync(meal.Id)
                : new List<MealFood>();

            logger.LogInformation("Processing meal {MealType} (Id: {MealId}) with {FoodCount} foods",
                meal.MealType, meal.Id, foodsInMeal.Count());

            meal.TotalCalories = 0;
            meal.TotalProtein = 0;
            meal.TotalCarbs = 0;
            meal.TotalFat = 0;
            meal.TotalFiber = 0;

            var foodNutrition = new List<FoodNutritionDto>();

            if (foodsInMeal != null && foodsInMeal.Any())
            {
                foreach (var mealFood in foodsInMeal)
                {
                    if (mealFood.Food == null) continue;

                    var calories = mealFood.Food.Calories;
                    var protein = mealFood.Food.Protein;
                    var carbs = mealFood.Food.Carbohydrates;
                    var fat = mealFood.Food.Fat;
                    var fiber = mealFood.Food.Fiber;

                    meal.TotalCalories += (double)calories;
                    meal.TotalProtein += (double)protein;
                    meal.TotalCarbs += (double)carbs;
                    meal.TotalFat += (double)fat;
                    meal.TotalFiber += (double)fiber;

                    foodNutrition.Add(new FoodNutritionDto()
                    {
                        FoodId = mealFood.Food.Id,
                        FoodName = mealFood.Food.Name,
                        Calories = calories,
                        Protein = protein,
                        Carbs = carbs,
                        Fat = fat,
                        Fiber = fiber
                    });
                }
            }

            logger.LogInformation(
                "Meal {MealType} nutrition: Calories={Calories}, Protein={Protein}, Carbs={Carbs}, Fat={Fat}, Fiber={Fiber}",
                meal.MealType, meal.TotalCalories, meal.TotalProtein, meal.TotalCarbs, meal.TotalFat, meal.TotalFiber);

            result.TotalCalories += meal.TotalCalories;
            result.TotalProtein += meal.TotalProtein;
            result.TotalCarbs += meal.TotalCarbs;
            result.TotalFat += meal.TotalFat;
            result.TotalFiber += meal.TotalFiber;

            var nutritionDto = new NutritionDto()
            {
                MealId = meal.Id,
                MealType = meal.MealType,
                MealDate = meal.MealDate,
                TotalCalories = meal.TotalCalories,
                TotalProtein = meal.TotalProtein,
                TotalCarbs = meal.TotalCarbs,
                TotalFat = meal.TotalFat,
                TotalFiber = meal.TotalFiber,
                Foods = foodNutrition
            };

            nutritionDto.NormalizeMealDate();
            mealBreakdown.Add(nutritionDto);
        }

        if (usingAIAssistant)
        {
            try
            {
                var targetValue = await nutritionTargetsService.GetDailyUserNutritionTargetsAsync(
                    userNutritionRequestDto.UserInformationDto);

                if (targetValue != null)
                {
                    result.TargetCalories = (double)(targetValue.TargetDailyCalories);
                    result.TargetProtein = (double)(targetValue.TargetDailyProtein);
                    result.TargetCarbs = (double)(targetValue.TargetDailyCarbohydrates);
                    result.TargetFat = (double)(targetValue.TargetDailyFat);
                    result.TargetFiber = (double)(targetValue.TargetDailyFiber);

                    logger.LogInformation(
                        "AI Nutrition Targets: Cal={Calories}, Protein={Protein}, Carbs={Carbs}, Fat={Fat}, Fiber={Fiber}",
                        result.TargetCalories, result.TargetProtein, result.TargetCarbs, result.TargetFat,
                        result.TargetFiber);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting nutrition targets for userId {UserId}", currentUserId);
            }
        }

        result.MealBreakdown = mealBreakdown;
        result.NormalizeDate();

        logger.LogInformation(
            "Finished summary for date {Date}: TotalCalories={Calories}, Protein={Protein}, Carbs={Carbs}, Fat={Fat}, Fiber={Fiber}",
            result.Date, result.TotalCalories, result.TotalProtein, result.TotalCarbs, result.TotalFat,
            result.TotalFiber);

        return result;
    }

    public async Task<WeeklyNutritionSummaryDto> GetWeeklyNutritionSummaryAsync(
        UserNutritionRequestDto userNutritionRequestDto)
    {
        var dailyBreakdown = new List<DailyNutritionSummaryDto>();
        double totalCalories = 0, totalProtein = 0, totalCarbs = 0, totalFat = 0, totalFiber = 0;
        int dayCount = (userNutritionRequestDto.EndDate - userNutritionRequestDto.StartDate).Days + 1;
        var userInformation = userNutritionRequestDto.UserInformationDto;
        // Lặp qua từng ngày trong khoảng thời gian
        for (var date = userNutritionRequestDto.StartDate;
             date <= userNutritionRequestDto.EndDate;
             date = date.AddDays(1))
        {
            userNutritionRequestDto.CurrentDate = date;
            var dailySummary = await GetDailyNutritionSummaryAsync(userNutritionRequestDto);
            dailyBreakdown.Add(dailySummary);

            // Cộng dồn để tính tổng
            totalCalories += dailySummary.TotalCalories;
            totalProtein += dailySummary.TotalProtein;
            totalCarbs += dailySummary.TotalCarbs;
            totalFat += dailySummary.TotalFat;
            totalFiber += dailySummary.TotalFiber;
        }

        var weeklySummary = new WeeklyNutritionSummaryDto
        {
            WeekStart = userNutritionRequestDto.StartDate,
            WeekEnd = userNutritionRequestDto.EndDate,
            DailyBreakdown = dailyBreakdown,

            // AverageCalories = dayCount > 0 ? totalCalories / dayCount : 0,
            // AverageProtein = dayCount > 0 ? totalProtein / dayCount : 0,
            // AverageCarbs = dayCount > 0 ? totalCarbs / dayCount : 0,
            // AverageFat = dayCount > 0 ? totalFat / dayCount : 0,
            // AverageFiber = dayCount > 0 ? totalFiber / dayCount : 0

            AverageCalories = totalCalories,
            AverageProtein = totalProtein,
            AverageCarbs = totalCarbs,
            AverageFat = totalFat,
            AverageFiber = totalFiber
        };

        //Using AI to get Target Avg Nutrition value
        var targetValue =
            await nutritionTargetsService.GetWeeklyUserNutritionTargetsAsync(userNutritionRequestDto
                .UserInformationDto);
        weeklySummary.TargetCalories = (double)targetValue.TargetDailyCalories;
        weeklySummary.TargetProtein = (double)targetValue.TargetDailyProtein;
        weeklySummary.TargetCarbs = (double)targetValue.TargetDailyCarbohydrates;
        weeklySummary.TargetFat = (double)targetValue.TargetDailyFat;
        weeklySummary.TargetFiber = (double)targetValue.TargetDailyFiber;

        weeklySummary.NormalizeWeekDates();
        return weeklySummary;
    }

    public async Task<OverviewNutritionSummaryDto> GetOverviewNutritionSummaryAsync(UserInformationDto userInformation)
    {
        double totalCalories = 0, totalProtein = 0, totalCarbs = 0, totalFat = 0, totalFiber = 0;
        // Lấy toàn bộ meals của người dùng
        var meals = await mealRepository.GetAllAsync();
        var mealArray = meals as Meal[] ?? meals.ToArray();
        var sortedMeals = mealArray.OrderBy(m => m.MealDate).ToArray();

        //Laays danh sach ngay ton tai
        var existingDays = sortedMeals.Select(m => m.MealDate.Date).Distinct().ToList();
        var userRequest = new UserNutritionRequestDto
        {
            UserInformationDto = userInformation
        };
        foreach (var existingDay in existingDays)
        {
            userRequest.CurrentDate = existingDay;
            var dailySummary = await GetDailyNutritionSummaryAsync(userRequest);

            // Cộng dồn để tính tổng
            totalCalories += dailySummary.TotalCalories;
            totalProtein += dailySummary.TotalProtein;
            totalCarbs += dailySummary.TotalCarbs;
            totalFat += dailySummary.TotalFat;
            totalFiber += dailySummary.TotalFiber;
        }

        var result = new OverviewNutritionSummaryDto
        {
            AverageCalories = existingDays.Count > 0 ? totalCalories / existingDays.Count : 0,
            AverageProtein = totalCalories > 0 ? totalProtein / existingDays.Count : 0,
            AverageCarbs = totalCalories > 0 ? totalCarbs / existingDays.Count : 0,
            AverageFat = totalCalories > 0 ? totalFat / existingDays.Count : 0,
            AverageFiber = totalCalories > 0 ? totalFiber / existingDays.Count : 0,
        };

        var targetValue =
            await nutritionTargetsService.GetOverviewUserNutritionTargetsAsync(userInformation, 1);
        result.TargetCalories = (double)targetValue.TargetDailyCalories;
        result.TargetProtein = (double)targetValue.TargetDailyProtein;
        result.TargetCarbs = (double)targetValue.TargetDailyCarbohydrates;
        result.TargetFat = (double)targetValue.TargetDailyFat;
        result.TargetFiber = (double)targetValue.TargetDailyFiber;
        return result;
    }
}