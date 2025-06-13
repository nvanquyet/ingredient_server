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

            var userId = GetUserIdFromContextOrDto(dto.UserId);
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
                new { id = createdFood.Id, userId }, response);
        }

        // PUT: api/food/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateFood(int id, [FromBody] UpdateFoodDto dto, [FromQuery] int userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (userId <= 0)
                return Unauthorized("Invalid UserId");

            var existingFood = await _foodRepository.GetByIdAsync(id, userId);
            if (existingFood == null)
                return NotFound($"Food with ID {id} not found for user {userId}");

            existingFood.Name = dto.Name;
            existingFood.Category = dto.Category;
            existingFood.Recipe = dto.Recipe;
            existingFood.PreparationTimeMinutes = dto.PreparationTimeMinutes;
            existingFood.UpdatedAt = DateTime.UtcNow; // Cập nhật thời gian

            var updatedFood = await _foodRepository.UpdateAsync(existingFood);
            var response = MapToResponseDto(updatedFood);
            return Ok(response);
        }

        // ... (các phương thức khác như GetFood, DeleteFood giữ nguyên)
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetFood(int id, [FromQuery] int userId)
        {
            if (userId <= 0)
                return Unauthorized("Invalid UserId");

            var food = await _foodRepository.GetByIdAsync(id, userId);
            if (food == null)
                return NotFound($"Food with ID {id} not found for user {userId}");

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