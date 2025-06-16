using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IngredientServer.Utils.DTOs.Ingredient;

namespace IngredientServer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodController(IFoodRepository foodRepository) : ControllerBase
    {
        private readonly IFoodRepository _foodRepository = foodRepository;

        // POST: api/food
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateFood([FromBody] CreateFoodDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId <= 0)
                return Unauthorized("Invalid or missing UserId");

            var food = new Food
            {
                UserId = userId,
                Name = dto.Name,
                Category = dto.Category,
                Recipe = dto.Recipe,
                PreparationTimeMinutes = dto.PreparationTimeMinutes,
            };

            var createdFood = await _foodRepository.AddAsync(food);
            var response = MapToResponseDto(createdFood);

            return CreatedAtAction(nameof(GetFood),
                new { id = createdFood.Id }, response);
        }

        // PUT: api/food/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateFood(int id, [FromBody] UpdateFoodDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var existingFood = await _foodRepository.GetByIdAsync(id);
            if (existingFood == null)
                return NotFound($"Food with ID {id} not found");

            existingFood.Name = dto.Name;
            existingFood.Category = dto.Category;
            existingFood.Recipe = dto.Recipe;
            existingFood.PreparationTimeMinutes = dto.PreparationTimeMinutes;
            existingFood.UpdatedAt = DateTime.UtcNow;

            var updatedFood = await _foodRepository.UpdateAsync(existingFood);
            var response = MapToResponseDto(updatedFood);
            return Ok(response);
        }

        // GET: api/food/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetFood(int id)
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
                return Unauthorized("Invalid UserId");

            var food = await _foodRepository.GetByIdAsync(id);
            if (food == null)
                return NotFound($"Food with ID {id} not found");

            var response = MapToResponseDto(food);
            return Ok(response);
        }

        // GET: api/food
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllFoods([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var foods = await _foodRepository.GetAllAsync(pageNumber, pageSize);
            var response = foods.Select(MapToResponseDto).ToList();
            return Ok(response);
        }

        // DELETE: api/food/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteFood(int id)
        {
            var result = await _foodRepository.DeleteAsync(id);
            if (!result)
                return NotFound($"Food with ID {id} not found");

            return NoContent();
        }

        // GET: api/food/{id}/details
        [HttpGet("{id}/details")]
        [Authorize]
        public async Task<IActionResult> GetFoodDetails(int id)
        {
            var food = await _foodRepository.GetFoodDetailsAsync(id);
            if (food == null)
                return NotFound($"Food with ID {id} not found");

            var response = MapToResponseDto(food);
            return Ok(response);
        }
        
        private static FoodResponseDto MapToResponseDto(Food food)
        {
            return new FoodResponseDto
            {
                Id = food.Id,
                Name = food.Name,
                Category = food.Category.ToString(),
                Recipe = food.Recipe,
                UserId = food.UserId,
                CreatedAt = food.CreatedAt,
                UpdatedAt = food.UpdatedAt
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