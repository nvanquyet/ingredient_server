﻿using System.Text.Json;
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
    public async Task<ActionResult<ApiResponse<FoodDataResponseDto>>> CreateFood(
        [FromForm] CreateFoodRequestDto dataDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<FoodDataResponseDto>
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
                new ApiResponse<FoodDataResponseDto>
                {
                    Success = true,
                    Data = food,
                    Message = "Food created successfully"
                });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<FoodDataResponseDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<FoodDataResponseDto>
            {
                Success = false,
                Message = "Internal server error",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["details"] = [ex.Message]
                }
            });
        }
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<FoodDataResponseDto>>> UpdateFood([FromForm] UpdateFoodRequestDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<FoodDataResponseDto>
                {
                    Success = false,
                    Message = "Invalid model state",
                    Metadata = new Dictionary<string, List<string>?>
                    {
                        ["errors"] = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    }
                });
            }

            var food = await foodService.UpdateFoodAsync(dto);
            return Ok(new ApiResponse<FoodDataResponseDto>
            {
                Success = true,
                Data = food,
                Message = "Food updated successfully"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return NotFound(new ApiResponse<FoodDataResponseDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<FoodDataResponseDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<FoodDataResponseDto>
            {
                Success = false,
                Message = "Internal server error",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["details"] = [ex.Message]
                }
            });
        }
    }

    [HttpDelete]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteFood([FromBody] DeleteFoodRequestDto dto)
    {
        try
        {
            if (dto.Id <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Invalid food ID"
                });
            }

            var result = await foodService.DeleteFoodAsync(dto.Id);
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
                    ["details"] = [ex.Message]
                }
            });
        }
    }

    [HttpPost("suggestions")]
    public async Task<ActionResult<ApiResponse<List<FoodSuggestionResponseDto>>>> GetSuggestions(
        [FromBody] FoodSuggestionRequestDto requestDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<List<FoodSuggestionResponseDto>>
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
            return Ok(new ApiResponse<List<FoodSuggestionResponseDto>>
            {
                Success = true,
                Data = suggestions,
                Message = "Food suggestions retrieved successfully",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["count"] = [suggestions.Count.ToString()]
                }
            });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new ApiResponse<List<FoodSuggestionResponseDto>>
            {
                Success = false,
                Message = "Service unavailable",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["details"] = [ex.Message]
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<List<FoodSuggestionResponseDto>>
            {
                Success = false,
                Message = "Internal server error",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["details"] = [ex.Message]
                }
            });
        }
    }

    [HttpPost("recipes")]
    public async Task<ActionResult<ApiResponse<FoodDataResponseDto>>> GetRecipeSuggestions(
        [FromBody] FoodRecipeRequestDto recipeRequest)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<FoodDataResponseDto>
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
            return Ok(new ApiResponse<FoodDataResponseDto>
            {
                Success = true,
                Data = recipe,
                Message = "Recipe retrieved successfully"
            });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new ApiResponse<FoodDataResponseDto>
            {
                Success = false,
                Message = "Service unavailable",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["details"] = [ex.Message]
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<FoodDataResponseDto>
            {
                Success = false,
                Message = "Internal server error",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["details"] = [ex.Message]
                }
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<FoodDataResponseDto>>> GetFood(int id)
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
        food.NormalizeConsumedAt();
        if (food.Instructions.Count > 0)
        {
            var processedInstructions = new List<string>();
            foreach (var instruction in food.Instructions)
            {
                try
                {
                    // Nếu instruction là một chuỗi JSON, parse nó
                    if (instruction.StartsWith("[") && instruction.EndsWith("]"))
                    {
                        var parsedInstructions = JsonSerializer.Deserialize<List<string>>(instruction);
                        if (parsedInstructions != null)
                        {
                            processedInstructions.AddRange(parsedInstructions);
                        }
                    }
                    else
                    {
                        // Nếu không phải JSON, giữ nguyên
                        processedInstructions.Add(instruction);
                    }
                }
                catch (JsonException)
                {
                    // Nếu parse lỗi, giữ nguyên chuỗi gốc
                    processedInstructions.Add(instruction);
                }
            }

            food.Instructions = processedInstructions;
        }

        // Xử lý Tips - tương tự như Instructions
        if (food.Tips.Count > 0)
        {
            var processedTips = new List<string>();
            foreach (var tip in food.Tips)
            {
                try
                {
                    // Nếu tip là một chuỗi JSON, parse nó
                    if (tip.StartsWith("[") && tip.EndsWith("]"))
                    {
                        var parsedTips = JsonSerializer.Deserialize<List<string>>(tip);
                        if (parsedTips != null)
                        {
                            processedTips.AddRange(parsedTips);
                        }
                    }
                    else
                    {
                        // Nếu không phải JSON, giữ nguyên
                        processedTips.Add(tip);
                    }
                }
                catch (JsonException)
                {
                    // Nếu parse lỗi, giữ nguyên chuỗi gốc
                    processedTips.Add(tip);
                }
            }

            food.Tips = processedTips;
        }

        return Ok(new ApiResponse<FoodDataResponseDto>
        {
            Success = true,
            Data = food,
            Message = "Food found"
        });
    }
}