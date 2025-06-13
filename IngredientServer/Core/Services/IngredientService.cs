using IngredientServer.Core.Entities;
using IngredientServer.Core.Exceptions;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs.Ingredient;

namespace IngredientServer.Core.Services;

public class IngredientService(IIngredientRepository ingredientRepository) : IIngredientService
{
    public async Task<Ingredient?> GetByIdAsync(int id, int userId)
    {
        if (id <= 0 || userId <= 0)
            throw new ArgumentException("Invalid id or userId");

        return await ingredientRepository.GetByIdAsync(id, userId);
    }

    public async Task<IEnumerable<Ingredient>> GetAllAsync(int userId, int pageNumber = 1, int pageSize = 10)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid userId");
        if (pageNumber < 1)
            throw new ArgumentException("Invalid pageNumber");
        if (pageSize <= 0)
            throw new ArgumentException("Invalid pageSize");

        return await ingredientRepository.GetAllAsync(userId, pageNumber, pageSize);
    }

    public async Task<Ingredient> AddAsync(Ingredient entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
        if (entity.UserId <= 0)
            throw new ArgumentException("Invalid UserId");
        if (string.IsNullOrWhiteSpace(entity.Name))
            throw new ArgumentException("Name is required");

        return await ingredientRepository.AddAsync(entity);
    }

    public async Task<Ingredient> UpdateAsync(Ingredient entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
        if (entity.Id <= 0 || entity.UserId <= 0)
            throw new ArgumentException("Invalid Id or UserId");

        var existing = await ingredientRepository.GetByIdAsync(entity.Id, entity.UserId);
        if (existing == null) throw new NotFoundException($"Ingredient with ID {entity.Id} not found for user {entity.UserId}");

        return await ingredientRepository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        if (id <= 0 || userId <= 0)
            throw new ArgumentException("Invalid id or userId");

        return await ingredientRepository.DeleteAsync(id, userId);
    }

    public async Task<bool> ExistsAsync(int id, int userId)
    {
        if (id <= 0 || userId <= 0)
            throw new ArgumentException("Invalid id or userId");

        return await ingredientRepository.ExistsAsync(id, userId);
    }

    public async Task<Ingredient?> GetByIdAndUserIdAsync(int id, int userId)
    {
        return await ingredientRepository.GetByIdAndUserIdAsync(id, userId);
    }

    public async Task<IEnumerable<Ingredient>> GetByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 10)
    {
        return await ingredientRepository.GetByUserIdAsync(userId, pageNumber, pageSize);
    }

    public async Task<IEnumerable<Ingredient>> GetExpiringItemsAsync(int userId, int days = 7)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid userId");
        if (days < 0)
            throw new ArgumentException("Invalid days");

        return await ingredientRepository.GetExpiringItemsAsync(userId, days);
    }

    public async Task<IEnumerable<Ingredient>> GetExpiredItemsAsync(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid userId");

        return await ingredientRepository.GetExpiredItemsAsync(userId);
    }

    public async Task<IEnumerable<Ingredient>> GetFilteredAsync(IngredientFilterDto filter, int pageNumber = 1, int pageSize = 10)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));
        if (filter.UserId <= 0)
            throw new ArgumentException("Invalid UserId");

        return await ingredientRepository.GetFilteredAsync(filter, pageNumber, pageSize);
    }

    public async Task<IEnumerable<Ingredient>> GetSortedAsync(int userId, IngredientSortDto sort, int pageNumber = 1, int pageSize = 10)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid userId");
        if (sort == null)
            throw new ArgumentNullException(nameof(sort));

        return await ingredientRepository.GetSortedAsync(userId, sort, pageNumber, pageSize);
    }

    public async Task<IEnumerable<Ingredient>> GetByCategoryAsync(int userId, IngredientCategory category, int pageNumber = 1, int pageSize = 10)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid userId");

        return await ingredientRepository.GetByCategoryAsync(userId, category, pageNumber, pageSize);
    }

    public async Task<int> CountByUserIdAsync(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid userId");

        return await ingredientRepository.CountByUserIdAsync(userId);
    }
}