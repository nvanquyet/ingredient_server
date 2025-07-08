// AIController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Core.Entities;
using System.Security.Claims;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Entity;
using Microsoft.Extensions.Logging;

namespace IngredientServer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AIController(IAIService _aiService, IUserContextService userContextService, ILogger<AIController> logger) : ControllerBase
    {

        /// <summary>
        /// Gợi ý món ăn dựa trên nguyên liệu có sẵn
        /// </summary>
        [HttpPost("detect_food")]
        public async Task<ActionResult<ApiResponse<List<FoodAnalysticResponseDto>>>> OnDetectFood(
            [FromForm] FoodAnalysticRequestDto? request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<FoodAnalysticResponseDto>
                {
                    Success = false,
                    Message = "Invalid model state",
                    Metadata = new Dictionary<string, List<string>?>
                    {
                        ["errors"] = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    }
                });
            }
            
            if(request?.Image == null)
            {
                return BadRequest(new ApiResponse<List<FoodAnalysticResponseDto>>
                {
                    Success = false,
                    Message = "Image is required"
                });
            }

            var response = await _aiService.GetFoodAnalysticAsync(request);
            response.NormalizeConsumedAt();
            return Ok(new ApiResponse<FoodAnalysticResponseDto>
            {
                Success = true,
                Data = response,
                Message = "Food analysis successful"
            });

        }

        /// <summary>
        /// Tạo công thức nấu ăn chi tiết
        /// </summary>
        [HttpPost("detect_ingredient")]
        public async Task<ActionResult<ApiResponse<IngredientAnalysticResponseDto>>> OnDetectIngredient(
            [FromBody] IngredientAnalysticRequestDto? request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<IngredientAnalysticResponseDto>
                {
                    Success = false,
                    Message = "Invalid model state",
                    Metadata = new Dictionary<string, List<string>?>
                    {
                        ["errors"] = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    }
                });
            }

            if (request?.Image == null)
            {
                return BadRequest(new ApiResponse<IngredientAnalysticResponseDto>
                {
                    Success = false,
                    Message = "Image is required"
                });
            }

            var response = await _aiService.GetIngredientAnalysticAsync(request);
            response.NormalizeExpiryDate();
            return Ok(new ApiResponse<IngredientAnalysticResponseDto>
            {
                Success = true,
                Data = response,
                Message = "Ingredient analysis successful"
            });
        }
    }
}
