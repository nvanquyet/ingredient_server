using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngredientServer.API.Controllers;

// NutritionController.cs
[ApiController]
[Microsoft.AspNetCore.Components.Route("api/[controller]")]
[Authorize]
public class NutritionController(INutritionService nutritionService) : ControllerBase
{
    [HttpGet("daily/{userId}")]
    public async Task<ActionResult<DailyNutritionSummaryDto>> GetDailyNutritionSummary(
        int userId, 
        [FromQuery] DateTime date)
    {
        try
        {
            var summary = await nutritionService.GetDailyNutritionSummaryAsync(userId, date);
            return Ok(summary);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("weekly/{userId}")]
    public async Task<ActionResult<WeeklyNutritionSummaryDto>> GetWeeklyNutritionSummary(
        int userId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var summary = await nutritionService.GetWeeklyNutritionSummaryAsync(userId, startDate, endDate);
            return Ok(summary);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("total/{userId}")]
    public async Task<ActionResult<TotalNutritionSummaryDto>> GetTotalNutritionSummary(int userId)
    {
        try
        {
            var summary = await nutritionService.GetTotalNutritionSummaryAsync(userId);
            return Ok(summary);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
}