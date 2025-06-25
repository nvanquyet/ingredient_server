using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs.Ingredient;

namespace IngredientServer.Core.Services;

public class IngredientService(IIngredientRepository ingredientRepository, IUserContextService userContextService)
    : IIngredientService
{
    public async Task<IngredientSearchResultDto> GetAllAsync(IngredientFilterDto filter)
    {
        // Validate filter
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
        }

        if (filter.PageNumber < 1)
        {
            throw new ArgumentException("Page number must be greater than or equal to 1.", nameof(filter.PageNumber));
        }

        if (filter.PageSize < 1)
        {
            throw new ArgumentException("Page size must be greater than or equal to 1.", nameof(filter.PageSize));
        }

        // Get filtered and paginated ingredients
        var ingredients = await ingredientRepository.GetAllAsync(filter.PageNumber, filter.PageSize, filter);

        // Get total count for pagination
        var totalCount = (await ingredientRepository.GetAllAsync(1, int.MaxValue, filter)).Count();

        // Map to DTO
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
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize),
            HasNextPage = filter.PageNumber < (int)Math.Ceiling((double)totalCount / filter.PageSize),
            HasPreviousPage = filter.PageNumber > 1
        };

        return result;
    }

    public async Task<Ingredient> CreateIngredientAsync(CreateIngredientDto dto)
    {
        var ingredient = new Ingredient
        {
            Name = dto.Name,
            Description = dto.Description,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            Category = dto.Category,
            ExpiryDate = dto.ExpiryDate,
            ImageUrl = dto.ImageUrl,
            UserId = userContextService.GetAuthenticatedUserId(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await ingredientRepository.AddAsync(ingredient);
    }

    public async Task<Ingredient> UpdateIngredientAsync(int ingredientId, UpdateIngredientDto dto)
    {
        var ingredient = await ingredientRepository.GetByIdAsync(ingredientId);
        if (ingredient == null)
        {
            throw new KeyNotFoundException("Ingredient not found or access denied.");
        }

        if (!string.IsNullOrEmpty(dto.Name)) ingredient.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Description)) ingredient.Description = dto.Description;
        if (dto.Quantity.HasValue) ingredient.Quantity = dto.Quantity.Value;
        if (dto.Unit.HasValue) ingredient.Unit = dto.Unit.Value;
        if (dto.Category.HasValue) ingredient.Category = dto.Category.Value;
        if (dto.ExpiryDate.HasValue) ingredient.ExpiryDate = dto.ExpiryDate.Value;
        if (!string.IsNullOrEmpty(dto.ImageUrl)) ingredient.ImageUrl = dto.ImageUrl;
        ingredient.UpdatedAt = DateTime.UtcNow;

        return await ingredientRepository.UpdateAsync(ingredient);
    }

    public async Task<bool> DeleteIngredientAsync(int ingredientId)
    {
        return await ingredientRepository.DeleteAsync(ingredientId);
    }
}