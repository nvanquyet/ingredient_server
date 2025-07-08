using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Entity;

namespace IngredientServer.Core.Services;

public class NutritionService(
    INutritionTargetsService nutritionTargetsService,
    IMealRepository mealRepository,
    IMealFoodRepository mealFoodRepository,
    IFoodRepository foodRepository,
    IUserContextService userContextService)
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

        // Định nghĩa các loại bữa ăn cần thiết
        var requiredMealTypes = new List<MealType>
            { MealType.Breakfast, MealType.Lunch, MealType.Dinner, MealType.Snack, MealType.Other };

        // Lấy meals theo ngày
        var existingMeals =
            await mealRepository.GetByDateAsync(userNutritionRequestDto.CurrentDate.ToString("yyyy-MM-dd"));
        var mealList = existingMeals?.ToList() ?? new List<Meal>();

        // FILTER MEALS THEO ĐÚNG NGÀY ĐỂ TRÁNH LẤY DATA NGÀY KHÁC
        var filteredMeals = mealList
            .Where(m => m.MealDate.Date == userNutritionRequestDto.CurrentDate.Date)
            .ToList();

        // Group meals theo MealType và chỉ lấy meal mới nhất cho mỗi loại (trong cùng ngày)
        var groupedMeals = filteredMeals
            .GroupBy(m => m.MealType)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(m => m.MealDate).First());

        // Tạo danh sách meals cuối cùng - đảm bảo có đủ 5 loại
        var finalMeals = new List<Meal>();
        var currentUserId = userContextService.GetAuthenticatedUserId();

        foreach (var mealType in requiredMealTypes)
        {
            if (groupedMeals.ContainsKey(mealType))
            {
                // Sử dụng meal có sẵn (đã được filter theo đúng ngày)
                finalMeals.Add(groupedMeals[mealType]);
            }
            else
            {
                // Tạo meal mới nếu thiếu
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

        // Sắp xếp lại theo thứ tự mong muốn (theo thứ tự enum)
        var orderedMeals = finalMeals.OrderBy(m => (int)m.MealType).ToList();

        // Xử lý từng meal
        foreach (var meal in orderedMeals)
        {
            // Lấy foods trong meal (chỉ với meal có Id > 0)
            var foodsInMeal = meal.Id > 0
                ? await mealFoodRepository.GetByMealIdAsync(meal.Id)
                : new List<MealFood>();

            // Reset nutrition values để tính toán lại
            meal.TotalCalories = 0;
            meal.TotalProtein = 0;
            meal.TotalCarbs = 0;
            meal.TotalFat = 0;
            meal.TotalFiber = 0;

            var foodNutrition = new List<FoodNutritionDto>();

            // Kiểm tra có foods trong meal không
            if (foodsInMeal != null && foodsInMeal.Any())
            {
                foreach (var mealFood in foodsInMeal)
                {
                    // Kiểm tra food có tồn tại không
                    if (mealFood.Food == null) continue;

                    // Tính nutrition với null check và đảm bảo không null
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

            // Cộng dồn vào tổng
            result.TotalCalories += meal.TotalCalories;
            result.TotalProtein += meal.TotalProtein;
            result.TotalCarbs += meal.TotalCarbs;
            result.TotalFat += meal.TotalFat;
            result.TotalFiber += meal.TotalFiber;

            // Tạo nutrition DTO
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

        // Using AI to get Target Nutrition value 
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
                }
            }
            catch (Exception ex)
            {
                // Log error nhưng không throw để không ảnh hưởng đến kết quả chính
                // logger.LogError(ex, "Error getting nutrition targets for user");
            }
        }

        result.MealBreakdown = mealBreakdown;
        result.NormalizeDate();
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