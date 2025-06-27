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
    public async Task<IEnumerable<Ingredient>> GetByFilterAsync(IngredientFilterDto? filter = null)
    {
        var query = Context.Set<Ingredient>()
            .Where(e => e.UserId == AuthenticatedUserId);

        if (filter != null)
        {
            if (filter.Category.HasValue)
                query = query.Where(i => i.Category == filter.Category.Value);

            if (filter.Unit.HasValue)
                query = query.Where(i => i.Unit == filter.Unit.Value);

            if (filter.IsExpired.HasValue)
                query = query.Where(i => i.IsExpired == filter.IsExpired.Value);

            if (filter.IsExpiringSoon.HasValue)
                query = query.Where(i => i.IsExpiringSoon == filter.IsExpiringSoon.Value);

            if (filter.IsLowStock.HasValue)
                query = query.Where(i => i.Quantity <= 10); 

            if (!string.IsNullOrEmpty(filter.SearchTerm))
                query = query.Where(i => i.Name.Contains(filter.SearchTerm));

            if (filter.ExpiryDateFrom.HasValue)
                query = query.Where(i => i.ExpiryDate >= filter.ExpiryDateFrom.Value);

            if (filter.ExpiryDateTo.HasValue)
                query = query.Where(i => i.ExpiryDate <= filter.ExpiryDateTo.Value);

            if (filter.MinQuantity.HasValue)
                query = query.Where(i => i.Quantity >= filter.MinQuantity.Value);

            if (filter.MaxQuantity.HasValue)
                query = query.Where(i => i.Quantity <= filter.MaxQuantity.Value);

            if (string.IsNullOrEmpty(filter.SortBy)) return await query.ToListAsync();
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
        }

        return await query.ToListAsync();
    }
}
