﻿using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs.Entity;
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
            ExpiryDate = dto.ExpiryDate,
            Unit = dto.Unit,
            UserId = userContextService.GetAuthenticatedUserId(),
            ImageUrl = imageUrl
        };

        var savedIngredient = await ingredientRepository.AddAsync(ingredient);
        
        if (savedIngredient == null)
        {
            throw new UnauthorizedAccessException("Failed to create ingredient or access denied.");
        }
        // Map the saved ingredient to DTO
        var response = new IngredientDataResponseDto
        {
            Id = savedIngredient.Id,
            Name = savedIngredient.Name,
            Description = savedIngredient.Description,
            Unit = savedIngredient.Unit,
            Category = savedIngredient.Category,
            Quantity = savedIngredient.Quantity,
            ExpiryDate = savedIngredient.ExpiryDate,
            ImageUrl = savedIngredient.ImageUrl
        };
        response.NormalizeExpiryDate();
        
        return response;
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
        ingredient.ExpiryDate = dto.ExpiryDate;
        
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
        var response = new IngredientDataResponseDto
        {
            Id = updatedIngredient.Id,
            Name = updatedIngredient.Name,
            Description = updatedIngredient.Description,
            Unit = updatedIngredient.Unit,
            Category = updatedIngredient.Category,
            Quantity = updatedIngredient.Quantity,
            ExpiryDate = updatedIngredient.ExpiryDate,
            ImageUrl = updatedIngredient.ImageUrl
        };
        response.NormalizeExpiryDate();
        
        return response;
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
        return new IngredientDataResponseDto
        {
            Id = ingredient.Id,
            Name = ingredient.Name,
            Description = ingredient.Description,
            Unit = ingredient.Unit,
            Category = ingredient.Category,
            Quantity = ingredient.Quantity,
            ExpiryDate = ingredient.ExpiryDate,
            ImageUrl = ingredient.ImageUrl
        };
    }
}