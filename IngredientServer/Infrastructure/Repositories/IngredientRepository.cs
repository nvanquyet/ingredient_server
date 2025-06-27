using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Infrastructure.Data;
using IngredientServer.Utils.DTOs.Entity;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public class IngredientRepository(ApplicationDbContext context, IUserContextService userContextService)
    : BaseRepository<Ingredient>(context, userContextService), IIngredientRepository
{
    // Override GetAllAsync to support filtering and pagination
    public async Task<IngredientSearchResultDto> GetByFilterAsync(IngredientFilterDto? filter = null)
    {
        var query = Context.Set<Ingredient>()
            .Where(e => e.UserId == AuthenticatedUserId);

        if (filter != null)
        {
            if (filter.Category.HasValue && Enum.IsDefined(typeof(IngredientCategory), filter.Category.Value))
                query = query.Where(i => i.Category == filter.Category.Value);

            if (filter.Unit.HasValue && Enum.IsDefined(typeof(IngredientUnit), filter.Unit.Value))
                query = query.Where(i => i.Unit == filter.Unit.Value);

            if (filter.IsExpired.HasValue)
                query = query.Where(i => i.ExpiryDate < DateTime.UtcNow == filter.IsExpired.Value);

            if (filter.IsExpiringSoon.HasValue)
                query = query.Where(i => (i.ExpiryDate - DateTime.UtcNow).Days <= 7 == filter.IsExpiringSoon.Value);

            if (filter.IsLowStock.HasValue)
                query = query.Where(i => i.Quantity <= 10);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
                query = query.Where(i => i.Name.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase));

            if (filter.ExpiryDateFrom.HasValue)
                query = query.Where(i => i.ExpiryDate >= filter.ExpiryDateFrom.Value);

            if (filter.ExpiryDateTo.HasValue)
                query = query.Where(i => i.ExpiryDate <= filter.ExpiryDateTo.Value);

            if (filter.MinQuantity.HasValue)
                query = query.Where(i => i.Quantity >= filter.MinQuantity.Value);

            if (filter.MaxQuantity.HasValue)
                query = query.Where(i => i.Quantity <= filter.MaxQuantity.Value);

            // Tính tổng số bản inhabghi trước khi phân trang
            var totalCount = await query.CountAsync();

            // Áp dụng phân trang
            query = query.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);

            // Áp dụng sắp xếp
            var validSortFields = new[] { "name", "quantity", "expirydate", "category", "createdat" };
            var sortBy = validSortFields.Contains(filter.SortBy?.ToLower()) ? filter.SortBy.ToLower() : "name";
            var sortDirection = filter.SortDirection?.ToLower() == "desc" ? "desc" : "asc";

            query = sortBy switch
            {
                "name" => sortDirection == "desc" ? query.OrderByDescending(i => i.Name) : query.OrderBy(i => i.Name),
                "quantity" => sortDirection == "desc"
                    ? query.OrderByDescending(i => i.Quantity)
                    : query.OrderBy(i => i.Quantity),
                "expirydate" => sortDirection == "desc"
                    ? query.OrderByDescending(i => i.ExpiryDate)
                    : query.OrderBy(i => i.ExpiryDate),
                "category" => sortDirection == "desc"
                    ? query.OrderByDescending(i => i.Category)
                    : query.OrderBy(i => i.Category),
                "createdat" => sortDirection == "desc"
                    ? query.OrderByDescending(i => i.CreatedAt)
                    : query.OrderBy(i => i.CreatedAt),
                _ => query.OrderBy(i => i.Name)
            };

            var ingredients = await query.ToListAsync();
            return new IngredientSearchResultDto
            {
                Ingredients = ingredients.Select(i => new IngredientDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    ImageUrl = i.ImageUrl,
                    Unit = i.Unit,
                    Category = i.Category,
                    Quantity = i.Quantity,
                    ExpiryDate = i.ExpiryDate,
                    DaysUntilExpiry = (i.ExpiryDate - DateTime.UtcNow).Days,
                    IsExpired = i.ExpiryDate < DateTime.UtcNow,
                    IsExpiringSoon = (i.ExpiryDate - DateTime.UtcNow).Days <= 7
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
            };
        }

        var allIngredients = await query.ToListAsync();
        return new IngredientSearchResultDto
        {
            Ingredients = allIngredients.Select(i => new IngredientDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                ImageUrl = i.ImageUrl,
                Unit = i.Unit,
                Category = i.Category,
                Quantity = i.Quantity,
                ExpiryDate = i.ExpiryDate,
                DaysUntilExpiry = (i.ExpiryDate - DateTime.UtcNow).Days,
                IsExpired = i.ExpiryDate < DateTime.UtcNow,
                IsExpiringSoon = (i.ExpiryDate - DateTime.UtcNow).Days <= 7
            }).ToList(),
            TotalCount = allIngredients.Count,
            PageNumber = 1,
            TotalPages = 1
        };
    }
}