using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Ingredient;
using Microsoft.AspNetCore.Mvc;

namespace IngredientServer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IngredientsController(IIngredientRepository ingredientRepository, IIngredientService ingredientService)
    : ControllerBase
{
    // Giả định service để xử lý logic nghiệp vụ

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IngredientSearchResultDto>>> GetIngredients(
        [FromQuery] IngredientFilterDto filter)
    {
        var ingredients = await ingredientRepository.GetAllAsync(filter.PageNumber, filter.PageSize, filter);
        var totalCount = (await ingredientRepository.GetAllAsync(1, int.MaxValue, filter)).Count();

        var result = new IngredientSearchResultDto
        {
            Ingredients = ingredients.Select(i => new IngredientDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Category = i.Category,
                ExpiryDate = i.ExpiryDate,
                ImageUrl = i.ImageUrl,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                DaysUntilExpiry = i.DaysUntilExpiry,
                IsExpired = i.IsExpired,
                IsExpiringSoon = i.IsExpiringSoon,
                Status = i.IsExpired ? "Expired" : i.IsExpiringSoon ? "Expiring Soon" : i.Quantity <= 10 ? "Low Stock" : "Available",
                UnitDisplay = i.Unit.ToString(),
                CategoryDisplay = i.Category.ToString()
            }),
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize),
            HasNextPage = filter.PageNumber < (int)Math.Ceiling((double)totalCount / filter.PageSize),
            HasPreviousPage = filter.PageNumber > 1
        };

        return Ok(new ApiResponse<IngredientSearchResultDto>
        {
            Success = true,
            Data = result,
            Message = "Ingredients retrieved successfully."
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<IngredientDto>>> CreateIngredient([FromBody] CreateIngredientDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<IngredientDto>
            {
                Success = false,
                Message = "Invalid input data.",
                Metadata = ModelState.ToDictionary(
                    m => m.Key,
                    m => m.Value?.Errors.Select(e => e.ErrorMessage).ToList())
            });
        }

        var ingredient = await ingredientService.CreateIngredientAsync(dto);
        return Ok(new ApiResponse<IngredientDto>
        {
            Success = true,
            Data = MapToIngredientDto(ingredient),
            Message = "Ingredient created successfully."
        });
    }

    [HttpPut("{ingredientId}")]
    public async Task<ActionResult<ApiResponse<IngredientDto>>> UpdateIngredient(int ingredientId, [FromBody] UpdateIngredientDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<IngredientDto>
            {
                Success = false,
                Message = "Invalid input data.",
                Metadata = ModelState.ToDictionary(
                    m => m.Key,
                    m => m.Value?.Errors.Select(e => e.ErrorMessage).ToList())
            });
        }

        try
        {
            var ingredient = await ingredientService.UpdateIngredientAsync(ingredientId, dto);
            return Ok(new ApiResponse<IngredientDto>
            {
                Success = true,
                Data = MapToIngredientDto(ingredient),
                Message = "Ingredient updated successfully."
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<IngredientDto> { Success = false, Message = "Access denied." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse<IngredientDto> { Success = false, Message = "Ingredient not found." });
        }
    }

    [HttpDelete("{ingredientId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteIngredient(int ingredientId)
    {
        try
        {
            var success = await ingredientService.DeleteIngredientAsync(ingredientId);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = success,
                Message = "Ingredient deleted successfully."
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<bool> { Success = false, Message = "Access denied." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse<bool> { Success = false, Message = "Ingredient not found." });
        }
    }

    private IngredientDto MapToIngredientDto(Ingredient ingredient)
    {
        return new IngredientDto
        {
            Id = ingredient.Id,
            Name = ingredient.Name,
            Description = ingredient.Description,
            Quantity = ingredient.Quantity,
            Unit = ingredient.Unit,
            Category = ingredient.Category,
            ExpiryDate = ingredient.ExpiryDate,
            ImageUrl = ingredient.ImageUrl,
            CreatedAt = ingredient.CreatedAt,
            UpdatedAt = ingredient.UpdatedAt,
            DaysUntilExpiry = ingredient.DaysUntilExpiry,
            IsExpired = ingredient.IsExpired,
            IsExpiringSoon = ingredient.IsExpiringSoon,
            Status = ingredient.IsExpired ? "Expired" : ingredient.IsExpiringSoon ? "Expiring Soon" : ingredient.Quantity <= 10 ? "Low Stock" : "Available",
            UnitDisplay = ingredient.Unit.ToString(),
            CategoryDisplay = ingredient.Category.ToString()
        };
    }
}