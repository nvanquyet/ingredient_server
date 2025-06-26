using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngredientServer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FoodController(IFoodService foodService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Food>>> CreateFood([FromBody] FoodDataDto dataDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<Food>
                {
                    Success = false,
                    Message = "Invalid model state",
                    Metadata = new Dictionary<string, List<string>?>
                    {
                        ["errors"] = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    }
                });
            }
            var food = await foodService.CreateFoodAsync(dataDto);
            return CreatedAtAction(
                nameof(GetFood), 
                new { id = food.Id }, 
                new ApiResponse<Food>
                {
                    Success = true,
                    Data = food,
                    Message = "Food created successfully"
                });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<Food>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<Food>
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

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Food>>> UpdateFood(int id, [FromBody] FoodDataDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<Food>
                {
                    Success = false,
                    Message = "Invalid model state",
                    Metadata = new Dictionary<string, List<string>?>
                    {
                        ["errors"] = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    }
                });
            }
            var food = await foodService.UpdateFoodAsync(id, dto);
            return Ok(new ApiResponse<Food>
            {
                Success = true,
                Data = food,
                Message = "Food updated successfully"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return NotFound(new ApiResponse<Food>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<Food>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<Food>
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

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteFood(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Invalid food ID"
                });
            }
            var result = await foodService.DeleteFoodAsync(id);
            if (result)
            {
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Food deleted successfully"
                });
            }
            
            return NotFound(new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                Message = "Food not found"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                Message = "Internal server error",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["details"] = new List<string> { ex.Message }
                }
            });
        }
    }

    [HttpPost("suggestions")]
    public async Task<ActionResult<ApiResponse<List<FoodSuggestionDto>>>> GetSuggestions([FromBody] FoodSuggestionRequestDto requestDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<List<FoodSuggestionDto>>
                {
                    Success = false,
                    Message = "Invalid model state",
                    Metadata = new Dictionary<string, List<string>?>
                    {
                        ["errors"] = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    }
                });
            }
            var suggestions = await foodService.GetSuggestionsAsync(requestDto);
            return Ok(new ApiResponse<List<FoodSuggestionDto>>
            {
                Success = true,
                Data = suggestions,
                Message = "Food suggestions retrieved successfully",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["count"] = new List<string> { suggestions.Count.ToString() }
                }
            });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new ApiResponse<List<FoodSuggestionDto>>
            {
                Success = false,
                Message = "Service unavailable",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["details"] = new List<string> { ex.Message }
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<List<FoodSuggestionDto>>
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

    [HttpPost("recipes")]
    public async Task<ActionResult<ApiResponse<FoodRecipeDto>>> GetRecipeSuggestions([FromBody] FoodRecipeRequestDto recipeRequest)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<FoodRecipeDto>
                {
                    Success = false,
                    Message = "Invalid model state",
                    Metadata = new Dictionary<string, List<string>?>
                    {
                        ["errors"] = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    }
                });
            }
            var recipe = await foodService.GetRecipeSuggestionsAsync(recipeRequest);
            return Ok(new ApiResponse<FoodRecipeDto>
            {
                Success = true,
                Data = recipe,
                Message = "Recipe retrieved successfully"
            });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new ApiResponse<FoodRecipeDto>
            {
                Success = false,
                Message = "Service unavailable",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["details"] = new List<string> { ex.Message }
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<FoodRecipeDto>
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

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> GetFood(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Invalid food ID"
            });
        }
        // Implement actual logic here when service method is available
        var food = await foodService.GetFoodByIdAsync(id);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = food,
            Message = "Food found"
        });
    }
}