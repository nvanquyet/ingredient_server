using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using IngredientServer.Utils.DTOs.Ingredient;
using IngredientServer.Utils.Extension;

namespace IngredientServer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MealController(IMealRepository mealRepository) : ControllerBase
    {
        private readonly IMealRepository _mealRepository = mealRepository;

        // POST: api/meal
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateMeal([FromBody] CreateMealDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserIdFromContextOrDto(dto.UserId);
            if (userId <= 0)
                return Unauthorized("Invalid or missing UserId");

            var meal = new Meal
            {
                UserId = userId,
                MealType = EnumExtension.ConvertStringToMealType<MealType>(dto.Name),
                MealDate = dto.MealDate,
            };

            var createdMeal = await _mealRepository.AddAsync(meal);
            var response = MapToResponseDto(createdMeal);

            return CreatedAtAction(nameof(GetMeal),
                new { id = createdMeal.Id, userId }, response);
        }

        // PUT: api/meal/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateMeal(int id, [FromBody] UpdateMealDto dto, [FromQuery] int userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (userId <= 0)
                return Unauthorized("Invalid UserId");

            var existingMeal = await _mealRepository.GetByIdAsync(id, userId);
            if (existingMeal == null)
                return NotFound($"Meal with ID {id} not found for user {userId}");

            existingMeal.MealType = EnumExtension.ConvertStringToMealType<MealType>(dto.Name);
            existingMeal.MealDate = dto.MealDate;
            existingMeal.UpdatedAt = DateTime.UtcNow; // Cập nhật thời gian

            var updatedMeal = await _mealRepository.UpdateAsync(existingMeal);
            var response = MapToResponseDto(updatedMeal);
            return Ok(response);
        }

        // POST: api/meal/{mealId}/foods
        [HttpPost("{mealId}/foods")]
        [Authorize]
        public async Task<IActionResult> AddFoodToMeal(int mealId, [FromBody] AddFoodToMealDto dto, [FromQuery] int userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (userId <= 0)
                return Unauthorized("Invalid UserId");

            await _mealRepository.AddFoodToMealAsync(mealId, dto.FoodId, dto.PortionWeight, userId);
            return NoContent();
        }

        // DELETE: api/meal/{mealId}/foods/{foodId}
        [HttpDelete("{mealId}/foods/{foodId}")]
        [Authorize]
        public async Task<IActionResult> RemoveFoodFromMeal(int mealId, int foodId, [FromQuery] int userId)
        {
            if (userId <= 0)
                return Unauthorized("Invalid UserId");

            await _mealRepository.RemoveFoodFromMealAsync(mealId, foodId, userId);
            return NoContent();
        }

        // ... (các phương thức khác như GetMeal, GetRecentMeals giữ nguyên)
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetMeal(int id, [FromQuery] int userId)
        {
            if (userId <= 0)
                return Unauthorized("Invalid UserId");

            var meal = await _mealRepository.GetByIdAsync(id, userId);
            if (meal == null)
                return NotFound($"Meal with ID {id} not found for user {userId}");

            var response = MapToResponseDto(meal);
            return Ok(response);
        }
        
        private static MealResponseDto MapToResponseDto(Meal meal)
        {
            return new MealResponseDto
            {
                Id = meal.Id,
                Name = meal.MealType.ToString(),
                MealDate = meal.MealDate,
                UserId = meal.UserId,
                CreatedAt = meal.CreatedAt,
                UpdatedAt = meal.UpdatedAt
            };
        }

        private int GetUserIdFromContextOrDto(int? dtoUserId)
        {
            var userIdFromToken = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdFromToken, out int tokenUserId) && tokenUserId > 0)
            {
                if (dtoUserId.HasValue && dtoUserId != tokenUserId)
                    throw new UnauthorizedAccessException("UserId from DTO does not match authenticated user.");
                return tokenUserId;
            }

            if (dtoUserId.HasValue && dtoUserId > 0)
                return dtoUserId.Value;

            throw new UnauthorizedAccessException("User ID is not valid or not authenticated.");
        }
    }

    
}