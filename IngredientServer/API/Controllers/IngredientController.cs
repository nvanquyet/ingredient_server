using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Utils.DTOs.Ingredient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace IngredientServer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientController(IIngredientRepository ingredientRepository) : ControllerBase
    {
        private readonly IIngredientRepository _ingredientRepository = ingredientRepository;

        // POST: api/ingredient
        [HttpPost]
        [Authorize] // Yêu cầu xác thực cho việc tạo ingredient
        public async Task<IActionResult> CreateIngredient([FromBody] CreateIngredientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserIdFromContextOrDto(dto.UserId); // Lấy userId từ token hoặc DTO
            if (userId <= 0)
                return Unauthorized("Invalid or missing UserId");

            var ingredient = new Ingredient
            {
                UserId = userId,
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                Quantity = dto.Quantity,
                Unit = dto.Unit,
                ExpiryDate = dto.ExpiryDate,
                ImageUrl = dto.ImageUrl
            };

            var createdIngredient = await _ingredientRepository.AddAsync(ingredient);
            var response = MapToResponseDto(createdIngredient);

            return CreatedAtAction(nameof(GetIngredient),
                new { id = createdIngredient.Id, userId }, response);
        }

        // GET: api/ingredient/{id}
        [HttpGet("{id}")]
        [Authorize] // Yêu cầu xác thực để lấy ingredient
        public async Task<IActionResult> GetIngredient(int id, [FromQuery] int userId)
        {
            if (userId <= 0)
                return Unauthorized("Invalid UserId");

            var ingredient = await _ingredientRepository.GetByIdAsync(id, userId);
            if (ingredient == null)
                return NotFound($"Ingredient with ID {id} not found for user {userId}");

            var response = MapToResponseDto(ingredient);
            return Ok(response);
        }

        // GET: api/ingredient/user
        [HttpGet("user")]
        [Authorize] // Yêu cầu xác thực để lấy danh sách
        public async Task<IActionResult> GetUserIngredients([FromQuery] int userId)
        {
            if (userId <= 0)
                return Unauthorized("Invalid UserId");

            var ingredients = await _ingredientRepository.GetByUserIdAsync(userId);
            var response = ingredients.Select(MapToResponseDto).ToList();
            return Ok(response);
        }

        // PUT: api/ingredient/{id}
        [HttpPut("{id}")]
        [Authorize] // Yêu cầu xác thực để cập nhật
        public async Task<IActionResult> UpdateIngredient(int id, [FromBody] UpdateIngredientDto dto, [FromQuery] int userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (userId <= 0)
                return Unauthorized("Invalid UserId");

            var existingIngredient = await _ingredientRepository.GetByIdAsync(id, userId);
            if (existingIngredient == null)
                return NotFound($"Ingredient with ID {id} not found for user {userId}");

            existingIngredient.Name = dto.Name;
            existingIngredient.Description = dto.Description;
            existingIngredient.Category = dto.Category;
            existingIngredient.Quantity = dto.Quantity;
            existingIngredient.Unit = dto.Unit;
            existingIngredient.ExpiryDate = dto.ExpiryDate;
            existingIngredient.ImageUrl = dto.ImageUrl;

            var updatedIngredient = await _ingredientRepository.UpdateAsync(existingIngredient);
            var response = MapToResponseDto(updatedIngredient);
            return Ok(response);
        }

        // DELETE: api/ingredient/{id}
        [HttpDelete("{id}")]
        [Authorize] // Yêu cầu xác thực để xóa
        public async Task<IActionResult> DeleteIngredient(int id, [FromQuery] int userId)
        {
            if (userId <= 0)
                return Unauthorized("Invalid UserId");

            var success = await _ingredientRepository.DeleteAsync(id, userId);
            if (!success)
                return NotFound($"Ingredient with ID {id} not found for user {userId}");

            return NoContent();
        }

        // GET: api/ingredient/user/expiring?days=7
        [HttpGet("user/expiring")]
        [Authorize] // Yêu cầu xác thực
        public async Task<IActionResult> GetExpiringItems([FromQuery] int userId, [FromQuery] int days = 7)
        {
            if (userId <= 0)
                return Unauthorized("Invalid UserId");
            if (days < 0)
                return BadRequest("Days parameter must be non-negative");

            var items = await _ingredientRepository.GetExpiringItemsAsync(userId, days);
            var response = items.Select(MapToResponseDto).ToList();
            return Ok(response);
        }

        // GET: api/ingredient/user/expired
        [HttpGet("user/expired")]
        [Authorize] // Yêu cầu xác thực
        public async Task<IActionResult> GetExpiredItems([FromQuery] int userId)
        {
            if (userId <= 0)
                return Unauthorized("Invalid UserId");

            var items = await _ingredientRepository.GetExpiredItemsAsync(userId);
            var response = items.Select(MapToResponseDto).ToList();
            return Ok(response);
        }

        // GET: api/ingredient/user/sorted?sortBy=name&sortOrder=asc
        [HttpGet("user/sorted")]
        [Authorize] // Yêu cầu xác thực
        public async Task<IActionResult> GetSortedItems(
            [FromQuery] int userId,
            [FromQuery] string sortBy = "name",
            [FromQuery] string sortOrder = "asc")
        {
            if (userId <= 0)
                return Unauthorized("Invalid UserId");

            var validSortFields = new[] { "name", "expirydate", "quantity", "createdat", "category" };
            var validSortOrders = new[] { "asc", "desc" };

            if (!validSortFields.Contains(sortBy.ToLower()))
                return BadRequest($"Invalid sortBy field. Valid values: {string.Join(", ", validSortFields)}");

            if (!validSortOrders.Contains(sortOrder.ToLower()))
                return BadRequest($"Invalid sortOrder. Valid values: {string.Join(", ", validSortOrders)}");

            var sortDto = new IngredientSortDto { SortBy = sortBy, SortOrder = sortOrder };
            var items = await _ingredientRepository.GetSortedAsync(userId, sortDto);
            var response = items.Select(MapToResponseDto).ToList();
            return Ok(response);
        }

        // GET: api/ingredient/user/category/{category}
        [HttpGet("user/category/{category}")]
        [Authorize] // Yêu cầu xác thực
        public async Task<IActionResult> GetByCategory([FromQuery] int userId, IngredientCategory category)
        {
            if (userId <= 0)
                return Unauthorized("Invalid UserId");

            var items = await _ingredientRepository.GetByCategoryAsync(userId, category);
            var response = items.Select(MapToResponseDto).ToList();
            return Ok(response);
        }

        // POST: api/ingredient/filter
        [HttpPost("filter")]
        [Authorize] // Yêu cầu xác thực
        public async Task<IActionResult> GetFilteredItems([FromBody] IngredientFilterDto filter)
        {
            if (filter == null)
                return BadRequest("Filter data is required");

            if (filter.UserId <= 0)
                return Unauthorized("Invalid UserId");

            var items = await _ingredientRepository.GetFilteredAsync(filter);
            var response = items.Select(MapToResponseDto).ToList();
            return Ok(response);
        }

        // GET: api/ingredient/user/count
        [HttpGet("user/count")]
        [Authorize] // Yêu cầu xác thực
        public async Task<IActionResult> GetUserIngredientCount([FromQuery] int userId)
        {
            if (userId <= 0)
                return Unauthorized("Invalid UserId");

            var count = await _ingredientRepository.CountByUserIdAsync(userId);
            return Ok(new { Count = count });
        }

        // GET: api/ingredient/categories (Endpoint công khai)
        [HttpGet("categories")]
        [AllowAnonymous] // Cho phép truy cập mà không cần xác thực
        public IActionResult GetCategories()
        {
            var categories = Enum.GetValues<IngredientCategory>()
                .Select(c => new { Value = (int)c, Name = c.ToString() })
                .ToList();
            return Ok(categories);
        }

        // GET: api/ingredient/units (Endpoint công khai)
        [HttpGet("units")]
        [AllowAnonymous] // Cho phép truy cập mà không cần xác thực
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
                DaysUntilExpiry = (int) (ingredient.ExpiryDate.Date - DateTime.UtcNow).TotalDays,
                IsExpired = ingredient.ExpiryDate.Date < DateTime.UtcNow,
                IsExpiringSoon = ingredient.ExpiryDate.Date <= DateTime.UtcNow.AddDays(7) && ingredient.ExpiryDate.Date > DateTime.UtcNow
            };
        }

        private int GetUserIdFromContextOrDto(int? dtoUserId)
        {
            // Nếu có token và xác thực, ưu tiên lấy từ context
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userIdFromToken) && userIdFromToken > 0)
                    return userIdFromToken;
            }

            // Nếu không có token hoặc không xác thực, sử dụng userId từ DTO (nếu có)
            if (dtoUserId.HasValue && dtoUserId > 0)
                return dtoUserId.Value;

            throw new UnauthorizedAccessException("User ID is not valid or not authenticated.");
        }
    }
}