using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngredientServer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MealController(IMealService mealService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<MealWithFoodsDto>>> GetMealById(int id)
    {
        try
        {
            var meal = await mealService.GetByIdAsync(id);
            return Ok(new ApiResponse<MealWithFoodsDto>
            {
                Success = true,
                Data = meal,
                Message = "Meal retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<MealWithFoodsDto>
            {
                Success = false,
                Message = "Internal server error",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["details"] = new List<string> { ex.Message }
                }
            });
        }
    }

    [HttpGet("date/{date}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MealWithFoodsDto>>>> GetMealsByDate(string date)
    {
        try
        {
            var meals = await mealService.GetByDateAsync(date);
            var mealWithFoodsDtos = meals as MealWithFoodsDto[] ?? meals.ToArray();
            return Ok(new ApiResponse<IEnumerable<MealWithFoodsDto>>
            {
                Success = true,
                Data = mealWithFoodsDtos,
                Message = "Meals retrieved successfully",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["count"] = new List<string> { mealWithFoodsDtos.Count().ToString() },
                    ["date"] = new List<string> { date }
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IEnumerable<MealWithFoodsDto>>
            {
                Success = false,
                Message = "Internal server error",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["details"] = new List<string> { ex.Message }
                }
            });
        }
    }
}
