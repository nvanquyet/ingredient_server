using IngredientServer.Core.Entities;
using IngredientServer.Core.Helpers;
using IngredientServer.Utils.DTOs.Entity;

namespace IngredientServer.Utils.Mappers;

/// <summary>
/// Extension methods for mapping Ingredient Entity to DTOs
/// </summary>
public static class IngredientMapper
{
    /// <summary>
    /// Maps Ingredient entity to IngredientDataResponseDto
    /// </summary>
    public static IngredientDataResponseDto ToDto(this Ingredient ingredient)
    {
        if (ingredient == null)
            throw new ArgumentNullException(nameof(ingredient));

        var dto = new IngredientDataResponseDto
        {
            Id = ingredient.Id,
            Name = ingredient.Name,
            Description = ingredient.Description,
            Unit = ingredient.Unit,
            Category = ingredient.Category,
            Quantity = ingredient.Quantity,
            ExpiryDate = DateTimeHelper.NormalizeToUtc(ingredient.ExpiryDate),
            ImageUrl = ingredient.ImageUrl
        };
        
        dto.NormalizeExpiryDate();
        return dto;
    }

    /// <summary>
    /// Maps collection of Ingredient entities to DTOs
    /// </summary>
    public static IEnumerable<IngredientDataResponseDto> ToDto(this IEnumerable<Ingredient> ingredients)
    {
        return ingredients.Select(i => i.ToDto());
    }
}

