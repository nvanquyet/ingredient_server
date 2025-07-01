using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Entity;

namespace IngredientServer.Core.Services;

public class NutritionService(IAIService aiService,IMealRepository mealRepository, IMealFoodRepository mealFoodRepository, IFoodRepository foodRepository, IUserContextService userContextService)
    : INutritionService
{
    public async Task<DailyNutritionSummaryDto> GetDailyNutritionSummaryAsync(DateTime date, UserInformationDto userInformation, bool usingAIAssistant = false)
    {
        var mealBreakdown = new List<NutritionDto>();
        var result = new DailyNutritionSummaryDto()
        {
            Date = date,
            TotalCalories = 0,
            TotalProtein = 0,
            TotalCarbs = 0,
            TotalFat = 0,
        };
        // Lấy meals theo ngày và bao gồm foods
        var meals = await mealRepository.GetByDateAsync(date.ToString("yyyy-MM-dd"));
        var mealArray = meals as Meal[] ?? meals.ToArray();
        foreach (var meal in mealArray)
        {
            var foodsInMeal = await mealFoodRepository.GetByMealIdAsync(meal.Id);
            meal.TotalCalories = 0;
            meal.TotalProtein = 0;
            meal.TotalCarbs = 0;
            meal.TotalFat = 0;
            var foodNutrition = new List<FoodNutritionDto>();
            foreach (var f in foodsInMeal)
            {
                meal.TotalCalories += (double) f.Food.Calories;
                meal.TotalProtein += (double) f.Food.Protein;
                meal.TotalCarbs += (double) f.Food.Carbohydrates;
                meal.TotalFat += (double) f.Food.Fat;
                meal.TotalFiber += (double) f.Food.Fiber;
                
                foodNutrition.Add(new FoodNutritionDto()
                {
                    FoodId = f.Food.Id,
                    FoodName = f.Food.Name,
                    Calories = f.Food.Calories,
                    Protein = f.Food.Protein,
                    Carbs = f.Food.Carbohydrates,
                    Fat = f.Food.Fat,
                    Fiber = f.Food.Fiber
                });
            }
            
            result.TotalCalories += meal.TotalCalories;
            result.TotalProtein += meal.TotalProtein;
            result.TotalCarbs += meal.TotalCarbs;
            result.TotalFat += meal.TotalFat;
            
            mealBreakdown.Add(new NutritionDto()
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
            });
        }
        
        //Using AI to get Target Nutrition value 
        if (usingAIAssistant)
        {
            var targetValue = await aiService.GetTargetDailyNutritionAsync(userInformation);
            result.TargetCalories = targetValue[0];
            result.TargetProtein = targetValue[1];
            result.TargetCarbs = targetValue[2];
            result.TargetFat = targetValue[3];
            result.TargetFiber = targetValue[4];
        }
        
        result.MealBreakdown = mealBreakdown;
        return result;
    }

    public async Task<WeeklyNutritionSummaryDto> GetWeeklyNutritionSummaryAsync(DateTime startDate, DateTime endDate,UserInformationDto userInformation)
    {
        var dailyBreakdown = new List<DailyNutritionSummaryDto>();
        double totalCalories = 0, totalProtein = 0, totalCarbs = 0, totalFat = 0, totalFiber = 0;
        int dayCount = (endDate - startDate).Days + 1;

        // Lặp qua từng ngày trong khoảng thời gian
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dailySummary = await GetDailyNutritionSummaryAsync(date, userInformation);
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
            WeekStart = startDate,
            WeekEnd = endDate,
            DailyBreakdown = dailyBreakdown,
            
            AverageCalories = dayCount > 0 ? totalCalories / dayCount : 0,
            AverageProtein = dayCount > 0 ? totalProtein / dayCount : 0,
            AverageCarbs = dayCount > 0 ? totalCarbs / dayCount : 0,
            AverageFat = dayCount > 0 ? totalFat / dayCount : 0,
            AverageFiber = dayCount > 0 ? totalFiber / dayCount : 0
        };
        
        //Using AI to get Target Avg Nutrition value
        var targetValue = await aiService.GetTargetWeeklyNutritionAsync(userInformation);
        weeklySummary.TargetCalories = targetValue[0];
        weeklySummary.TargetProtein = targetValue[1];
        weeklySummary.TargetCarbs = targetValue[2];
        weeklySummary.TargetFat = targetValue[3];
        weeklySummary.TargetFiber = targetValue[4];
        
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
        foreach (var existingDay in existingDays)
        {
            var dailySummary = await GetDailyNutritionSummaryAsync(existingDay, userInformation);

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
        
        //Using AI to get Target Avg Nutrition value
        var targetValue = await aiService.GetTargetOverviewNutritionAsync(userInformation, existingDays.Count);
        result.TargetCalories = targetValue[0];
        result.TargetProtein = targetValue[1];
        result.TargetCarbs = targetValue[2];
        result.TargetFat = targetValue[3];
        result.TargetFiber = targetValue[4];
        return result;
    }
}