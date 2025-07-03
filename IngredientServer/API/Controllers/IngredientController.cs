using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngredientServer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IngredientController(IIngredientService ingredientService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<IngredientDataResponseDto>>> CreateIngredient([FromBody] CreateIngredientRequestDto dataDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<IngredientDataResponseDto>
                {
                    Success = false,
                    Message = "Invalid data",
                    Metadata = new Dictionary<string, List<string>?>
                    {
                        ["errors"] = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    }
                });
            }

            dataDto.NormalizeExpiryDate();
            var ingredient = await ingredientService.CreateIngredientAsync(dataDto);
            return CreatedAtAction(
                nameof(GetIngredient), 
                new { id = ingredient.Id }, 
                new ApiResponse<IngredientDataResponseDto>
                {
                    Success = true,
                    Data = ingredient,
                    Message = "Ingredient created successfully"
                });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IngredientDataResponseDto>
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

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<IngredientDataResponseDto>>> UpdateIngredient([FromBody] UpdateIngredientRequestDto dto)
    {
        try
        {
            if(ModelState.IsValid == false)
            {
                return BadRequest(new ApiResponse<IngredientDataResponseDto>
                {
                    Success = false,
                    Message = "Invalid data",
                    Metadata = new Dictionary<string, List<string>?>
                    {
                        ["errors"] = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    }
                });
            }

            dto.NormalizeExpiryDate();
            var ingredient = await ingredientService.UpdateIngredientAsync(dto);
            return Ok(new ApiResponse<IngredientDataResponseDto>
            {
                Success = true,
                Data = ingredient,
                Message = "Ingredient updated successfully"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return NotFound(new ApiResponse<IngredientDataResponseDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IngredientDataResponseDto>
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

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteIngredient([FromBody] DeleteIngredientRequestDto dto)
    {
        try
        {
            if (dto.Id <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Invalid ingredient ID"
                });
            }
            var result = await ingredientService.DeleteIngredientAsync(dto.Id);
            if (result)
            {
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Ingredient deleted successfully"
                });
            }
            
            return NotFound(new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                Message = "Ingredient not found"
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

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IngredientSearchResultDto>>> GetAllIngredients([FromQuery] IngredientFilterDto filter)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<IngredientSearchResultDto>
                {
                    Success = false,
                    Message = "Invalid filter parameters",
                    Metadata = new Dictionary<string, List<string>?>
                    {
                        ["errors"] = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    }
                });
            }
            var result = await ingredientService.GetAllAsync(filter);
            return Ok(new ApiResponse<IngredientSearchResultDto>
            {
                Success = true,
                Data = result,
                Message = "Ingredients retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IngredientSearchResultDto>
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
    public async Task<ActionResult<ApiResponse<IngredientDataResponseDto>>> GetIngredient(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new ApiResponse<IngredientDataResponseDto>
            {
                Success = false,
                Message = "Invalid ingredient ID"
            });
        }
        // Implement actual logic here when service method is available
        var ingredient = await ingredientService.GetIngredientByIdAsync(id);
        return Ok(new ApiResponse<IngredientDataResponseDto>
        {
            Success = true,
            Data = ingredient,
            Message = "Ingredient found"
        });
    }
}