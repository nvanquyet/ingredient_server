using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// MealController.cs
namespace IngredientServer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MealController(IMealService mealService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<MealWithFoodsDto>> GetMealById(int id)
    {
        try
        {
            var meal = await mealService.GetByIdAsync(id);
            return Ok(meal);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("date/{date}")]
    public async Task<ActionResult<IEnumerable<MealWithFoodsDto>>> GetMealsByDate(string date)
    {
        try
        {
            var meals = await mealService.GetByDateAsync(date);
            return Ok(meals);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
    
}