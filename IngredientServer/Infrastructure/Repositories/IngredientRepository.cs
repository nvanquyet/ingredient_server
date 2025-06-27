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
    public async Task<IngredientSearchResultDto> GetByFilterAsync(IngredientFilterDto? filter = null)
    {
        var query = Context.Set<Ingredient>()
            .Where(e => e.UserId == AuthenticatedUserId);

        if (filter != null)
        {
            // Lọc theo Category
            if (filter.Category.HasValue)
            {
                query = query.Where(i => i.Category == filter.Category.Value);
            }

            // Lọc theo IsExpired
            if (filter.IsExpired.HasValue)
            {
                query = query.Where(i => i.IsExpired == filter.IsExpired.Value);
            }

            // Lọc theo IsExpiringSoon
            if (filter.IsExpiringSoon.HasValue)
            {
                query = query.Where(i => i.IsExpiringSoon == filter.IsExpiringSoon.Value);
            }

            // Lọc theo SearchTerm
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(i => i.Name.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Sắp xếp
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                query = filter.SortBy.ToLower() switch
                {
                    "name" => filter.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(i => i.Name)
                        : query.OrderBy(i => i.Name),
                    "quantity" => filter.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(i => i.Quantity)
                        : query.OrderBy(i => i.Quantity),
                    "expirydate" => filter.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(i => i.ExpiryDate)
                        : query.OrderBy(i => i.ExpiryDate),
                    "category" => filter.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(i => i.Category)
                        : query.OrderBy(i => i.Category),
                    "createdat" => filter.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(i => i.CreatedAt)
                        : query.OrderBy(i => i.CreatedAt),
                    _ => query.OrderBy(i => i.Id)
                };
            }
            else
            {
                query = query.OrderBy(i => i.Id); // Sắp xếp mặc định theo Id nếu không có SortBy
            }
        }
        else
        {
            query = query.OrderBy(i => i.Id); // Sắp xếp mặc định theo Id nếu filter là null
        }

        // Tính tổng số bản ghi
        var totalCount = await query.CountAsync();

        // Phân trang
        var pageNumber = filter?.PageNumber ?? 1;
        var pageSize = filter?.PageSize ?? 20;
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Trả về kết quả với phân trang
        return new IngredientSearchResultDto
        {
            Ingredients = items.Select(i => new IngredientDto
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
                IsExpiringSoon = (i.ExpiryDate - DateTime.UtcNow).Days <= 7,
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            PageSize = pageSize,
            HasNextPage = pageNumber < (int)Math.Ceiling((double)totalCount / pageSize),
            HasPreviousPage = pageNumber > 1
        };
    }
}