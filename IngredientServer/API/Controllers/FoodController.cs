using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Ingredient;
using Microsoft.AspNetCore.Mvc;

namespace IngredientServer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FoodsController(
    IFoodRepository foodRepository,
    IIngredientRepository ingredientRepository,
    IMealRepository mealRepository,
    IExternalApiService externalApiService,
    IFoodService foodService)
    : ControllerBase
{
    private readonly IFoodRepository _foodRepository = foodRepository;
    private readonly IIngredientRepository _ingredientRepository = ingredientRepository;
    private readonly IMealRepository _mealRepository = mealRepository;
    private readonly IFoodService _foodService = foodService;

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FoodDto>>> CreateFood([FromBody] CreateFoodDto dto)
    {
        // Validate DTO
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<FoodDto>
            {
                Success = false,
                Message = "Invalid input data.",
                Metadata = ModelState.ToDictionary(
                    m => m.Key,
                    m => m.Value?.Errors.Select(e => e.ErrorMessage).ToList())
            });
        }

        // Map DTO to entity and save (handled by service for ingredient deduction and meal creation)
        var food = await _foodService.CreateFoodAsync(dto);
        return Ok(new ApiResponse<FoodDto>
        {
            Success = true,
            Data = MapToFoodDto(food),
            Message = "Food created successfully."
        });
    }

    [HttpPut("{foodId}")]
    public async Task<ActionResult<ApiResponse<FoodDto>>> UpdateFood(int foodId, [FromBody] UpdateFoodDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<FoodDto>
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
            var food = await _foodService.UpdateFoodAsync(foodId, dto);
            return Ok(new ApiResponse<FoodDto>
            {
                Success = true,
                Data = MapToFoodDto(food),
                Message = "Food updated successfully."
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<FoodDto> { Success = false, Message = "Access denied." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse<FoodDto> { Success = false, Message = "Food not found." });
        }
    }

    [HttpDelete("{foodId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteFood(int foodId)
    {
        try
        {
            var success = await _foodService.DeleteFoodAsync(foodId);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = success,
                Message = "Food deleted successfully."
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<bool> { Success = false, Message = "Access denied." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse<bool> { Success = false, Message = "Food not found." });
        }
    }

    [HttpGet("suggestions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<FoodSuggestionDto>>>> GetFoodSuggestions(
        [FromQuery] string? availableIngredients, [FromQuery] int? nutritionGoalId)
    {
        var request = new FoodSuggestionRequest
        {
            IngredientIds = availableIngredients?.Split(',').Select(int.Parse).ToList(),
            NutritionGoal = nutritionGoalId.HasValue ? (NutritionGoal)nutritionGoalId.Value : NutritionGoal.Balanced
        };

        var suggestions = await externalApiService.GetFoodSuggestionsAsync(request);
        return Ok(new ApiResponse<IEnumerable<FoodSuggestionDto>>
        {
            Success = true,
            Data = suggestions,
            Message = "Suggestions retrieved successfully."
        });
    }

    [HttpGet("recipe/{foodName}")]
    public async Task<ActionResult<ApiResponse<RecipeDto>>> GetRecipe(string foodName)
    {
        var request = new GetRecipeRequestDto { FoodName = foodName };
        var recipe = await externalApiService.GetRecipeAsync(request);
        return Ok(new ApiResponse<RecipeDto>
        {
            Success = true,
            Data = recipe,
            Message = "Recipe retrieved successfully."
        });
    }

    [HttpPost("recipe")]
    public async Task<ActionResult<ApiResponse<RecipeDto>>> GetRecipePost([FromBody] GetRecipeRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<RecipeDto>
            {
                Success = false,
                Message = "Invalid input data.",
                Metadata = ModelState.ToDictionary(
                    m => m.Key,
                    m => m.Value?.Errors.Select(e => e.ErrorMessage).ToList())
            });
        }

        var recipe = await externalApiService.GetRecipeAsync(request);
        return Ok(new ApiResponse<RecipeDto>
        {
            Success = true,
            Data = recipe,
            Message = "Recipe retrieved successfully."
        });
    }

    private FoodDto MapToFoodDto(Food food)
    {
        return new FoodDto
        {
            Id = food.Id,
            Name = food.Name,
            Description = food.Description,
            Quantity = food.Quantity,
            Calories = food.Calories,
            Protein = food.Protein,
            Carbs = food.Carbs,
            Fat = food.Fat,
            CreatedAt = food.CreatedAt,
            UpdatedAt = food.UpdatedAt,
            Ingredients = food.FoodIngredients.Select(fi => new FoodIngredientDto
            {
                IngredientId = fi.IngredientId,
                Quantity = fi.Quantity,
                Unit = fi.Unit,
                IngredientName = fi.Ingredient.Name
            }).ToList()
        };
    }
}