using System.Net.Http;
using System.Net.Http.Json;
using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs.Entity;
using Microsoft.AspNetCore.Mvc;

namespace IngredientServer.Core.Services;

public class FoodService(
    IFoodRepository foodRepository,
    IMealRepository mealRepository,
    IMealFoodRepository mealFoodRepository,
    IIngredientRepository ingredientRepository,
    IFoodIngredientRepository foodIngredientRepository,
    HttpClient httpClient,
    IUserContextService userContextService)
    : IFoodService
{
    public async Task<Food> CreateFoodAsync(FoodDataDto dataDto)
    {
        // Kiểm tra meal tồn tại
        var meal = (await mealRepository.GetByDateAsync(dataDto.Date)).FirstOrDefault(m => m.MealType == dataDto.MealType) ??
                   await mealRepository.AddAsync(new Meal
        {
            MealType = dataDto.MealType,
            MealDate = dataDto.Date,
            UserId = userContextService.GetAuthenticatedUserId()
        });

        // Tạo food
        var food = new Food
        {
            Name = dataDto.Name,
            Description = dataDto.Description,
            Quantity = dataDto.Quantity,
            Calories = dataDto.Calories,
            Protein = dataDto.Protein,
            Carbs = dataDto.Carbs,
            Fat = dataDto.Fat,
            UserId = userContextService.GetAuthenticatedUserId()
        };

        var savedFood = await foodRepository.AddAsync(food);

        // Liên kết food với meal
        var mealFood = new MealFood
        {
            MealId = meal.Id,
            FoodId = food.Id,
            UserId = userContextService.GetAuthenticatedUserId()
        };
        await mealFoodRepository.AddAsync(mealFood);

        // Trừ ingredients từ kho
        foreach (var ingredient in dataDto.Ingredients)
        {
            var existingIngredient = await ingredientRepository.GetByIdAsync(ingredient.IngredientId);
            if (existingIngredient == null || existingIngredient.Quantity < ingredient.Quantity)
            {
                throw new InvalidOperationException($"Insufficient ingredient: {ingredient.IngredientName}");
            }

            existingIngredient.Quantity -= ingredient.Quantity;
            await ingredientRepository.UpdateAsync(existingIngredient);

            var foodIngredient = new FoodIngredient
            {
                FoodId = food.Id,
                IngredientId = ingredient.IngredientId,
                Quantity = ingredient.Quantity,
                UserId = userContextService.GetAuthenticatedUserId()
            };
            await foodIngredientRepository.AddAsync(foodIngredient);
        }

        return savedFood;
    }

    public async Task<Food> UpdateFoodAsync(int foodId, FoodDataDto dto)
    {
        var food = await foodRepository.GetByIdWithIngredientsAsync(foodId);
        if (food == null)
        {
            throw new UnauthorizedAccessException("Food not found or access denied.");
        }

        // Hoàn trả ingredients cũ
        foreach (var foodIngredient in food.FoodIngredients)
        {
            var ingredient = await ingredientRepository.GetByIdAsync(foodIngredient.IngredientId);
            if (ingredient == null) continue;
            ingredient.Quantity += foodIngredient.Quantity;
            await ingredientRepository.UpdateAsync(ingredient);
        }
        await foodIngredientRepository.DeleteAsync(fi => fi.FoodId == foodId);

        // Cập nhật thông tin food
        food.Name = dto.Name;
        food.Calories = dto.Calories;
        food.Protein = dto.Protein;
        food.Carbs = dto.Carbs;
        food.Fat = dto.Fat;
        food.Quantity = dto.Quantity;
        await foodRepository.UpdateAsync(food);

        // Cập nhật meal nếu thay đổi ngày/bữa ăn
        var meal = (await mealRepository.GetByDateAsync(dto.Date))
            .FirstOrDefault(m => m.MealType == dto.MealType) ?? await mealRepository.AddAsync(new Meal
            {
                MealType = dto.MealType,
                MealDate = dto.Date,
                UserId = userContextService.GetAuthenticatedUserId()
            });

        var mealFood = (await mealFoodRepository.GetByMealIdAsync(foodId)).FirstOrDefault();
        if (mealFood != null)
        {
            mealFood.MealId = meal.Id;
            await mealFoodRepository.UpdateAsync(mealFood);
        }

        // Trừ ingredients mới
        foreach (var ingredient in dto.Ingredients)
        {
            var existingIngredient = await ingredientRepository.GetByIdAsync(ingredient.IngredientId);
            if (existingIngredient == null || existingIngredient.Quantity < ingredient.Quantity)
            {
                throw new InvalidOperationException($"Insufficient ingredient: {ingredient.IngredientName}");
            }

            existingIngredient.Quantity -= ingredient.Quantity;
            await ingredientRepository.UpdateAsync(existingIngredient);

            var foodIngredient = new FoodIngredient
            {
                FoodId = food.Id,
                IngredientId = ingredient.IngredientId,
                Quantity = ingredient.Quantity,
                UserId = userContextService.GetAuthenticatedUserId()
            };
            await foodIngredientRepository.AddAsync(foodIngredient);
        }

        return food;
    }

    public async Task<bool> DeleteFoodAsync(int foodId)
    {
        var food = await foodRepository.GetByIdWithIngredientsAsync(foodId);

        // Hoàn trả ingredients
        foreach (var foodIngredient in food.FoodIngredients)
        {
            var ingredient = await ingredientRepository.GetByIdAsync(foodIngredient.IngredientId);
            if (ingredient == null) continue;
            ingredient.Quantity += foodIngredient.Quantity;
            await ingredientRepository.UpdateAsync(ingredient);
        }

        // Xóa liên kết meal-food và food-ingredient
        await mealFoodRepository.DeleteAsync(mf => mf.FoodId == foodId);
        await foodIngredientRepository.DeleteAsync(fi => fi.FoodId == foodId);

        // Xóa food
        return await foodRepository.DeleteAsync(foodId);
    }

    public async Task<List<FoodSuggestionDto>> GetSuggestionsAsync(FoodSuggestionRequestDto requestDto)
    {
        //Todo: Gọi API bên ngoài để lấy gợi ý thực phẩm
        var response = await httpClient.GetFromJsonAsync<List<FoodSuggestionDto>>("https://api.example.com/food-suggestions");
        if (response == null || !response.Any())
        {
            throw new HttpRequestException("Failed to fetch food suggestions.");
        }
        return response;
    }

    public async Task<FoodRecipeDto> GetRecipeSuggestionsAsync(FoodRecipeRequestDto recipeRequest)
    {
        //Todo: Gọi API bên ngoài để lấy gợi ý công thức nấu ăn
        
        var response = await httpClient.GetFromJsonAsync<FoodRecipeDto>("https://api.example.com/recipes");
        if (response == null)
        {
            throw new HttpRequestException("Failed to fetch recipe suggestions.");
        }
        return response;
    }
}