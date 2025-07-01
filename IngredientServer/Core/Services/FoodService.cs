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
    IAIService aiService,
    IUserContextService userContextService)
    : IFoodService
{
    public async Task<Food> CreateFoodAsync(FoodDataDto dataDto)
    {
        //Convert to Food
        if (dataDto == null)
        {
            throw new ArgumentNullException(nameof(dataDto), "Food data cannot be null.");
        }

        // Kiểm tra meal tồn tại
        var meal = (await mealRepository.GetByDateAsync(dataDto.MealDate)).FirstOrDefault(m =>
                       m.MealType == dataDto.MealType) ??
                   await mealRepository.AddAsync(new Meal
                   {
                       MealType = dataDto.MealType,
                       MealDate = dataDto.MealDate,
                       UserId = userContextService.GetAuthenticatedUserId()
                   });

        var food = dataDto.ToFood();
        // Tạo food
        food.UserId = userContextService.GetAuthenticatedUserId();
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

        foreach (var foodIngredient in food.FoodIngredients)
        {
            var ingredient = await ingredientRepository.GetByIdAsync(foodIngredient.IngredientId);
            if (ingredient == null) continue;
            ingredient.Quantity += foodIngredient.Quantity;
            await ingredientRepository.UpdateAsync(ingredient);
        }

        await foodIngredientRepository.DeleteAsync(fi => fi.FoodId == foodId);

        // Cập nhật thông tin food
        food.UpdateFromDto(dto);

        await foodRepository.UpdateAsync(food);

        // Cập nhật meal nếu thay đổi ngày/bữa ăn
        var meal = (await mealRepository.GetByDateAsync(dto.MealDate))
            .FirstOrDefault(m => m.MealType == dto.MealType) ?? await mealRepository.AddAsync(new Meal
            {
                MealType = dto.MealType,
                MealDate = dto.MealDate,
                UserId = userContextService.GetAuthenticatedUserId()
            });

        // XÓA tất cả liên kết cũ của food này
        await mealFoodRepository.DeleteAsync(mf => mf.FoodId == foodId);

        // TẠO liên kết mới
        var newMealFood = new MealFood
        {
            MealId = meal.Id,
            FoodId = foodId,
            UserId = userContextService.GetAuthenticatedUserId()
        };
        await mealFoodRepository.AddAsync(newMealFood);

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
        if(!requestDto.Ingredients.Any())
        {
            //Get all Ingredient if no ingredients provided
            var ingredients = await ingredientRepository.GetAllAsync();
            requestDto.Ingredients = ingredients.Select(i => new FoodIngredientDto
            {
                IngredientId = i.Id,
                Quantity = 1, // Default quantity, can be adjusted based on requirements
                Unit = i.Unit,
                IngredientName = i.Name
            }).ToList();
        }
        var response = await aiService.GetSuggestionsAsync(requestDto);
        if (response == null || !response.Any())
        {
            throw new HttpRequestException("Failed to fetch food suggestions.");
        }

        return response;
    }

    public async Task<FoodDataDto> GetRecipeSuggestionsAsync(FoodRecipeRequestDto recipeRequest)
    {
        var response = await aiService.GetRecipeSuggestionsAsync(recipeRequest);
        if (response == null)
        {
            throw new HttpRequestException("Failed to fetch recipe suggestions.");
        }
        return response;
    }

    public async Task<FoodDataDto> GetFoodByIdAsync(int id)
    {
        var food = await foodRepository.GetByIdAsync(id);
        if (food == null)
        {
            throw new UnauthorizedAccessException("Food not found or access denied.");
        }

        return FoodDataDto.FromFood(food);
    }
}