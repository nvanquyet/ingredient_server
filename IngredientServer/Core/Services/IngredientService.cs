using IngredientServer.Core.Entities;
using IngredientServer.Core.Helpers;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs.Entity;
using IngredientServer.Utils.Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IngredientServer.Core.Services;

public class IngredientService(IIngredientRepository ingredientRepository, 
    IUserContextService userContextService,
    IImageService imageService,
    ILogger<IngredientService> logger)
    : IIngredientService
{
    public async Task<IngredientDataResponseDto> CreateIngredientAsync(CreateIngredientRequestDto dto)
    {
        //Save Image 
        string? imageUrl = "";
        
        // Image processing with logging
        if (dto.Image is { Length: > 0 })
        {
            logger.LogInformation("Processing image upload - Size: {ImageSize} bytes, ContentType: {ContentType}", 
                dto.Image.Length, dto.Image.ContentType);
            
            try
            {
                imageUrl = await imageService.SaveImageAsync(dto.Image);
                logger.LogInformation("Image saved successfully: {ImageUrl}", imageUrl);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save image for food {FoodName}", dto.Name);
                throw;
            }
        }
        else
        {
            logger.LogInformation("No image provided for food creation");
        }
       
        var ingredient = new Ingredient
        {
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category,
            Quantity = dto.Quantity,
            ExpiryDate = DateTimeHelper.NormalizeToUtc(dto.ExpiryDate),
            Unit = dto.Unit,
            UserId = userContextService.GetAuthenticatedUserId(),
            ImageUrl = imageUrl
        };

        var savedIngredient = await ingredientRepository.AddAsync(ingredient);
        
        if (savedIngredient == null)
        {
            throw new UnauthorizedAccessException("Failed to create ingredient or access denied.");
        }
        
        return savedIngredient.ToDto();
    }

    public async Task<IngredientDataResponseDto> UpdateIngredientAsync(UpdateIngredientRequestDto dto)
    {
        if (dto.Id <= 0)
        {
            throw new ArgumentException("Invalid ingredient ID", nameof(dto.Id));
        }
        
        var ingredient = await ingredientRepository.GetByIdAsync(dto.Id);
        
        if (ingredient == null)
        {
            throw new UnauthorizedAccessException("Ingredient not found or access denied.");
        }

        ingredient.Name = dto.Name;
        ingredient.Description = dto.Description;
        ingredient.Unit = dto.Unit;
        ingredient.Category = dto.Category;
        ingredient.Quantity = dto.Quantity;
        ingredient.ExpiryDate = DateTimeHelper.NormalizeToUtc(dto.ExpiryDate);
        
        if (dto.Image is { Length: > 0 })
        {
            if (string.IsNullOrEmpty(ingredient.ImageUrl))
            {
                ingredient.ImageUrl = await imageService.SaveImageAsync(dto.Image);
            }
            else
            {
                ingredient.ImageUrl = await imageService.UpdateImageAsync(dto.Image, ingredient.ImageUrl);
            }
        }

        var updatedIngredient = await ingredientRepository.UpdateAsync(ingredient);
        return updatedIngredient.ToDto();
    }

    public async Task<bool> DeleteIngredientAsync(int ingredientId)
    {
        if (ingredientId <= 0)
        {
            throw new ArgumentException("Invalid ingredient ID", nameof(ingredientId));
        }

        var ingredient = await ingredientRepository.GetByIdAsync(ingredientId);

        if (ingredient == null)
        {
            throw new UnauthorizedAccessException("Ingredient not found or access denied.");
        }

        if (string.IsNullOrEmpty(ingredient.ImageUrl)) return await ingredientRepository.DeleteAsync(ingredientId);
        await imageService.DeleteImageAsync(ingredient.ImageUrl);
        return await ingredientRepository.DeleteAsync(ingredientId);
    }


    public async Task<IngredientSearchResultDto> GetAllAsync(IngredientFilterDto filter)
    {
        var ingredients = await ingredientRepository.GetByFilterAsync(filter);
        return ingredients;
    }

    public async Task<IngredientDataResponseDto> GetIngredientByIdAsync(int id)
    {
        var ingredient = await ingredientRepository.GetByIdAsync(id);
        if (ingredient == null)
        {
            throw new UnauthorizedAccessException("Ingredient not found or access denied.");
        }
        return ingredient.ToDto();
    }
}