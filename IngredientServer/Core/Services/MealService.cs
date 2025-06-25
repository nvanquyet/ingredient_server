using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Ingredient;
using IngredientServer.Utils.DTOs.Meal;
using Microsoft.AspNetCore.Mvc;

namespace IngredientServer.Core.Services;

[ApiController]
[Route("api/[controller]")]
public class MealsController : ControllerBase
{
    private readonly IMealService _mealService;

    public MealsController(IMealService mealService)
    {
        _mealService = mealService;
    }

    [HttpGet("{mealId}")]
    public async Task<IActionResult> GetMeal(int mealId)
    {
        try
        {
            var meal = await _mealService.GetByIdAsync(mealId);
            return Ok(new ApiResponse<MealWithFoodsDto>
            {
                Success = true,
                Data = meal,
                Message = "Meal retrieved successfully."
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<ErrorDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<ErrorDto>
            {
                Success = false,
                Message = "Access denied."
            });
        }
    }

    [HttpGet("by-date/{date}")]
    public async Task<IActionResult> GetMealsByDate(string date, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var meals = await _mealService.GetByDateAsync(date, pageNumber, pageSize);
            return Ok(new ApiResponse<IEnumerable<MealWithFoodsDto>>
            {
                Success = true,
                Data = meals,
                Message = "Meals retrieved successfully."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<ErrorDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateMeal([FromBody] MealDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<ErrorDto>
            {
                Success = false,
                Message = "Invalid input data.",
                Metadata = ModelState.ToDictionary(
                    m => m.Key,
                    m => m.Value?.Errors.Select(e => e.ErrorMessage).ToList())
            });
        }

        try
        {
            var meal = await _mealService.CreateMealAsync(request.MealType, request.MealDate);
            return Ok(new ApiResponse<MealWithFoodsDto>
            {
                Success = true,
                Data = MapToMealWithFoodsDto(meal),
                Message = "Meal created successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<ErrorDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPut("{mealId}")]
    public async Task<IActionResult> UpdateMeal(int mealId, [FromBody] MealDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<ErrorDto>
            {
                Success = false,
                Message = "Invalid input data.",
                Metadata = ModelState.ToDictionary(
                    m => m.Key,
                    m => m.Value?.Errors.Select(e => e.ErrorMessage).ToList())
            });
        }

        try
        {
            var meal = await _mealService.UpdateMealAsync(mealId, request);
            return Ok(new ApiResponse<MealWithFoodsDto>
            {
                Success = true,
                Data = MapToMealWithFoodsDto(meal),
                Message = "Meal updated successfully."
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<ErrorDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<ErrorDto>
            {
                Success = false,
                Message = "Access denied."
            });
        }
    }

    [HttpDelete("{mealId}")]
    public async Task<IActionResult> DeleteMeal(int mealId)
    {
        try
        {
            var success = await _mealService.DeleteMealAsync(mealId);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = success,
                Message = "Meal deleted successfully."
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<ErrorDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<ErrorDto>
            {
                Success = false,
                Message = "Access denied."
            });
        }
    }

    private MealWithFoodsDto MapToMealWithFoodsDto(Meal meal)
    {
        return new MealWithFoodsDto
        {
            Id = meal.Id,
            MealType = meal.MealType,
            MealDate = meal.MealDate,
            ConsumedAt = meal.ConsumedAt,
            TotalCalories = meal.TotalCalories,
            TotalProtein = meal.TotalProtein,
            TotalCarbs = meal.TotalCarbs,
            TotalFat = meal.TotalFat,
            FoodCount = meal.FoodCount,
            CreatedAt = meal.CreatedAt,
            UpdatedAt = meal.UpdatedAt,
            Foods = meal.MealFoods.Select(mf => new FoodDto
            {
                Id = mf.Food.Id,
                Name = mf.Food.Name,
                Description = mf.Food.Description,
                Quantity = mf.Food.Quantity,
                Calories = mf.Food.Calories,
                Protein = mf.Food.Protein,
                Carbs = mf.Food.Carbs,
                Fat = mf.Food.Fat,
                CreatedAt = mf.Food.CreatedAt,
                UpdatedAt = mf.Food.UpdatedAt,
                Ingredients = mf.Food.FoodIngredients.Select(fi => new FoodIngredientDto
                {
                    IngredientId = fi.IngredientId,
                    Quantity = fi.Quantity,
                    Unit = fi.Unit,
                    IngredientName = fi.Ingredient.Name
                }).ToList()
            }).ToList()
        };
    }
}