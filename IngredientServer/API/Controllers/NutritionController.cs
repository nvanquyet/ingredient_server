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
public class NutritionController(INutritionService nutritionService, IUserContextService userContextService)
    : ControllerBase
{
    [HttpPost("daily")]
    public async Task<ActionResult<ApiResponse<DailyNutritionSummaryDto>>> GetDailyNutritionSummary(
        [FromBody] UserNutritionRequestDto userNutritionRequestDto)
    {
        try
        {
            if (userNutritionRequestDto.CurrentDate == default)
            {
                userNutritionRequestDto.CurrentDate = DateTime.UtcNow.Date; // Default to today if no date is provided
            }
            var summary = await nutritionService.GetDailyNutritionSummaryAsync(userNutritionRequestDto, true);
            return Ok(new ApiResponse<DailyNutritionSummaryDto>
            {
                Success = true,
                Data = summary,
                Message = "Daily nutrition summary retrieved successfully",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["date"] = new List<string> { userNutritionRequestDto.CurrentDate.ToString("yyyy-MM-dd") }
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

    [HttpPost("weekly")]
    public async Task<ActionResult<ApiResponse<WeeklyNutritionSummaryDto>>> GetWeeklyNutritionSummary(
        [FromBody] UserNutritionRequestDto userNutritionRequestDto)
    {
        try
        {
            var userId = userContextService.GetAuthenticatedUserId();
            var summary = await nutritionService.GetWeeklyNutritionSummaryAsync(userNutritionRequestDto);
            return Ok(new ApiResponse<WeeklyNutritionSummaryDto>
            {
                Success = true,
                Data = summary,
                Message = "Weekly nutrition summary retrieved successfully",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["userId"] = new List<string> { userId.ToString() },
                    ["startDate"] = new List<string> { userNutritionRequestDto.StartDate.ToString("yyyy-MM-dd") },
                    ["endDate"] = new List<string> { userNutritionRequestDto.EndDate.ToString("yyyy-MM-dd") }
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

    [HttpPost("overview")]
    public async Task<ActionResult<ApiResponse<OverviewNutritionSummaryDto>>> GetOverviewNutritionSummary([FromBody] UserInformationDto userInformation)
    {
        try
        {
            var userId = userContextService.GetAuthenticatedUserId();
            var summary = await nutritionService.GetOverviewNutritionSummaryAsync(userInformation);
            return Ok(new ApiResponse<OverviewNutritionSummaryDto>
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
            return Forbid(new ApiResponse<OverviewNutritionSummaryDto>
            {
                Success = false,
                Message = ex.Message
            }.ToString() ?? string.Empty);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<OverviewNutritionSummaryDto>
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