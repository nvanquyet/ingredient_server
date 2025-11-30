using System.Net.Http;
using System.Net.Http.Json;
using IngredientServer.Core.Entities;
using IngredientServer.Core.Helpers;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs.Entity;
using IngredientServer.Utils.Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IngredientServer.Core.Services;

public class FoodService(
    IFoodRepository foodRepository,
    IMealRepository mealRepository,
    IMealFoodRepository mealFoodRepository,
    IIngredientRepository ingredientRepository,
    IFoodIngredientRepository foodIngredientRepository,
    IAIService aiService,
    IUserContextService userContextService,
    IImageService imageService,
    ILogger<FoodService> logger)
    : IFoodService
{
    public async Task<FoodDataResponseDto> CreateFoodAsync(CreateFoodRequestDto dataDto)
    {
        var userId = userContextService.GetAuthenticatedUserId();
        var operationId = Guid.NewGuid().ToString("N")[..8];

        logger.LogInformation("=== START CREATE FOOD OPERATION ===");
        logger.LogInformation("Operation ID: {OperationId}, User ID: {UserId}", operationId, userId);
        logger.LogInformation("Food Name: {FoodName}, MealType: {MealType}, MealDate: {MealDate}",
            dataDto.Name, dataDto.MealType, dataDto.MealDate);

        if (dataDto == null)
        {
            logger.LogError("CreateFoodAsync called with null dataDto");
            throw new ArgumentNullException(nameof(dataDto), "Food data cannot be null.");
        }

        dataDto.NormalizeConsumedAt();

        string? imageUrl = "";

        // Image processing with logging
        if (dataDto.Image is { Length: > 0 })
        {
            logger.LogInformation("Processing image upload - Size: {ImageSize} bytes, ContentType: {ContentType}",
                dataDto.Image.Length, dataDto.Image.ContentType);

            try
            {
                imageUrl = await imageService.SaveImageAsync(dataDto.Image);
                logger.LogInformation("Image saved successfully: {ImageUrl}", imageUrl);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save image for food {FoodName}", dataDto.Name);
                throw;
            }
        }
        else
        {
            logger.LogInformation("No image provided for food creation");
        }

        // Create Food entity
        var food = new Food
        {
            UserId = userId,
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
            ConsumedAt = dataDto.ConsumedAt,
            DifficultyLevel = dataDto.DifficultyLevel
        };

        logger.LogInformation("Saving food entity to database...");
        var savedFood = await foodRepository.AddAsync(food);
        logger.LogInformation("Food saved with ID: {FoodId}", savedFood.Id);

        // Normalize to UTC and get date only (bỏ time component)
        var mealDate = DateTimeHelper.NormalizeToUtc(dataDto.MealDate).Date;

        // Handle Meal logic
        logger.LogInformation("Checking for existing meal - Date: {MealDate}, Type: {MealType}",
            mealDate, dataDto.MealType);

        var existingMeals = await mealRepository.GetByDateAsync(mealDate);
        var meal = existingMeals.FirstOrDefault(m => m.MealType == dataDto.MealType);

        if (meal == null)
        {
            logger.LogInformation("Creating new meal for {MealType} on {MealDate}",
                dataDto.MealType, mealDate);
            meal = await mealRepository.AddAsync(new Meal
            {
                MealType = dataDto.MealType,
                MealDate = mealDate, // FIX: Chỉ lưu Date
                UserId = userId
            });
            logger.LogInformation("New meal created with ID: {MealId}", meal.Id);
        }
        else
        {
            logger.LogInformation("Using existing meal with ID: {MealId}, Current food count: {Count}",
                meal.Id, meal.MealFoods?.Count ?? 0);
        }

        // Link Food with Meal
        logger.LogInformation("Linking food {FoodId} with meal {MealId}", savedFood.Id, meal.Id);
        var mealFood = new MealFood
        {
            MealId = meal.Id,
            FoodId = savedFood.Id,
            UserId = userId
        };
        await mealFoodRepository.AddAsync(mealFood);
        logger.LogInformation("Food-Meal link created successfully");

        // Process Ingredients
        logger.LogInformation("Processing {IngredientCount} ingredients", dataDto.Ingredients?.Count() ?? 0);
        var processedIngredients = 0;
        var skippedIngredients = 0;
        var insufficientIngredients = new List<string>();

        foreach (var ingredient in dataDto.Ingredients ?? [])
        {
            if (ingredient is not { IngredientId: > 0 } || ingredient.Quantity <= 0)
            {
                logger.LogWarning("Skipping invalid ingredient - ID: {IngredientId}, Quantity: {Quantity}",
                    ingredient?.IngredientId, ingredient?.Quantity);
                skippedIngredients++;
                continue;
            }

            logger.LogDebug("Processing ingredient ID: {IngredientId}, Required quantity: {Quantity}",
                ingredient.IngredientId, ingredient.Quantity);

            var existingIngredient = await ingredientRepository.GetByIdAsync(ingredient.IngredientId);
            if (existingIngredient == null)
            {
                logger.LogWarning("Ingredient not found with ID: {IngredientId}", ingredient.IngredientId);
                skippedIngredients++;
                continue;
            }

            logger.LogDebug("Found ingredient: {IngredientName}, Available: {Available}, Required: {Required}",
                existingIngredient.Name, existingIngredient.Quantity, ingredient.Quantity);

            // Update ingredient quantity
            if (existingIngredient.Quantity < ingredient.Quantity)
            {
                logger.LogWarning(
                    "Insufficient ingredient {IngredientName} - Available: {Available}, Required: {Required}. Setting to 0.",
                    existingIngredient.Name, existingIngredient.Quantity, ingredient.Quantity);
                insufficientIngredients.Add(
                    $"{existingIngredient.Name} (needed: {ingredient.Quantity}, available: {existingIngredient.Quantity})");
                existingIngredient.Quantity = 0;
            }
            else
            {
                logger.LogDebug("Deducting {Quantity} from {IngredientName}", ingredient.Quantity,
                    existingIngredient.Name);
                existingIngredient.Quantity -= ingredient.Quantity;
            }

            await ingredientRepository.UpdateAsync(existingIngredient);
            logger.LogDebug("Ingredient {IngredientName} updated, new quantity: {NewQuantity}",
                existingIngredient.Name, existingIngredient.Quantity);

            // Create FoodIngredient relationship
            var foodIngredient = new FoodIngredient
            {
                FoodId = savedFood.Id,
                IngredientId = ingredient.IngredientId,
                Quantity = ingredient.Quantity,
                UserId = userId
            };
            await foodIngredientRepository.AddAsync(foodIngredient);
            processedIngredients++;
        }

        // Log ingredient processing summary
        logger.LogInformation("Ingredient processing complete - Processed: {Processed}, Skipped: {Skipped}",
            processedIngredients, skippedIngredients);

        if (insufficientIngredients.Count > 0)
        {
            logger.LogWarning("Some ingredients had insufficient quantities: {InsufficientIngredients}",
                string.Join(", ", insufficientIngredients));
        }

        // Create response
        var response = new FoodDataResponseDto
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
            MealDate = mealDate, // FIX: Trả về Date đã chuẩn hóa
            ConsumedAt = dataDto.ConsumedAt,
            Ingredients = dataDto.Ingredients?.Select(i => new FoodIngredientDto
            {
                IngredientId = i.IngredientId,
                Quantity = i.Quantity,
                Unit = i.Unit,
                IngredientName = i.IngredientName
            }).ToList() ?? new List<FoodIngredientDto>()
        };

        logger.LogInformation("=== FOOD CREATION COMPLETED SUCCESSFULLY ===");
        logger.LogInformation("Operation ID: {OperationId}, Food ID: {FoodId}",
            operationId, savedFood.Id);

        if (insufficientIngredients.Count > 0)
        {
            logger.LogInformation("⚠️  Warning: Some ingredients had insufficient stock");
        }

        return response;
    }

    public async Task<FoodDataResponseDto> UpdateFoodAsync(UpdateFoodRequestDto dto)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        logger.LogInformation("=== START UPDATE FOOD OPERATION ===");
        logger.LogInformation("Operation ID: {OperationId}, Food ID: {FoodId}", operationId, dto.Id);

        dto.NormalizeConsumedAt();

        var food = await foodRepository.GetByIdAsync(dto.Id);
        if (food == null)
        {
            logger.LogWarning("Food with ID {FoodId} not found or access denied", dto.Id);
            throw new UnauthorizedAccessException("Food not found or access denied.");
        }

        // Image handling
        if (!string.IsNullOrEmpty(food.ImageUrl) && dto.Image is { Length: > 0 })
        {
            food.ImageUrl = await imageService.UpdateImageAsync(dto.Image, food.ImageUrl);
            logger.LogInformation("Image updated successfully: {ImageUrl}", food.ImageUrl);
        }
        else if (dto.Image is { Length: > 0 })
        {
            food.ImageUrl = await imageService.SaveImageAsync(dto.Image);
            logger.LogInformation("Image saved successfully: {ImageUrl}", food.ImageUrl);
        }

        // Update food fields
        logger.LogInformation("Updating food fields...");
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
        food.ConsumedAt = dto.ConsumedAt;
        food.UserId = userContextService.GetAuthenticatedUserId();

        await foodRepository.UpdateAsync(food);
        logger.LogInformation("Food with ID {FoodId} updated successfully.", food.Id);

        // Normalize to UTC and get date only (bỏ time component)
        var mealDate = DateTimeHelper.NormalizeToUtc(dto.MealDate).Date;

        // Update meal info
        logger.LogInformation("Checking meal for date {MealDate} and type {MealType}", mealDate, dto.MealType);

        var existingMeals = await mealRepository.GetByDateAsync(mealDate);
        var meal = existingMeals.FirstOrDefault(m => m.MealType == dto.MealType);

        if (meal == null)
        {
            logger.LogInformation("Creating new meal for {MealType} on {MealDate}", dto.MealType, mealDate);
            meal = await mealRepository.AddAsync(new Meal
            {
                MealType = dto.MealType,
                MealDate = mealDate, // FIX: Chỉ lưu Date
                UserId = userContextService.GetAuthenticatedUserId()
            });
            logger.LogInformation("New meal created with ID: {MealId}", meal.Id);
        }
        else
        {
            logger.LogInformation("Using existing meal with ID: {MealId}", meal.Id);
        }

        // Delete old meal-food links and create new one
        await mealFoodRepository.DeleteAsync(mf => mf.FoodId == food.Id);
        logger.LogInformation("Old meal-food links deleted for Food ID: {FoodId}", food.Id);

        await mealFoodRepository.AddAsync(new MealFood
        {
            MealId = meal.Id,
            FoodId = food.Id,
            UserId = userContextService.GetAuthenticatedUserId()
        });
        logger.LogInformation("New meal-food link created: MealId={MealId}, FoodId={FoodId}", meal.Id, food.Id);

        logger.LogInformation("=== FOOD UPDATE COMPLETED SUCCESSFULLY ===");

        // Reload food with relationships to get updated data
        var updatedFood = await foodRepository.GetByIdAsync(food.Id);
        if (updatedFood == null)
        {
            throw new UnauthorizedAccessException("Food not found after update");
        }

        return updatedFood.ToDto();
    }

    public async Task<bool> DeleteFoodAsync(int foodId)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        logger.LogInformation("=== START DELETE FOOD OPERATION ===");
        logger.LogInformation("Operation ID: {OperationId}, Food ID: {FoodId}", operationId, foodId);

        // Validate input
        if (foodId <= 0)
        {
            logger.LogWarning("Invalid food ID: {FoodId}", foodId);
            throw new ArgumentException("Food ID must be greater than 0.", nameof(foodId));
        }

        var food = await foodRepository.GetByIdWithIngredientsAsync(foodId);
        if (food == null)
        {
            logger.LogWarning("Food with ID {FoodId} not found for deletion.", foodId);
            throw new UnauthorizedAccessException("Food not found or access denied.");
        }

        // LƯU LẠI MealIds TRƯỚC KHI XÓA (để check empty meals sau)
        var affectedMealIds = food.MealFoods?.Select(mf => mf.MealId).Distinct().ToList()
                              ?? new List<int>();
        logger.LogInformation("Food {FoodId} belongs to {Count} meal(s): [{MealIds}]",
            foodId, affectedMealIds.Count, string.Join(", ", affectedMealIds));

        logger.LogInformation("Restoring ingredients from food {FoodId}...", foodId);

        // Check if FoodIngredients collection exists and is not empty
        if (food.FoodIngredients?.Any() == true)
        {
            logger.LogInformation("Found {Count} ingredients to restore for food {FoodId}",
                food.FoodIngredients.Count, foodId);

            foreach (var foodIngredient in food.FoodIngredients)
            {
                // Validate foodIngredient
                if (foodIngredient == null)
                {
                    logger.LogWarning("Null food ingredient found for food {FoodId}, skipping", foodId);
                    continue;
                }

                // Validate ingredient ID
                if (foodIngredient.IngredientId <= 0)
                {
                    logger.LogWarning("Invalid ingredient ID {IngredientId} for food {FoodId}, skipping",
                        foodIngredient.IngredientId, foodId);
                    continue;
                }

                // Validate quantity
                if (foodIngredient.Quantity <= 0)
                {
                    logger.LogWarning(
                        "Invalid quantity {Quantity} for ingredient {IngredientId} in food {FoodId}, skipping",
                        foodIngredient.Quantity, foodIngredient.IngredientId, foodId);
                    continue;
                }

                var ingredient = await ingredientRepository.GetByIdAsync(foodIngredient.IngredientId);
                if (ingredient == null)
                {
                    logger.LogWarning("Ingredient with ID {IngredientId} not found, cannot restore quantity",
                        foodIngredient.IngredientId);
                    continue;
                }

                // Check for potential overflow
                if (ingredient.Quantity > decimal.MaxValue - foodIngredient.Quantity)
                {
                    logger.LogWarning(
                        "Quantity overflow detected for ingredient {IngredientId}, setting to maximum value",
                        ingredient.Id);
                    ingredient.Quantity = decimal.MaxValue;
                }
                else
                {
                    ingredient.Quantity += foodIngredient.Quantity;
                }

                await ingredientRepository.UpdateAsync(ingredient);
                logger.LogInformation("Restored {Quantity} to ingredient {IngredientName} (ID: {IngredientId})",
                    foodIngredient.Quantity, ingredient.Name, ingredient.Id);
            }
        }
        else
        {
            logger.LogInformation("No ingredients found to restore for food {FoodId}", foodId);
        }

        logger.LogInformation("Deleting meal-food and food-ingredient links for Food ID: {FoodId}", foodId);

        // Delete related entities
        try
        {
            await mealFoodRepository.DeleteSafeAsync(mf => mf.FoodId == foodId);
            await foodIngredientRepository.DeleteSafeAsync(fi => fi.FoodId == foodId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting meal-food or food-ingredient links for Food ID: {FoodId}", foodId);
            throw;
        }

        // Handle image deletion
        if (!string.IsNullOrEmpty(food.ImageUrl))
        {
            try
            {
                logger.LogInformation("Deleting image for food ID: {FoodId}", foodId);
                await imageService.DeleteImageAsync(food.ImageUrl);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to delete image {ImageUrl} for food {FoodId}, continuing with deletion",
                    food.ImageUrl, foodId);
                // Don't throw - image deletion failure shouldn't stop food deletion
            }
        }

        logger.LogInformation("Deleting food record with ID: {FoodId}", foodId);
        var result = await foodRepository.DeleteAsync(foodId);

        // THÊM: Cleanup empty meals
        logger.LogInformation("Checking for empty meals after food deletion...");
        var emptyMealsDeleted = 0;

        foreach (var mealId in affectedMealIds)
        {
            try
            {
                // Đếm số Food còn lại trong Meal
                var remainingFoods = await mealFoodRepository.GetByMealIdAsync(mealId);

                if (remainingFoods == null || !remainingFoods.Any())
                {
                    logger.LogInformation("Meal {MealId} is now empty, deleting...", mealId);
                    await mealRepository.DeleteAsync(mealId);
                    emptyMealsDeleted++;
                    logger.LogInformation("Empty meal {MealId} deleted successfully", mealId);
                }
                else
                {
                    logger.LogInformation("Meal {MealId} still has {Count} food(s), keeping it",
                        mealId, remainingFoods.Count());
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to check/delete empty meal {MealId}, continuing", mealId);
                // Don't throw - meal cleanup failure shouldn't stop the operation
            }
        }

        if (emptyMealsDeleted > 0)
        {
            logger.LogInformation("Cleaned up {Count} empty meal(s)", emptyMealsDeleted);
        }

        logger.LogInformation("=== FOOD DELETE COMPLETED SUCCESSFULLY ===");
        logger.LogInformation("Operation ID: {OperationId}, Food ID: {FoodId}, Empty meals deleted: {EmptyMealsCount}",
            operationId, foodId, emptyMealsDeleted);

        return result;
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

        return food.ToDto();
    }
}