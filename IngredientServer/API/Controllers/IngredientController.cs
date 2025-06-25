using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngredientServer.API.Controllers;
// IngredientController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IngredientController(IIngredientService ingredientService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<IngredientDto>> CreateIngredient([FromBody] IngredientDataDto dataDto)
    {
        try
        {
            var ingredient = await ingredientService.CreateIngredientAsync(dataDto);
            return CreatedAtAction(nameof(GetIngredient), new { id = ingredient.Id }, ingredient);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<IngredientDto>> UpdateIngredient(int id, [FromBody] IngredientDataDto dto)
    {
        try
        {
            var ingredient = await ingredientService.UpdateIngredientAsync(id, dto);
            return Ok(ingredient);
        }
        catch (UnauthorizedAccessException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteIngredient(int id)
    {
        try
        {
            var result = await ingredientService.DeleteIngredientAsync(id);
            if (result)
                return NoContent();
            return NotFound(new { message = "Ingredient not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IngredientSearchResultDto>> GetAllIngredients([FromQuery] IngredientFilterDto filter)
    {
        try
        {
            var result = await ingredientService.GetAllAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    // Helper method for CreatedAtAction
    [HttpGet("{id}")]
    public ActionResult GetIngredient(int id)
    {
        // This is a placeholder - you might want to implement this in your service
        return Ok(new { id, message = "Ingredient retrieved successfully" });
    }
}