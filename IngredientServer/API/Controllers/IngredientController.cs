using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Utils.DTOs.Ingredient;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace IngredientServer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientController(IIngredientRepository ingredientRepository) : BaseController
    {
        // POST: api/ingredient
        [HttpPost]
        public async Task<IActionResult> CreateIngredient([FromBody] CreateIngredientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ingredient = new Ingredient
            {
                UserId = dto.UserId,
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                Quantity = dto.Quantity,
                Unit = dto.Unit,
                ExpiryDate = dto.ExpiryDate,
                ImageUrl = dto.ImageUrl
            };

            var createdIngredient = await ingredientRepository.AddAsync(ingredient);
            var response = MapToResponseDto(createdIngredient);

            return CreatedAtAction(nameof(GetIngredient),
                new { id = createdIngredient.Id }, response);
        }

        // GET: api/ingredient/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetIngredient(int id)
        {
            var ingredient = await ingredientRepository.GetByIdAsync(id);
            if (ingredient == null)
                return NotFound($"Ingredient with ID {id} not found");

            var response = MapToResponseDto(ingredient);
            return Ok(response);
        }

        // GET: api/ingredient/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserIngredients(int userId)
        {
            var ingredients = await ingredientRepository.GetByUserIdAsync(userId);
            var response = ingredients.Select(MapToResponseDto).ToList();
            return Ok(response);
        }

        // PUT: api/ingredient/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIngredient(int id, [FromBody] UpdateIngredientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingIngredient = await ingredientRepository.GetByIdAsync(id);
            if (existingIngredient == null)
                return NotFound($"Ingredient with ID {id} not found");

            existingIngredient.Name = dto.Name;
            existingIngredient.Description = dto.Description;
            existingIngredient.Category = dto.Category;
            existingIngredient.Quantity = dto.Quantity;
            existingIngredient.Unit = dto.Unit;
            existingIngredient.ExpiryDate = dto.ExpiryDate;
            existingIngredient.ImageUrl = dto.ImageUrl;

            var updatedIngredient = await ingredientRepository.UpdateAsync(existingIngredient);
            if (updatedIngredient == null)
                return BadRequest("Failed to update ingredient");

            var response = MapToResponseDto(updatedIngredient);
            return Ok(response);
        }

        // DELETE: api/ingredient/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIngredient(int id)
        {
            var success = await ingredientRepository.DeleteAsync(id);
            if (!success)
                return NotFound($"Ingredient with ID {id} not found");

            return NoContent();
        }

        // GET: api/ingredient/user/{userId}/expiring?days=7
        [HttpGet("user/{userId}/expiring")]
        public async Task<IActionResult> GetExpiringItems(
            int userId,
            [FromQuery] int days = 7)
        {
            if (days < 0)
                return BadRequest("Days parameter must be non-negative");

            var items = await ingredientRepository.GetExpiringItemsAsync(userId, days);
            var response = items.Select(MapToResponseDto).ToList();
            return Ok(response);
        }

        // GET: api/ingredient/user/{userId}/expired
        [HttpGet("user/{userId}/expired")]
        public async Task<IActionResult> GetExpiredItems(int userId)
        {
            var items = await ingredientRepository.GetExpiredItemsAsync(userId);
            var response = items.Select(MapToResponseDto).ToList();
            return Ok(response);
        }

        // GET: api/ingredient/user/{userId}/sorted?sortBy=name&sortOrder=asc
        [HttpGet("user/{userId}/sorted")]
        public async Task<IActionResult> GetSortedItems(
            int userId,
            [FromQuery] string sortBy = "name",
            [FromQuery] string sortOrder = "asc")
        {
            var validSortFields = new[] { "name", "expirydate", "quantity", "createdat", "category" };
            var validSortOrders = new[] { "asc", "desc" };

            if (!validSortFields.Contains(sortBy.ToLower()))
                return BadRequest($"Invalid sortBy field. Valid values: {string.Join(", ", validSortFields)}");

            if (!validSortOrders.Contains(sortOrder.ToLower()))
                return BadRequest($"Invalid sortOrder. Valid values: {string.Join(", ", validSortOrders)}");

            var sortDto = new IngredientSortDto { SortBy = sortBy, SortOrder = sortOrder };
            var items = await ingredientRepository.GetSortedAsync(userId, sortDto);
            var response = items.Select(MapToResponseDto).ToList();
            return Ok(response);
        }

        // GET: api/ingredient/user/{userId}/category/{category}
        [HttpGet("user/{userId}/category/{category}")]
        public async Task<IActionResult> GetByCategory(int userId, IngredientCategory category)
        {
            var items = await ingredientRepository.GetByCategoryAsync(userId, category);
            var response = items.Select(MapToResponseDto).ToList();
            return Ok(response);
        }

        // POST: api/ingredient/filter
        [HttpPost("filter")]
        public async Task<IActionResult> GetFilteredItems([FromBody] IngredientFilterDto filter)
        {
            var items = await ingredientRepository.GetFilteredAsync(filter);
            var response = items.Select(MapToResponseDto).ToList();
            return Ok(response);
        }

        // GET: api/ingredient/user/{userId}/count
        [HttpGet("user/{userId}/count")]
        public async Task<IActionResult> GetUserIngredientCount(int userId)
        {
            var count = await ingredientRepository.CountByUserIdAsync(userId);
            return Ok(new { Count = count });
        }

        // GET: api/ingredient/categories
        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            var categories = Enum.GetValues<IngredientCategory>()
                .Select(c => new { Value = (int)c, Name = c.ToString() })
                .ToList();
            return Ok(categories);
        }

        // GET: api/ingredient/units
        [HttpGet("units")]
        public IActionResult GetUnits()
        {
            var units = Enum.GetValues<IngredientUnit>()
                .Select(u => new { Value = (int)u, Name = u.ToString() })
                .ToList();
            return Ok(units);
        }

        private static IngredientResponseDto MapToResponseDto(Ingredient ingredient)
        {
            return new IngredientResponseDto
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Description = ingredient.Description,
                UserId = ingredient.UserId,
                Quantity = ingredient.Quantity,
                Unit = ingredient.Unit.ToString(),
                Category = ingredient.Category.ToString(),
                ExpiryDate = ingredient.ExpiryDate,
                ImageUrl = ingredient.ImageUrl,
                CreatedAt = ingredient.CreatedAt,
                UpdatedAt = ingredient.UpdatedAt,
                DaysUntilExpiry = ingredient.DaysUntilExpiry,
                IsExpired = ingredient.IsExpired,
                IsExpiringSoon = ingredient.IsExpiringSoon
            };
        }
    }
}