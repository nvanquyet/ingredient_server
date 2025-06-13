using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IngredientServer.Core.Exceptions;

namespace IngredientServer.Core.Services;

public class FoodService : IFoodService
{
    private readonly IFoodRepository _foodRepository;

    public FoodService(IFoodRepository foodRepository)
    {
        _foodRepository = foodRepository;
    }

    public async Task<Food?> GetByIdAsync(int id, int userId)
    {
        if (id <= 0 || userId <= 0)
            throw new ArgumentException("Invalid id or userId");

        return await _foodRepository.GetByIdAsync(id, userId);
    }

    public async Task<IEnumerable<Food>> GetAllAsync(int userId, int pageNumber = 1, int pageSize = 10)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid userId");
        if (pageNumber < 1)
            throw new ArgumentException("Invalid pageNumber");
        if (pageSize <= 0)
            throw new ArgumentException("Invalid pageSize");

        return await _foodRepository.GetAllAsync(userId, pageNumber, pageSize);
    }

    public async Task<Food> AddAsync(Food entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
        if (entity.UserId <= 0)
            throw new ArgumentException("Invalid UserId");
        if (string.IsNullOrWhiteSpace(entity.Name))
            throw new ArgumentException("Name is required");

        return await _foodRepository.AddAsync(entity);
    }

    public async Task<Food> UpdateAsync(Food entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
        if (entity.Id <= 0 || entity.UserId <= 0)
            throw new ArgumentException("Invalid Id or UserId");

        var existing = await _foodRepository.GetByIdAsync(entity.Id, entity.UserId);
        if (existing == null)
            throw new NotFoundException($"Food with ID {entity.Id} not found for user {entity.UserId}");

        return await _foodRepository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        if (id <= 0 || userId <= 0)
            throw new ArgumentException("Invalid id or userId");

        return await _foodRepository.DeleteAsync(id, userId);
    }

    public async Task<bool> ExistsAsync(int id, int userId)
    {
        if (id <= 0 || userId <= 0)
            throw new ArgumentException("Invalid id or userId");

        return await _foodRepository.ExistsAsync(id, userId);
    }

    public async Task<Food?> GetFoodDetailsAsync(int foodId, int userId)
    {
        if (foodId <= 0 || userId <= 0)
            throw new ArgumentException("Invalid foodId or userId");

        var food = await _foodRepository.GetFoodDetailsAsync(foodId, userId);
        if (food == null)
            throw new NotFoundException($"Food with ID {foodId} not found for user {userId}");

        return food;
    }
}