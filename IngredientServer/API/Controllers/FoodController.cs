using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Services;
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
    public async Task<ActionResult<Food>> CreateFood([FromBody] FoodDataDto dataDto)
    {
        try
        {
            var food = await foodService.CreateFoodAsync(dataDto);
            return CreatedAtAction(nameof(GetFood), new { id = food.Id }, food);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Food>> UpdateFood(int id, [FromBody] FoodDataDto dto)
    {
        try
        {
            var food = await foodService.UpdateFoodAsync(id, dto);
            return Ok(food);
        }
        catch (UnauthorizedAccessException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFood(int id)
    {
        try
        {
            var result = await foodService.DeleteFoodAsync(id);
            if (result)
                return NoContent();
            return NotFound(new { message = "Food not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost("suggestions")]
    public async Task<ActionResult<List<FoodSuggestionDto>>> GetSuggestions([FromBody] FoodSuggestionRequestDto requestDto)
    {
        try
        {
            var suggestions = await foodService.GetSuggestionsAsync(requestDto);
            return Ok(suggestions);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new { message = "Service unavailable", details = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost("recipes")]
    public async Task<ActionResult<FoodRecipeDto>> GetRecipeSuggestions([FromBody] FoodRecipeRequestDto recipeRequest)
    {
        try
        {
            var recipe = await foodService.GetRecipeSuggestionsAsync(recipeRequest);
            return Ok(recipe);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new { message = "Service unavailable", details = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    // Helper method for CreatedAtAction
    [HttpGet("{id}")]
    public async Task<ActionResult> GetFood(int id)
    {
        // This is a placeholder - you might want to implement this in your service
        return Ok(new { id, message = "Food retrieved successfully" });
    }
}