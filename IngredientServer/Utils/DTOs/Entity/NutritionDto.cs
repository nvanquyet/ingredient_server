﻿using IngredientServer.Core.Entities;

namespace IngredientServer.Utils.DTOs.Entity;
public class FoodNutritionDto
{
    public int FoodId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public decimal Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbs { get; set; }
    public decimal Fat { get; set; }
    public decimal Fiber { get; set; }
}
public class NutritionDto
{
    public int MealId { get; set; }
    public MealType MealType { get; set; }
    public DateTime MealDate { get; set; }
    
    public double TotalCalories { get; set; }
    public double TotalProtein { get; set; }
    public double TotalCarbs { get; set; }
    public double TotalFat { get; set; }
    public double TotalFiber { get; set; }
    
    public IEnumerable<FoodNutritionDto> Foods { get; set; } = new List<FoodNutritionDto>();
    
    // Normalize meal date to UTC
    public void NormalizeMealDate()
    {
        MealDate = MealDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(MealDate, DateTimeKind.Local).ToUniversalTime()
            : MealDate.ToUniversalTime();
    }
}

public class DailyNutritionSummaryDto
{
    public DateTime Date { get; set; }
    public double TotalCalories { get; set; }
    public double TotalProtein { get; set; }
    public double TotalCarbs { get; set; }
    public double TotalFat { get; set; }
    public double TotalFiber { get; set; }
    
    // User's targets (if available)
    public double? TargetCalories { get; set; }
    public double? TargetProtein { get; set; }
    public double? TargetCarbs { get; set; }
    public double? TargetFat { get; set; }
    public double? TargetFiber { get; set; }

    public IEnumerable<NutritionDto> MealBreakdown { get; set; } = new List<NutritionDto>();
    
    //Normalize date to UTC
    public void NormalizeDate()
    {
        Date = Date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(Date, DateTimeKind.Local).ToUniversalTime()
            : Date.ToUniversalTime();
    }
}

public class WeeklyNutritionSummaryDto
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public double AverageCalories { get; set; }
    public double AverageProtein { get; set; }
    public double AverageCarbs { get; set; }
    public double AverageFat { get; set; }
    public double AverageFiber { get; set; }
    
    public double? TargetCalories { get; set; }
    public double? TargetProtein { get; set; }
    public double? TargetCarbs { get; set; }
    public double? TargetFat { get; set; }
    public double? TargetFiber { get; set; }

    public IEnumerable<DailyNutritionSummaryDto> DailyBreakdown { get; set; } = new List<DailyNutritionSummaryDto>();
    
    // Normalize week start and end dates to UTC
    public void NormalizeWeekDates()
    {
        WeekStart = WeekStart.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(WeekStart, DateTimeKind.Local).ToUniversalTime()
            : WeekStart.ToUniversalTime();
        
        WeekEnd = WeekEnd.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(WeekEnd, DateTimeKind.Local).ToUniversalTime()
            : WeekEnd.ToUniversalTime();
    }
}

public class OverviewNutritionSummaryDto
{
    public double AverageCalories { get; set; }
    public double AverageProtein { get; set; }
    public double AverageCarbs { get; set; }
    public double AverageFat { get; set; }
    public double AverageFiber { get; set; }
    
    public double? TargetCalories { get; set; }
    public double? TargetProtein { get; set; }
    public double? TargetCarbs { get; set; }
    public double? TargetFat { get; set; }
    public double? TargetFiber { get; set; }
}
    