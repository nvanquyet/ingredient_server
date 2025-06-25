using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs.Entity;

namespace IngredientServer.Core.Services;

public class NutritionService(IMealRepository mealRepository, IUserContextService userContextService)
    : INutritionService
{
    public async Task<DailyNutritionSummaryDto> GetDailyNutritionSummaryAsync(int userId, DateTime date)
    {
        if (userId != userContextService.GetAuthenticatedUserId())
        {
            throw new UnauthorizedAccessException("Access denied.");
        }

        // Lấy meals theo ngày và bao gồm foods
        var meals = await mealRepository.GetByDateAsync(date.ToString("yyyy-MM-dd"));
        var mealArray = meals as Meal[] ?? meals.ToArray();

        // Tính tổng dinh dưỡng từ các foods trong meals
        var summary = new DailyNutritionSummaryDto
        {
            Date = date,
            TotalCalories = mealArray.Sum(m => m.MealFoods.Sum(mf => mf.Food.Calories * mf.Food.Quantity)),
            TotalProtein = mealArray.Sum(m => m.MealFoods.Sum(mf => mf.Food.Protein * mf.Food.Quantity)),
            TotalCarbs = mealArray.Sum(m => m.MealFoods.Sum(mf => mf.Food.Carbs * mf.Food.Quantity)),
            TotalFat = mealArray.Sum(m => m.MealFoods.Sum(mf => mf.Food.Fat * mf.Food.Quantity))
        };

        return summary;
    }

    public async Task<WeeklyNutritionSummaryDto> GetWeeklyNutritionSummaryAsync(int userId, DateTime startDate, DateTime endDate)
    {
        if (userId != userContextService.GetAuthenticatedUserId())
        {
            throw new UnauthorizedAccessException("Access denied.");
        }

        var dailyBreakdown = new List<DailyNutritionSummaryDto>();
        double totalCalories = 0, totalProtein = 0, totalCarbs = 0, totalFat = 0;
        int totalMeals = 0, totalFoods = 0;
        int dayCount = (endDate - startDate).Days + 1;

        // Lặp qua từng ngày trong khoảng thời gian
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dailySummary = await GetDailyNutritionSummaryAsync(userId, date);
            dailyBreakdown.Add(dailySummary);

            // Cộng dồn để tính tổng
            totalCalories += dailySummary.TotalCalories;
            totalProtein += dailySummary.TotalProtein;
            totalCarbs += dailySummary.TotalCarbs;
            totalFat += dailySummary.TotalFat;

            // Đếm số meals và foods
            var meals = await mealRepository.GetByDateAsync(date.ToString("yyyy-MM-dd"));
            var mealArray = meals as Meal[] ?? meals.ToArray();
            totalMeals += mealArray.Length;
            totalFoods += mealArray.Sum(m => m.MealFoods.Count);
        }

        var weeklySummary = new WeeklyNutritionSummaryDto
        {
            WeekStart = startDate,
            WeekEnd = endDate,
            DailyBreakdown = dailyBreakdown,
            TotalCalories = totalCalories,
            TotalProtein = totalProtein,
            TotalCarbs = totalCarbs,
            TotalFat = totalFat,
            TotalMeals = totalMeals,
            TotalFoods = totalFoods,
            AverageCalories = dayCount > 0 ? totalCalories / dayCount : 0,
            AverageProtein = dayCount > 0 ? totalProtein / dayCount : 0,
            AverageCarbs = dayCount > 0 ? totalCarbs / dayCount : 0,
            AverageFat = dayCount > 0 ? totalFat / dayCount : 0
        };

        return weeklySummary;
    }

    public async Task<TotalNutritionSummaryDto> GetTotalNutritionSummaryAsync(int userId)
    {
        if (userId != userContextService.GetAuthenticatedUserId())
        {
            throw new UnauthorizedAccessException("Access denied.");
        }

        // Lấy toàn bộ meals của người dùng
        var meals = await mealRepository.GetAllAsync();
        var mealArray = meals as Meal[] ?? meals.ToArray();
        var sortedMeals = mealArray.OrderBy(m => m.MealDate).ToArray();
        // Tính số ngày duy nhất
        var distinctDays = sortedMeals.Select(m => m.MealDate.Date).Distinct().Count();
        // Tính tổng dinh dưỡng và số lượng meals/foods
        var totalSummary = new TotalNutritionSummaryDto
        {
            TotalCalories = sortedMeals.Sum(m => m.MealFoods.Sum(mf => mf.Food.Calories * mf.Food.Quantity)),
            TotalProtein = sortedMeals.Sum(m => m.MealFoods.Sum(mf => mf.Food.Protein * mf.Food.Quantity)),
            TotalCarbs = sortedMeals.Sum(m => m.MealFoods.Sum(mf => mf.Food.Carbs * mf.Food.Quantity)),
            TotalFat = sortedMeals.Sum(m => m.MealFoods.Sum(mf => mf.Food.Fat * mf.Food.Quantity)),
            TotalMeals = sortedMeals.Length,
            TotalFoods = sortedMeals.Sum(m => m.MealFoods.Count),
            AverageCalories = distinctDays > 0 ? sortedMeals.Sum(m => m.MealFoods.Sum(mf => mf.Food.Calories * mf.Food.Quantity)) / distinctDays : 0,
            AverageProtein = distinctDays > 0 ? sortedMeals.Sum(m => m.MealFoods.Sum(mf => mf.Food.Protein * mf.Food.Quantity)) / distinctDays : 0,
            AverageCarbs = distinctDays > 0 ? sortedMeals.Sum(m => m.MealFoods.Sum(mf => mf.Food.Carbs * mf.Food.Quantity)) / distinctDays : 0,
            AverageFat = distinctDays > 0 ? sortedMeals.Sum(m => m.MealFoods.Sum(mf => mf.Food.Fat * mf.Food.Quantity)) / distinctDays : 0
        };

        return totalSummary;
    }
}