using System.Text.Json;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngredientServer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FoodController(IFoodService foodService, ILogger<FoodController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<FoodDataResponseDto>>> CreateFood(
        [FromForm] CreateFoodRequestDto dataDto)
    {
        try
        {
            // FIX: Log all form keys for debugging
            logger.LogInformation("Form keys: {Keys}", string.Join(", ", Request.Form.Keys));
            
            // FIX: Parse Ingredients from JSON string if needed (multipart/form-data sends complex objects as JSON strings)
            if (Request.Form.ContainsKey("Ingredients"))
            {
                var ingredientsJson = Request.Form["Ingredients"].ToString();
                logger.LogInformation("Found Ingredients in form: {Json}", ingredientsJson);
                
                if (!string.IsNullOrEmpty(ingredientsJson))
                {
                    try
                    {
                        // FIX: Try to parse as JSON array
                        var parsedIngredients = JsonSerializer.Deserialize<List<FoodIngredientDto>>(ingredientsJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (parsedIngredients != null && parsedIngredients.Any())
                        {
                            dataDto.Ingredients = parsedIngredients;
                            logger.LogInformation("✅ Parsed {Count} ingredients from JSON", parsedIngredients.Count);
                        }
                        else
                        {
                            logger.LogWarning("Parsed ingredients is null or empty");
                        }
                    }
                    catch (JsonException ex)
                    {
                        logger.LogError(ex, "Failed to parse Ingredients JSON: {Json}", ingredientsJson);
                    }
                }
                else
                {
                    logger.LogWarning("Ingredients field is empty");
                }
            }
            else
            {
                logger.LogWarning("Ingredients field not found in form data");
            }
            
            // FIX: Also check if Ingredients is already populated (from model binding)
            if (dataDto.Ingredients != null && dataDto.Ingredients.Any())
            {
                logger.LogInformation("Ingredients already populated from model binding: {Count}", dataDto.Ingredients.Count());
            }
            else
            {
                logger.LogWarning("⚠️ No ingredients found in dataDto after parsing");
            }

            // FIX: Parse Instructions and Tips from JSON string if needed
            if (Request.Form.ContainsKey("Instructions"))
            {
                var instructionsJson = Request.Form["Instructions"].ToString();
                if (!string.IsNullOrEmpty(instructionsJson) && instructionsJson.StartsWith("["))
                {
                    try
                    {
                        var parsedInstructions = JsonSerializer.Deserialize<List<string>>(instructionsJson);
                        if (parsedInstructions != null)
                        {
                            dataDto.Instructions = parsedInstructions;
                        }
                    }
                    catch (JsonException)
                    {
                        // Keep original if parse fails
                    }
                }
            }

            if (Request.Form.ContainsKey("Tips"))
            {
                var tipsJson = Request.Form["Tips"].ToString();
                if (!string.IsNullOrEmpty(tipsJson) && tipsJson.StartsWith("["))
                {
                    try
                    {
                        var parsedTips = JsonSerializer.Deserialize<List<string>>(tipsJson);
                        if (parsedTips != null)
                        {
                            dataDto.Tips = parsedTips;
                        }
                    }
                    catch (JsonException)
                    {
                        // Keep original if parse fails
                    }
                }
            }

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
            // FIX: Parse Ingredients from JSON string if needed
            if (Request.Form.ContainsKey("Ingredients"))
            {
                var ingredientsJson = Request.Form["Ingredients"].ToString();
                if (!string.IsNullOrEmpty(ingredientsJson))
                {
                    try
                    {
                        var parsedIngredients = JsonSerializer.Deserialize<List<FoodIngredientDto>>(ingredientsJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (parsedIngredients != null)
                        {
                            dto.Ingredients = parsedIngredients;
                        }
                    }
                    catch (JsonException)
                    {
                        // Keep original if parse fails
                    }
                }
            }

            // FIX: Parse Instructions and Tips from JSON string if needed
            if (Request.Form.ContainsKey("Instructions"))
            {
                var instructionsJson = Request.Form["Instructions"].ToString();
                if (!string.IsNullOrEmpty(instructionsJson) && instructionsJson.StartsWith("["))
                {
                    try
                    {
                        var parsedInstructions = JsonSerializer.Deserialize<List<string>>(instructionsJson);
                        if (parsedInstructions != null)
                        {
                            dto.Instructions = parsedInstructions;
                        }
                    }
                    catch (JsonException)
                    {
                        // Keep original if parse fails
                    }
                }
            }

            if (Request.Form.ContainsKey("Tips"))
            {
                var tipsJson = Request.Form["Tips"].ToString();
                if (!string.IsNullOrEmpty(tipsJson) && tipsJson.StartsWith("["))
                {
                    try
                    {
                        var parsedTips = JsonSerializer.Deserialize<List<string>>(tipsJson);
                        if (parsedTips != null)
                        {
                            dto.Tips = parsedTips;
                        }
                    }
                    catch (JsonException)
                    {
                        // Keep original if parse fails
                    }
                }
            }

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