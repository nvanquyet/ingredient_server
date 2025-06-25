using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Core.Services;
using IngredientServer.Infrastructure.Data;
using IngredientServer.Utils.DTOs.Ingredient;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public class IngredientRepository(ApplicationDbContext context, IUserContextService userContextService)
    : BaseRepository<Ingredient>(context, userContextService), IIngredientRepository
{
    // Override GetAllAsync to support filtering and pagination
    public async Task<IEnumerable<Ingredient>> GetAllAsync(int pageNumber = 1, int pageSize = 10, IngredientFilterDto? filter = null)
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
                query = query.Where(i => i.Quantity <= 10); // Example threshold for low stock

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

            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                switch (filter.SortBy.ToLower())
                {
                    case "name":
                        query = filter.SortDirection?.ToLower() == "desc" 
                            ? query.OrderByDescending(i => i.Name) 
                            : query.OrderBy(i => i.Name);
                        break;
                    case "quantity":
                        query = filter.SortDirection?.ToLower() == "desc" 
                            ? query.OrderByDescending(i => i.Quantity) 
                            : query.OrderBy(i => i.Quantity);
                        break;
                    case "expirydate":
                        query = filter.SortDirection?.ToLower() == "desc" 
                            ? query.OrderByDescending(i => i.ExpiryDate) 
                            : query.OrderBy(i => i.ExpiryDate);
                        break;
                    case "category":
                        query = filter.SortDirection?.ToLower() == "desc" 
                            ? query.OrderByDescending(i => i.Category) 
                            : query.OrderBy(i => i.Category);
                        break;
                    case "createdat":
                        query = filter.SortDirection?.ToLower() == "desc" 
                            ? query.OrderByDescending(i => i.CreatedAt) 
                            : query.OrderBy(i => i.CreatedAt);
                        break;
                    default:
                        query = query.OrderBy(i => i.Id);
                        break;
                }
            }
        }

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
