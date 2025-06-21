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

            var userId = GetCurrentUserId();
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
                new { id = createdMeal.Id }, response);
        }

        // PUT: api/meal/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateMeal(int id, [FromBody] UpdateMealDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingMeal = await _mealRepository.GetByIdAsync(id);
            if (existingMeal == null)
                return NotFound($"Meal with ID {id} not found");

            existingMeal.MealType = EnumExtension.ConvertStringToMealType<MealType>(dto.Name);
            existingMeal.MealDate = dto.MealDate;
            existingMeal.UpdatedAt = DateTime.UtcNow;

            var updatedMeal = await _mealRepository.UpdateAsync(existingMeal);
            var response = MapToResponseDto(updatedMeal);
            return Ok(response);
        }

        // POST: api/meal/{mealId}/foods
        [HttpPost("{mealId}/foods")]
        [Authorize]
        public async Task<IActionResult> AddFoodToMeal(int mealId, [FromBody] AddFoodToMealDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                await _mealRepository.AddFoodToMealAsync(mealId, dto.FoodId, dto.PortionWeight);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/meal/{mealId}/foods/{foodId}
        [HttpDelete("{mealId}/foods/{foodId}")]
        [Authorize]
        public async Task<IActionResult> RemoveFoodFromMeal(int mealId, int foodId)
        {
            try
            {
                await _mealRepository.RemoveFoodFromMealAsync(mealId, foodId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/meal/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetMeal(int id)
        {
            var meal = await _mealRepository.GetByIdAsync(id);
            if (meal == null)
                return NotFound($"Meal with ID {id} not found");

            var response = MapToResponseDto(meal);
            return Ok(response);
        }

        // GET: api/meal
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllMeals([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var meals = await _mealRepository.GetAllAsync(pageNumber, pageSize);
            var response = meals.Select(MapToResponseDto).ToList();
            return Ok(response);
        }

        // GET: api/meal/recent
        [HttpGet("recent")]
        [Authorize]
        public async Task<IActionResult> GetRecentMeals([FromQuery] int days = 7, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var meals = await _mealRepository.GetRecentMealsAsync(days, pageNumber, pageSize);
            var response = meals.Select(MapToResponseDto).ToList();
            return Ok(response);
        }

        // GET: api/meal/{id}/details
        [HttpGet("{id}/details")]
        [Authorize]
        public async Task<IActionResult> GetMealDetails(int id)
        {
            var meal = await _mealRepository.GetMealDetailsAsync(id);
            if (meal == null)
                return NotFound($"Meal with ID {id} not found");

            var response = MapToResponseDto(meal);
            return Ok(response);
        }

        // DELETE: api/meal/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteMeal(int id)
        {
            var result = await _mealRepository.DeleteAsync(id);
            if (!result)
                return NotFound($"Meal with ID {id} not found");

            return NoContent();
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

        private int GetCurrentUserId()
        {
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId) && userId > 0)
            {
                return userId;
            }
            return 0;
        }
    }
}