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
    /// <summary>
    /// Helper method to update Meal nutrition values (TotalCalories, TotalProtein, etc.)
    /// </summary>
    private async Task UpdateMealNutritionAsync(Meal meal, Food food, bool isAdding)
    {
        if (meal == null || food == null)
        {
            logger.LogWarning("UpdateMealNutritionAsync: meal or food is null - Meal: {MealNull}, Food: {FoodNull}", 
                meal == null, food == null);
            return;
        }

        logger.LogInformation("=== UPDATE MEAL NUTRITION ===");
        logger.LogInformation("Meal ID: {MealId}, Food ID: {FoodId}, Food Name: {FoodName}", 
            meal.Id, food.Id, food.Name);
        logger.LogInformation("Operation: {Operation}, Food Nutrition - Calories: {Calories}, Protein: {Protein}, Carbs: {Carbs}, Fat: {Fat}, Fiber: {Fiber}",
            isAdding ? "ADDING" : "SUBTRACTING", food.Calories, food.Protein, food.Carbohydrates, food.Fat, food.Fiber);
        logger.LogInformation("Meal BEFORE - Calories: {Calories}, Protein: {Protein}, Carbs: {Carbs}, Fat: {Fat}, Fiber: {Fiber}",
            meal.TotalCalories, meal.TotalProtein, meal.TotalCarbs, meal.TotalFat, meal.TotalFiber);

        var multiplier = isAdding ? 1 : -1;
        
        meal.TotalCalories += (double)food.Calories * multiplier;
        meal.TotalProtein += (double)food.Protein * multiplier;
        meal.TotalCarbs += (double)food.Carbohydrates * multiplier;
        meal.TotalFat += (double)food.Fat * multiplier;
        meal.TotalFiber += (double)food.Fiber * multiplier;

        // Ensure values don't go negative
        meal.TotalCalories = Math.Max(0, meal.TotalCalories);
        meal.TotalProtein = Math.Max(0, meal.TotalProtein);
        meal.TotalCarbs = Math.Max(0, meal.TotalCarbs);
        meal.TotalFat = Math.Max(0, meal.TotalFat);
        meal.TotalFiber = Math.Max(0, meal.TotalFiber);

        logger.LogInformation("Meal AFTER - Calories: {Calories}, Protein: {Protein}, Carbs: {Carbs}, Fat: {Fat}, Fiber: {Fiber}",
            meal.TotalCalories, meal.TotalProtein, meal.TotalCarbs, meal.TotalFat, meal.TotalFiber);

        await mealRepository.UpdateAsync(meal);
        logger.LogInformation("✅ Meal {MealId} nutrition updated and saved to database", meal.Id);
    }

    /// <summary>
    /// Helper method to recalculate and update Meal nutrition from all foods
    /// </summary>
    private async Task RecalculateMealNutritionAsync(Meal meal)
    {
        if (meal == null || meal.Id <= 0) return;

        // Get all foods in meal
        var mealFoods = await mealFoodRepository.GetByMealIdAsync(meal.Id);
        
        // Reset nutrition values
        meal.TotalCalories = 0;
        meal.TotalProtein = 0;
        meal.TotalCarbs = 0;
        meal.TotalFat = 0;
        meal.TotalFiber = 0;

        // Calculate from foods
        foreach (var mealFood in mealFoods)
        {
            if (mealFood.Food == null) continue;
            
            meal.TotalCalories += (double)mealFood.Food.Calories;
            meal.TotalProtein += (double)mealFood.Food.Protein;
            meal.TotalCarbs += (double)mealFood.Food.Carbohydrates;
            meal.TotalFat += (double)mealFood.Food.Fat;
            meal.TotalFiber += (double)mealFood.Food.Fiber;
        }

        await mealRepository.UpdateAsync(meal);
        logger.LogInformation("Recalculated Meal {MealId} nutrition - Calories: {Calories}, Protein: {Protein}, Carbs: {Carbs}, Fat: {Fat}, Fiber: {Fiber}",
            meal.Id, meal.TotalCalories, meal.TotalProtein, meal.TotalCarbs, meal.TotalFat, meal.TotalFiber);
    }
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

        // FIX: Normalize all DateTime fields to UTC
        dataDto.NormalizeConsumedAt();
        dataDto.MealDate = DateTimeHelper.NormalizeToUtc(dataDto.MealDate);

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

        // Update Meal nutrition (add food calories, protein, etc.)
        await UpdateMealNutritionAsync(meal, savedFood, isAdding: true);
        logger.LogInformation("Meal nutrition updated after adding food");

        // Process Ingredients
        var ingredientsList = dataDto.Ingredients?.ToList() ?? new List<FoodIngredientDto>();
        logger.LogInformation("Processing {IngredientCount} ingredients", ingredientsList.Count);
        
        // FIX: Log ingredients for debugging
        foreach (var ing in ingredientsList)
        {
            logger.LogInformation("Ingredient in request - ID: {Id}, Quantity: {Qty}, Unit: {Unit}", 
                ing.IngredientId, ing.Quantity, ing.Unit);
        }
        
        var processedIngredients = 0;
        var skippedIngredients = 0;
        var insufficientIngredients = new List<string>();
        var ingredientResponses = new List<FoodIngredientDto>();

        foreach (var ingredient in ingredientsList)
        {
            // FIX: Handle IngredientId = 0 (user doesn't have this ingredient from cache)
            if (ingredient.IngredientId == 0)
            {
                logger.LogWarning("⚠️ User doesn't have ingredient '{IngredientName}' (ID=0) - skipping. User needs to add this ingredient first.",
                    ingredient.IngredientName ?? "Unknown");
                insufficientIngredients.Add(
                    $"{ingredient.IngredientName ?? "Unknown"} (user doesn't have this ingredient - ID=0)");
                skippedIngredients++;
                continue;
            }

            if (ingredient.IngredientId <= 0 || ingredient.Quantity <= 0)
            {
                logger.LogWarning("Skipping invalid ingredient - ID: {IngredientId}, Quantity: {Quantity}, Name: {Name}",
                    ingredient?.IngredientId, ingredient?.Quantity, ingredient?.IngredientName);
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
            logger.LogInformation("✅ Ingredient {IngredientName} (ID: {Id}) updated - Old: {OldQty}, Deducted: {Deducted}, New: {NewQty}",
                existingIngredient.Name, existingIngredient.Id, 
                existingIngredient.Quantity + ingredient.Quantity, 
                ingredient.Quantity, 
                existingIngredient.Quantity);

            // Create FoodIngredient relationship
            var foodIngredient = new FoodIngredient
            {
                FoodId = savedFood.Id,
                IngredientId = ingredient.IngredientId,
                Quantity = ingredient.Quantity,
                Unit = ingredient.Unit, // FIX: Set Unit
                UserId = userId
            };
            await foodIngredientRepository.AddAsync(foodIngredient);
            logger.LogInformation("✅ FoodIngredient created - FoodId: {FoodId}, IngredientId: {IngredientId}, Quantity: {Qty}, Unit: {Unit}",
                savedFood.Id, ingredient.IngredientId, ingredient.Quantity, ingredient.Unit);
            processedIngredients++;
            
            // Store ingredient info with remaining quantity for response
            ingredientResponses.Add(new FoodIngredientDto
            {
                IngredientId = ingredient.IngredientId,
                Quantity = ingredient.Quantity,
                Unit = ingredient.Unit,
                IngredientName = existingIngredient.Name,
                RemainingQuantity = existingIngredient.Quantity
            });
        }

        // Log ingredient processing summary
        logger.LogInformation("Ingredient processing complete - Processed: {Processed}, Skipped: {Skipped}",
            processedIngredients, skippedIngredients);

        if (insufficientIngredients.Count > 0)
        {
            logger.LogWarning("Some ingredients had insufficient quantities: {InsufficientIngredients}",
                string.Join(", ", insufficientIngredients));
        }

        // FIX: Reload food with all relationships to ensure data is fresh
        var reloadedFood = await foodRepository.GetByIdWithIngredientsAsync(savedFood.Id);
        logger.LogInformation("Reloaded food {FoodId} with {IngredientCount} FoodIngredients", 
            reloadedFood.Id, reloadedFood.FoodIngredients?.Count ?? 0);
        
        // Create response
        var response = new FoodDataResponseDto
        {
            Id = reloadedFood.Id,
            Name = reloadedFood.Name,
            Description = reloadedFood.Description,
            ImageUrl = reloadedFood.ImageUrl,
            PreparationTimeMinutes = reloadedFood.PreparationTimeMinutes,
            CookingTimeMinutes = reloadedFood.CookingTimeMinutes,
            Calories = reloadedFood.Calories,
            Protein = reloadedFood.Protein,
            Carbohydrates = reloadedFood.Carbohydrates,
            Fat = reloadedFood.Fat,
            Fiber = reloadedFood.Fiber,
            Instructions = reloadedFood.Instructions,
            Tips = reloadedFood.Tips,
            DifficultyLevel = reloadedFood.DifficultyLevel,
            MealType = dataDto.MealType,
            MealDate = mealDate, // FIX: Trả về Date đã chuẩn hóa
            ConsumedAt = DateTimeHelper.NormalizeToUtc(dataDto.ConsumedAt),
            Ingredients = reloadedFood.FoodIngredients?.Select(fi => new FoodIngredientDto
            {
                IngredientId = fi.IngredientId,
                Quantity = fi.Quantity,
                Unit = fi.Unit,
                IngredientName = fi.Ingredient?.Name ?? string.Empty,
                RemainingQuantity = ingredientResponses.FirstOrDefault(r => r.IngredientId == fi.IngredientId)?.RemainingQuantity
            }).ToList() ?? new List<FoodIngredientDto>()
        };
        
        logger.LogInformation("✅ Response created with {IngredientCount} ingredients", response.Ingredients.Count());

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

        // FIX: Normalize all DateTime fields to UTC
        dto.NormalizeConsumedAt();
        dto.MealDate = DateTimeHelper.NormalizeToUtc(dto.MealDate);

        // Get food with old ingredients for comparison
        var food = await foodRepository.GetByIdWithIngredientsAsync(dto.Id);
        if (food == null)
        {
            logger.LogWarning("Food with ID {FoodId} not found or access denied", dto.Id);
            throw new UnauthorizedAccessException("Food not found or access denied.");
        }

        // Store old values for Meal nutrition update
        var oldCalories = food.Calories;
        var oldProtein = food.Protein;
        var oldCarbs = food.Carbohydrates;
        var oldFat = food.Fat;
        var oldFiber = food.Fiber;
        
        // Get old meal to subtract old nutrition
        var oldMealFoods = await mealFoodRepository.GetAllAsync(mf => mf.FoodId == food.Id);
        var oldMealFood = oldMealFoods.FirstOrDefault();
        Meal? oldMeal = null;
        if (oldMealFood != null)
        {
            oldMeal = await mealRepository.GetByIdAsync(oldMealFood.MealId);
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

        // Process ingredients: restore old, deduct new
        logger.LogInformation("Processing ingredient changes for food {FoodId}", food.Id);
        var ingredientResponses = new List<FoodIngredientDto>();
        
        // Get old FoodIngredients (need to load from database)
        var oldFoodIngredients = await foodIngredientRepository.GetByFoodIdAsync(food.Id);
        logger.LogInformation("Found {Count} old FoodIngredients to restore for Food ID: {FoodId}", 
            oldFoodIngredients.Count(), food.Id);
        
        // Restore old ingredients
        foreach (var oldFoodIngredient in oldFoodIngredients)
        {
            if (oldFoodIngredient.IngredientId <= 0)
            {
                logger.LogWarning("Skipping invalid old FoodIngredient with ID: {IngredientId}", 
                    oldFoodIngredient.IngredientId);
                continue;
            }

            var ingredient = await ingredientRepository.GetByIdAsync(oldFoodIngredient.IngredientId);
            if (ingredient != null)
            {
                // Check for overflow
                if (ingredient.Quantity > decimal.MaxValue - oldFoodIngredient.Quantity)
                {
                    logger.LogWarning("Quantity overflow detected for ingredient {IngredientId}, setting to maximum",
                        ingredient.Id);
                    ingredient.Quantity = decimal.MaxValue;
                }
                else
                {
                    ingredient.Quantity += oldFoodIngredient.Quantity;
                }
                
                await ingredientRepository.UpdateAsync(ingredient);
                logger.LogInformation("✅ Restored {Quantity} {Unit} to ingredient {IngredientName} (ID: {Id})",
                    oldFoodIngredient.Quantity, oldFoodIngredient.Unit, ingredient.Name, ingredient.Id);
            }
            else
            {
                logger.LogWarning("⚠️ Ingredient with ID {IngredientId} not found, cannot restore quantity",
                    oldFoodIngredient.IngredientId);
            }
        }
        
        // Delete old FoodIngredients
        await foodIngredientRepository.DeleteAsync(fi => fi.FoodId == food.Id);
        logger.LogInformation("Old FoodIngredients deleted for Food ID: {FoodId}", food.Id);
        
        // Process new ingredients (similar to CreateFoodAsync)
        foreach (var ingredient in dto.Ingredients ?? [])
        {
            // FIX: Handle IngredientId = 0 (user doesn't have this ingredient from cache)
            if (ingredient.IngredientId == 0)
            {
                logger.LogWarning("⚠️ User doesn't have ingredient '{IngredientName}' (ID=0) - skipping. User needs to add this ingredient first.",
                    ingredient.IngredientName ?? "Unknown");
                continue;
            }

            if (ingredient.IngredientId <= 0 || ingredient.Quantity <= 0)
            {
                logger.LogWarning("Skipping invalid ingredient - ID: {IngredientId}, Quantity: {Quantity}, Name: {Name}",
                    ingredient?.IngredientId, ingredient?.Quantity, ingredient?.IngredientName);
                continue;
            }

            var existingIngredient = await ingredientRepository.GetByIdAsync(ingredient.IngredientId);
            if (existingIngredient == null)
            {
                logger.LogWarning("Ingredient not found with ID: {IngredientId}", ingredient.IngredientId);
                continue;
            }

            // Deduct new quantity
            if (existingIngredient.Quantity < ingredient.Quantity)
            {
                logger.LogWarning(
                    "Insufficient ingredient {IngredientName} - Available: {Available}, Required: {Required}. Setting to 0.",
                    existingIngredient.Name, existingIngredient.Quantity, ingredient.Quantity);
                existingIngredient.Quantity = 0;
            }
            else
            {
                existingIngredient.Quantity -= ingredient.Quantity;
            }

            await ingredientRepository.UpdateAsync(existingIngredient);
            
            // Create new FoodIngredient
            var foodIngredient = new FoodIngredient
            {
                FoodId = food.Id,
                IngredientId = ingredient.IngredientId,
                Quantity = ingredient.Quantity,
                Unit = ingredient.Unit, // FIX: Set Unit
                UserId = userContextService.GetAuthenticatedUserId()
            };
            await foodIngredientRepository.AddAsync(foodIngredient);
            
            // Store for response
            ingredientResponses.Add(new FoodIngredientDto
            {
                IngredientId = ingredient.IngredientId,
                Quantity = ingredient.Quantity,
                Unit = ingredient.Unit,
                IngredientName = existingIngredient.Name,
                RemainingQuantity = existingIngredient.Quantity
            });
        }

        // Update Meal nutrition: subtract old, add new
        if (oldMeal != null)
        {
            // Create temporary food with old values to subtract
            var oldFood = new Food
            {
                Calories = oldCalories,
                Protein = oldProtein,
                Carbohydrates = oldCarbs,
                Fat = oldFat,
                Fiber = oldFiber
            };
            await UpdateMealNutritionAsync(oldMeal, oldFood, isAdding: false);
            logger.LogInformation("Subtracted old nutrition from Meal {MealId}", oldMeal.Id);
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

        // Update new Meal nutrition (add new food values)
        await UpdateMealNutritionAsync(meal, food, isAdding: true);
        logger.LogInformation("Added new nutrition to Meal {MealId}", meal.Id);

        logger.LogInformation("=== FOOD UPDATE COMPLETED SUCCESSFULLY ===");

        // Reload food with relationships to get updated data
        var updatedFood = await foodRepository.GetByIdWithIngredientsAsync(food.Id);
        if (updatedFood == null)
        {
            throw new UnauthorizedAccessException("Food not found after update");
        }

        var response = updatedFood.ToDto();
        // Update ingredients with remaining quantities
        response.Ingredients = ingredientResponses;
        return response;
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

        // FIX: Load FoodIngredients from repository (more reliable than navigation property)
        var foodIngredients = await foodIngredientRepository.GetByFoodIdAsync(foodId);
        var ingredientsToRestore = foodIngredients.ToList();
        
        if (ingredientsToRestore.Any())
        {
            logger.LogInformation("Found {Count} ingredients to restore for food {FoodId}",
                ingredientsToRestore.Count, foodId);

            foreach (var foodIngredient in ingredientsToRestore)
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

        // Update Meal nutrition: subtract food nutrition before deleting
        foreach (var mealId in affectedMealIds)
        {
            try
            {
                var meal = await mealRepository.GetByIdAsync(mealId);
                if (meal != null)
                {
                    await UpdateMealNutritionAsync(meal, food, isAdding: false);
                    logger.LogInformation("Subtracted nutrition from Meal {MealId} before deleting food", mealId);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to update Meal nutrition for MealId {MealId}, continuing", mealId);
            }
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
        // FIX: Use GetByIdWithIngredientsAsync to ensure FoodIngredients and Ingredient are loaded
        var food = await foodRepository.GetByIdWithIngredientsAsync(id);
        if (food == null)
        {
            throw new UnauthorizedAccessException("Food not found or access denied.");
        }

        logger.LogInformation("Getting food {FoodId} with {IngredientCount} ingredients", 
            food.Id, food.FoodIngredients?.Count ?? 0);

        return food.ToDto();
    }
}