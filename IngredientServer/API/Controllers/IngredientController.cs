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
    public async Task<ActionResult<ApiResponse<IngredientDto>>> CreateIngredient([FromBody] IngredientDataDto dataDto)
    {
        try
        {
            var ingredient = await ingredientService.CreateIngredientAsync(dataDto);
            return CreatedAtAction(
                nameof(GetIngredient), 
                new { id = ingredient.Id }, 
                new ApiResponse<IngredientDto>
                {
                    Success = true,
                    Data = ingredient,
                    Message = "Ingredient created successfully"
                });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IngredientDto>
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
    public async Task<ActionResult<ApiResponse<IngredientDto>>> UpdateIngredient(int id, [FromBody] IngredientDataDto dto)
    {
        try
        {
            var ingredient = await ingredientService.UpdateIngredientAsync(id, dto);
            return Ok(new ApiResponse<IngredientDto>
            {
                Success = true,
                Data = ingredient,
                Message = "Ingredient updated successfully"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return NotFound(new ApiResponse<IngredientDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IngredientDto>
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
    public async Task<ActionResult<ApiResponse<bool>>> DeleteIngredient(int id)
    {
        try
        {
            var result = await ingredientService.DeleteIngredientAsync(id);
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
                    ["details"] = new List<string> { ex.Message }
                }
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IngredientSearchResultDto>>> GetAllIngredients([FromQuery] IngredientFilterDto filter)
    {
        try
        {
            var result = await ingredientService.GetAllAsync(filter);
            return Ok(new ApiResponse<IngredientSearchResultDto>
            {
                Success = true,
                Data = result,
                Message = "Ingredients retrieved successfully",
                Metadata = new Dictionary<string, List<string>?>
                {
                    ["totalCount"] = new List<string> { result.TotalCount.ToString() },
                    ["pageNumber"] = new List<string> { result.PageNumber.ToString() },
                    ["totalPages"] = new List<string> { result.TotalPages.ToString() }
                }
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
                    ["details"] = new List<string> { ex.Message }
                }
            });
        }
    }

    [HttpGet("{id}")]
    public ActionResult<ApiResponse<object>> GetIngredient(int id)
    {
        // Implement actual logic here when service method is available
        var ingredient = ingredientService.GetIngredientByIdAsync(id);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = ingredient,
            Message = "Ingredient found"
        });
    }
}