using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs.Entity;

namespace IngredientServer.Core.Services;

public class IngredientService(IIngredientRepository ingredientRepository, IUserContextService userContextService)
    : IIngredientService
{
    public async Task<IngredientDto> CreateIngredientAsync(IngredientDataDto dataDto)
    {
        var ingredient = new Ingredient
        {
            Name = dataDto.Name,
            Description = dataDto.Description,
            Category = dataDto.Category,
            Quantity = dataDto.Quantity,
            ExpiryDate = dataDto.ExpiryDate,
            Unit = dataDto.Unit,
            UserId = userContextService.GetAuthenticatedUserId()
        };

        var savedIngredient = await ingredientRepository.AddAsync(ingredient);
        return new IngredientDto
        {
            Id = savedIngredient.Id,
            Name = savedIngredient.Name,
            Description = savedIngredient.Description,
            Unit = savedIngredient.Unit,
            Category = savedIngredient.Category,
            Quantity = savedIngredient.Quantity,
            ExpiryDate = savedIngredient.ExpiryDate,
        };
    }

    public async Task<IngredientDto> UpdateIngredientAsync(int ingredientId, IngredientDataDto dto)
    {
        var ingredient = await ingredientRepository.GetByIdAsync(ingredientId);
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

        var updatedIngredient = await ingredientRepository.UpdateAsync(ingredient);
        return new IngredientDto
        {
            Id = updatedIngredient.Id,
            Name = updatedIngredient.Name,
            Description = updatedIngredient.Description,
            Unit = updatedIngredient.Unit,
            Category = updatedIngredient.Category,
            Quantity = updatedIngredient.Quantity,
            ExpiryDate = updatedIngredient.ExpiryDate,
        };
    }

    public async Task<bool> DeleteIngredientAsync(int ingredientId)
    {
        return await ingredientRepository.DeleteAsync(ingredientId);
    }

    public async Task<IngredientSearchResultDto> GetAllAsync(IngredientFilterDto filter)
    {
        var ingredients = await ingredientRepository.GetByFilterAsync(filter);
        var enumerable = ingredients as Ingredient[] ?? ingredients.ToArray();
        return new IngredientSearchResultDto
        {
            Ingredients = enumerable.Select(i => new IngredientDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                ImageUrl = i.ImageUrl,
                Unit = i.Unit,
                Category = i.Category,
                Quantity = i.Quantity,
                ExpiryDate = i.ExpiryDate,
                DaysUntilExpiry = (i.ExpiryDate - DateTime.UtcNow).Days,
                IsExpired = i.ExpiryDate < DateTime.UtcNow,
                IsExpiringSoon = (i.ExpiryDate - DateTime.UtcNow).Days <= 7,
            }).ToList(),
            TotalCount = enumerable.Count()
        };
    }
}