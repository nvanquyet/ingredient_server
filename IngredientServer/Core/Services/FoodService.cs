using System.Net.Http;
using System.Net.Http.Json;
using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs.Entity;
using Microsoft.AspNetCore.Http;
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
    IUserContextService userContextService,
    IImageService imageService)
    : IFoodService
{
    public async Task<FoodDataResponseDto> CreateFoodAsync(CreateFoodRequestDto dataDto)
    {
        //Convert to Food
        if (dataDto == null)
        {
            throw new ArgumentNullException(nameof(dataDto), "Food data cannot be null.");
        }
        string? imageUrl = "";
        if (dataDto.Image is { Length: > 0 })
        {
            // Lưu ảnh và lấy URL
            imageUrl = await imageService.SaveImageAsync(dataDto.Image);
        }
        var food = new Food
        {
            UserId = userContextService.GetAuthenticatedUserId(),
            Name = dataDto.Name,
            Description = dataDto.Description,
            ImageUrl = imageUrl,
            PreparationTimeMinutes = dataDto.PreparationTimeMinutes,
            CookingTimeMinutes = dataDto.CookingTimeMinutes,
            Calories = dataDto.Calories,
            Protein = dataDto.Protein,
            Carbohydrates = dataDto.Carbohydrates,
            Fat = dataDto.Fat,
            Fiber = dataDto.Fiber,
            Instructions = dataDto.Instructions,
            Tips = dataDto.Tips,
            DifficultyLevel = dataDto.DifficultyLevel
        };

        var savedFood = await foodRepository.AddAsync(food);

        // Kiểm tra meal tồn tại
        var meal = (await mealRepository.GetByDateAsync(dataDto.MealDate)).FirstOrDefault(m =>
                       m.MealType == dataDto.MealType) ??
                   await mealRepository.AddAsync(new Meal
                   {
                       MealType = dataDto.MealType,
                       MealDate = dataDto.MealDate,
                       UserId = userContextService.GetAuthenticatedUserId()
                   });
        

        // Liên kết food với meal
        var mealFood = new MealFood
        {
            MealId = meal.Id,
            FoodId = savedFood.Id,
            UserId = userContextService.GetAuthenticatedUserId()
        };

        await mealFoodRepository.AddAsync(mealFood);

        // Trừ ingredients từ kho
        foreach (var ingredient in dataDto.Ingredients)
        {
            if(ingredient is not { IngredientId: > 0 } || ingredient.Quantity <= 0)
            {
                continue;
            }
            var existingIngredient = await ingredientRepository.GetByIdAsync(ingredient.IngredientId);
            if (existingIngredient == null ) continue;
            if(existingIngredient.Quantity < ingredient.Quantity)
            {
                existingIngredient.Quantity = 0;
            }
            else
            {
                existingIngredient.Quantity -= ingredient.Quantity;
            }
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

        return new FoodDataResponseDto
        {
            Id = savedFood.Id,
            Name = savedFood.Name,
            Description = savedFood.Description,
            ImageUrl = savedFood.ImageUrl,
            PreparationTimeMinutes = savedFood.PreparationTimeMinutes,
            CookingTimeMinutes = savedFood.CookingTimeMinutes,
            Calories = savedFood.Calories,
            Protein = savedFood.Protein,
            Carbohydrates = savedFood.Carbohydrates,
            Fat = savedFood.Fat,
            Fiber = savedFood.Fiber,
            Instructions = savedFood.Instructions,
            Tips = savedFood.Tips,
            DifficultyLevel = savedFood.DifficultyLevel,
            MealType = dataDto.MealType,
            MealDate = dataDto.MealDate,
            Ingredients = dataDto.Ingredients.Select(i => new FoodIngredientDto
            {
                IngredientId = i.IngredientId,
                Quantity = i.Quantity,
                Unit = i.Unit,
                IngredientName = i.IngredientName
            }).ToList()
        };
    }

    public async Task<FoodDataResponseDto> UpdateFoodAsync(UpdateFoodRequestDto dto)
    {
        var food = await foodRepository.GetByIdWithIngredientsAsync(dto.Id);
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

        await foodIngredientRepository.DeleteAsync(fi => fi.FoodId == dto.Id);

        
        // Cập nhật thông tin food
        if (!string.IsNullOrEmpty(food.ImageUrl) && dto.Image is { Length: > 0 })
        {
            food.ImageUrl = await imageService.UpdateImageAsync(dto.Image, food.ImageUrl);
        }
        else
        {
            food.ImageUrl = await imageService.SaveImageAsync(dto.Image);
        }
        food.Name = dto.Name;
        food.Description = dto.Description;
        food.PreparationTimeMinutes = dto.PreparationTimeMinutes;
        food.CookingTimeMinutes = dto.CookingTimeMinutes;
        food.Calories = dto.Calories;
        food.Protein = dto.Protein;
        food.Carbohydrates = dto.Carbohydrates;
        food.Fat = dto.Fat;
        food.Fiber = dto.Fiber;
        food.Instructions = dto.Instructions;
        food.Tips = dto.Tips;
        food.DifficultyLevel = dto.DifficultyLevel;
        food.UserId = userContextService.GetAuthenticatedUserId();
        
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
        await mealFoodRepository.DeleteAsync(mf => mf.FoodId == food.Id);

        // TẠO liên kết mới
        var newMealFood = new MealFood
        {
            MealId = meal.Id,
            FoodId = food.Id,
            UserId = userContextService.GetAuthenticatedUserId()
        };
        await mealFoodRepository.AddAsync(newMealFood);

        // Trừ ingredients mới
        foreach (var ingredient in dto.Ingredients)
        {
            if(ingredient is not { IngredientId: > 0 } || ingredient.Quantity <= 0)
            {
                continue;
            }
            var existingIngredient = await ingredientRepository.GetByIdAsync(ingredient.IngredientId);
            if (existingIngredient == null) continue;
            if (existingIngredient.Quantity < ingredient.Quantity)
            {
                existingIngredient.Quantity = 0;
            }
            else
            {
                existingIngredient.Quantity -= ingredient.Quantity;
            }
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

        return new FoodDataResponseDto
        {
            Id = food.Id,
            Name = food.Name,
            Description = food.Description,
            ImageUrl = food.ImageUrl,
            PreparationTimeMinutes = food.PreparationTimeMinutes,
            CookingTimeMinutes = food.CookingTimeMinutes,
            Calories = food.Calories,
            Protein = food.Protein,
            Carbohydrates = food.Carbohydrates,
            Fat = food.Fat,
            Fiber = food.Fiber,
            Instructions = food.Instructions,
            Tips = food.Tips,
            DifficultyLevel = food.DifficultyLevel,
            MealType = dto.MealType,
            MealDate = dto.MealDate,
            Ingredients = dto.Ingredients.Select(i => new FoodIngredientDto
            {
                IngredientId = i.IngredientId,
                Quantity = i.Quantity,
                Unit = i.Unit,
                IngredientName = i.IngredientName
            }).ToList()
        };
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
        
        //Delete food image
        if (!string.IsNullOrEmpty(food.ImageUrl))
        {
            await imageService.DeleteImageAsync(food.ImageUrl);
        }

        // Xóa food
        return await foodRepository.DeleteAsync(foodId);
    }

    public async Task<List<FoodSuggestionResponseDto>> GetSuggestionsAsync(FoodSuggestionRequestDto requestDto)
    {
        //Todo: Gọi API bên ngoài để lấy gợi ý thực phẩm
        var ingredients = await ingredientRepository.GetAllAsync();
        var ingredientDto = ingredients.Select(i => new FoodIngredientDto
        {
            IngredientId = i.Id,
            Quantity = 1,
            Unit = i.Unit,
            IngredientName = i.Name
        }).ToList();
        var response = await aiService.GetSuggestionsAsync(requestDto, ingredientDto);
        if (response == null || !response.Any())
        {
            throw new HttpRequestException("Failed to fetch food suggestions.");
        }

        return response;
    }

    public async Task<FoodDataResponseDto> GetRecipeSuggestionsAsync(FoodRecipeRequestDto recipeRequest)
    {
        var response = await aiService.GetRecipeSuggestionsAsync(recipeRequest);
        if (response == null)
        {
            throw new HttpRequestException("Failed to fetch recipe suggestions.");
        }
        return response;
    }

    public async Task<FoodDataResponseDto> GetFoodByIdAsync(int id)
    {
        var food = await foodRepository.GetByIdAsync(id);
        if (food == null)
        {
            throw new UnauthorizedAccessException("Food not found or access denied.");
        }
        var mealFood = food.MealFoods.FirstOrDefault();
        
        return new FoodDataResponseDto
        {
            Id = food.Id,
            Name = food.Name,
            Description = food.Description,
            ImageUrl = food.ImageUrl,
            PreparationTimeMinutes = food.PreparationTimeMinutes,
            CookingTimeMinutes = food.CookingTimeMinutes,
            Calories = food.Calories,
            Protein = food.Protein,
            Carbohydrates = food.Carbohydrates,
            Fat = food.Fat,
            Fiber = food.Fiber,
            Instructions = food.Instructions,
            Tips = food.Tips,
            DifficultyLevel = food.DifficultyLevel,
            MealType = mealFood?.Meal.MealType ?? MealType.Breakfast,
            MealDate = mealFood?.Meal.MealDate ?? DateTime.UtcNow,
            Ingredients = food.FoodIngredients.Select(fi => new FoodIngredientDto
            {
                IngredientId = fi.IngredientId,
                Quantity = fi.Quantity,
                Unit = fi.Unit,
                IngredientName = fi.Ingredient.Name
            }).ToList()
        };
    }
}