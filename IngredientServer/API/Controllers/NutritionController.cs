// NutritionController.cs
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngredientServer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NutritionController(INutritionService nutritionService) : ControllerBase
{
    [HttpGet("daily/{userId}")]
    public async Task<ActionResult<ApiResponse<DailyNutritionSummaryDto>>> GetDailyNutritionSummary(
        int userId, 
        [FromQuery] DateTime date)
    {
        try
        {
            var summary = await nutritionService.GetDailyNutritionSummaryAsync(userId, date);
            return Ok(new ApiResponse<DailyNutritionSummaryDto>
            {
                Success = true,
                Data = summary,
                Message = "Daily nutrition summary retrieved successfully",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["userId"] = new List<string> { userId.ToString() },
                    ["date"] = new List<string> { date.ToString("yyyy-MM-dd") }
                }
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(new ApiResponse<DailyNutritionSummaryDto>
            {
                Success = false,
                Message = ex.Message
            }.ToString() ?? string.Empty);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<DailyNutritionSummaryDto>
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

    [HttpGet("weekly/{userId}")]
    public async Task<ActionResult<ApiResponse<WeeklyNutritionSummaryDto>>> GetWeeklyNutritionSummary(
        int userId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var summary = await nutritionService.GetWeeklyNutritionSummaryAsync(userId, startDate, endDate);
            return Ok(new ApiResponse<WeeklyNutritionSummaryDto>
            {
                Success = true,
                Data = summary,
                Message = "Weekly nutrition summary retrieved successfully",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["userId"] = new List<string> { userId.ToString() },
                    ["startDate"] = new List<string> { startDate.ToString("yyyy-MM-dd") },
                    ["endDate"] = new List<string> { endDate.ToString("yyyy-MM-dd") }
                }
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(new ApiResponse<WeeklyNutritionSummaryDto>
            {
                Success = false,
                Message = ex.Message
            }.ToString() ?? string.Empty);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<WeeklyNutritionSummaryDto>
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

    [HttpGet("total/{userId}")]
    public async Task<ActionResult<ApiResponse<TotalNutritionSummaryDto>>> GetTotalNutritionSummary(int userId)
    {
        try
        {
            var summary = await nutritionService.GetTotalNutritionSummaryAsync(userId);
            return Ok(new ApiResponse<TotalNutritionSummaryDto>
            {
                Success = true,
                Data = summary,
                Message = "Total nutrition summary retrieved successfully",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["userId"] = new List<string> { userId.ToString() }
                }
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(new ApiResponse<TotalNutritionSummaryDto>
            {
                Success = false,
                Message = ex.Message
            }.ToString() ?? string.Empty);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<TotalNutritionSummaryDto>
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
