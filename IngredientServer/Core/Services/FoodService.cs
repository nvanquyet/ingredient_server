using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs.Ingredient;

namespace IngredientServer.Core.Services;

public class FoodService(
    IFoodRepository foodRepository,
    IIngredientRepository ingredientRepository,
    IMealRepository mealRepository,
    IFoodIngredientRepository foodIngredientRepository,
    IMealFoodRepository mealFoodRepository,
    IUserContextService userContextService)
    : IFoodService
{
    public async Task<Food> CreateFoodAsync(CreateFoodDto dto)
    {
        // Map DTO to entity
        var food = new Food
        {
            Name = dto.Name,
            Description = dto.Description,
            Quantity = dto.Quantity,
            Calories = dto.Calories,
            Protein = dto.Protein,
            Carbs = dto.Carbs,
            Fat = dto.Fat,
            UserId = userContextService.GetAuthenticatedUserId(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Check ingredient availability
        foreach (var ingredientDto in dto.Ingredients)
        {
            var ingredient = await ingredientRepository.GetByIdAsync(ingredientDto.IngredientId);
            if (ingredient == null || ingredient.UserId != userContextService.GetAuthenticatedUserId())
            {
                throw new KeyNotFoundException($"Ingredient {ingredientDto.IngredientId} not found or access denied.");
            }
            if (ingredient.Quantity < ingredientDto.Quantity)
            {
                throw new InvalidOperationException($"Insufficient quantity for ingredient {ingredient.Name}.");
            }
        }

        // Deduct ingredients
        foreach (var ingredientDto in dto.Ingredients)
        {
            var ingredient = await ingredientRepository.GetByIdAsync(ingredientDto.IngredientId);
            if (ingredient != null)
            {
                ingredient.Quantity -= ingredientDto.Quantity;
                ingredient.UpdatedAt = DateTime.UtcNow;
                await ingredientRepository.UpdateAsync(ingredient);
            }

            // Create FoodIngredient link
            var foodIngredient = new FoodIngredient
            {
                Food = food,
                IngredientId = ingredientDto.IngredientId,
                Quantity = ingredientDto.Quantity,
                Unit = ingredientDto.Unit,
                UserId = userContextService.GetAuthenticatedUserId()
            };
            await foodIngredientRepository.AddAsync(foodIngredient);
        }

        var meal = (await mealRepository.GetByDateAsync(dto.Date.ToString("yyyy-MM-dd"), 1, int.MaxValue))
            .FirstOrDefault(m => m.MealType == dto.MealType && m.UserId == userContextService.GetAuthenticatedUserId());
        
        if (meal == null)
        {
            meal = new Meal
            {
                MealType = dto.MealType,
                MealDate = dto.Date,
                UserId = userContextService.GetAuthenticatedUserId(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            meal = await mealRepository.AddAsync(meal);
        }

        // Save Food
        food = await foodRepository.AddAsync(food);

        // Create MealFood link
        var mealFood = new MealFood
        {
            MealId = meal.Id,
            FoodId = food.Id,
            UserId = userContextService.GetAuthenticatedUserId()
        };
        await mealFoodRepository.AddAsync(mealFood);

        // Update Meal nutrition
        meal.TotalCalories += food.Calories;
        meal.TotalProtein += food.Protein;
        meal.TotalCarbs += food.Carbs;
        meal.TotalFat += food.Fat;
        meal.UpdatedAt = DateTime.UtcNow;
        await mealRepository.UpdateAsync(meal);

        return food;
    }

    public async Task<Food> UpdateFoodAsync(int foodId, UpdateFoodDto dto)
    {
        var food = await foodRepository.GetByIdAsync(foodId);
        if (food == null)
        {
            throw new KeyNotFoundException("Food not found or access denied.");
        }

        // Get current FoodIngredients
        var oldFoodIngredients = await foodIngredientRepository.GetByFoodIdAsync(foodId);

        // Restore old ingredients to inventory
        foreach (var oldFoodIngredient in oldFoodIngredients)
        {
            var ingredient = await ingredientRepository.GetByIdAsync(oldFoodIngredient.IngredientId);
            if (ingredient != null)
            {
                ingredient.Quantity += oldFoodIngredient.Quantity;
                ingredient.UpdatedAt = DateTime.UtcNow;
                await ingredientRepository.UpdateAsync(ingredient);
            }

            await foodIngredientRepository.DeleteAsync(oldFoodIngredient.Id);
        }

        // Update Food properties
        if (dto.Name != null) food.Name = dto.Name;
        if (dto.Description != null) food.Description = dto.Description;
        if (dto.Quantity.HasValue) food.Quantity = dto.Quantity.Value;
        if (dto.Calories.HasValue) food.Calories = dto.Calories.Value;
        if (dto.Protein.HasValue) food.Protein = dto.Protein.Value;
        if (dto.Carbs.HasValue) food.Carbs = dto.Carbs.Value;
        if (dto.Fat.HasValue) food.Fat = dto.Fat.Value;
        food.UpdatedAt = DateTime.UtcNow;

        // Check and deduct new ingredients
        if (dto.Ingredients != null)
        {
            foreach (var ingredientDto in dto.Ingredients)
            {
                var ingredient = await ingredientRepository.GetByIdAsync(ingredientDto.IngredientId);
                if (ingredient == null || ingredient.UserId != userContextService.GetAuthenticatedUserId())
                {
                    throw new KeyNotFoundException($"Ingredient {ingredientDto.IngredientId} not found or access denied.");
                }
                if (ingredient.Quantity < ingredientDto.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient quantity for ingredient {ingredient.Name}.");
                }

                ingredient.Quantity -= ingredientDto.Quantity;
                ingredient.UpdatedAt = DateTime.UtcNow;
                await ingredientRepository.UpdateAsync(ingredient);

                var foodIngredient = new FoodIngredient
                {
                    FoodId = food.Id,
                    IngredientId = ingredientDto.IngredientId,
                    Quantity = ingredientDto.Quantity,
                    Unit = ingredientDto.Unit,
                    UserId = userContextService.GetAuthenticatedUserId()
                };
                await foodIngredientRepository.AddAsync(foodIngredient);
            }
        }

        // Handle Meal change if date or mealType changed
        if (dto.Date.HasValue || dto.MealType.HasValue)
        {
            var mealFood = (await mealFoodRepository.GetByMealIdAsync(food.MealFoods.First().MealId))
                .FirstOrDefault(mf => mf.FoodId == foodId);
            if (mealFood != null)
            {
                var oldMeal = await mealRepository.GetByIdAsync(mealFood.MealId);
                if (oldMeal != null)
                {
                    oldMeal.TotalCalories -= food.Calories;
                    oldMeal.TotalProtein -= food.Protein;
                    oldMeal.TotalCarbs -= food.Carbs;
                    oldMeal.TotalFat -= food.Fat;
                    oldMeal.UpdatedAt = DateTime.UtcNow;
                    await mealRepository.UpdateAsync(oldMeal);
                }

                await mealFoodRepository.DeleteAsync(mealFood.Id);
            }

            var newMealDate = dto.Date ?? food.MealFoods.First().Meal.MealDate;
            var newMealType = dto.MealType ?? food.MealFoods.First().Meal.MealType;
            var newMeal = (await mealRepository.GetByDateAsync(newMealDate.ToString("yyyy-MM-dd"), 1, int.MaxValue))
                .FirstOrDefault(m => m.MealType == newMealType && m.UserId == userContextService.GetAuthenticatedUserId());

            if (newMeal == null)
            {
                newMeal = new Meal
                {
                    MealType = newMealType,
                    MealDate = newMealDate,
                    UserId = userContextService.GetAuthenticatedUserId(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                newMeal = await mealRepository.AddAsync(newMeal);
            }

            var newMealFood = new MealFood
            {
                MealId = newMeal.Id,
                FoodId = food.Id,
                UserId = userContextService.GetAuthenticatedUserId()
            };
            await mealFoodRepository.AddAsync(newMealFood);

            newMeal.TotalCalories += food.Calories;
            newMeal.TotalProtein += food.Protein;
            newMeal.TotalCarbs += food.Carbs;
            newMeal.TotalFat += food.Fat;
            newMeal.UpdatedAt = DateTime.UtcNow;
            await mealRepository.UpdateAsync(newMeal);
        }

        return await foodRepository.UpdateAsync(food);
    }

    public async Task<bool> DeleteFoodAsync(int foodId)
    {
        var food = await foodRepository.GetByIdAsync(foodId);
        if (food == null)
        {
            throw new KeyNotFoundException("Food not found or access denied.");
        }

        // Restore ingredients to inventory
        var foodIngredients = await foodIngredientRepository.GetByFoodIdAsync(foodId);
        foreach (var foodIngredient in foodIngredients)
        {
            var ingredient = await ingredientRepository.GetByIdAsync(foodIngredient.IngredientId);
            if (ingredient != null)
            {
                ingredient.Quantity += foodIngredient.Quantity;
                ingredient.UpdatedAt = DateTime.UtcNow;
                await ingredientRepository.UpdateAsync(ingredient);
            }

            await foodIngredientRepository.DeleteAsync(foodIngredient.Id);
        }

        // Update Meal nutrition
        var mealFood = (await mealFoodRepository.GetByMealIdAsync(food.MealFoods.First().MealId))
            .FirstOrDefault(mf => mf.FoodId == foodId);
        if (mealFood == null) return await foodRepository.DeleteAsync(foodId);
        var meal = await mealRepository.GetByIdAsync(mealFood.MealId);
        if (meal != null)
        {
            meal.TotalCalories -= food.Calories;
            meal.TotalProtein -= food.Protein;
            meal.TotalCarbs -= food.Carbs;
            meal.TotalFat -= food.Fat;
            meal.UpdatedAt = DateTime.UtcNow;
            await mealRepository.UpdateAsync(meal);
        }

        await mealFoodRepository.DeleteAsync(mealFood.Id);

        return await foodRepository.DeleteAsync(foodId);
    }
}