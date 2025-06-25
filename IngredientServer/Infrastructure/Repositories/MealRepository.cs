using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Core.Services;
using IngredientServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public class MealRepository : BaseRepository<Meal>, IMealRepository
{
    public MealRepository(ApplicationDbContext context, IUserContextService userContextService)
        : base(context, userContextService)
    {
    }

    // Override GetAllAsync to include related MealFoods
    public override async Task<IEnumerable<Meal>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await Context.Set<Meal>()
            .Where(e => e.UserId == AuthenticatedUserId)
            .Include(m => m.MealFoods)
            .ThenInclude(mf => mf.Food)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    // Override GetByIdAsync to include related MealFoods
    public override async Task<Meal?> GetByIdAsync(int id)
    {
        return await Context.Set<Meal>()
            .Where(e => e.Id == id && e.UserId == AuthenticatedUserId)
            .Include(m => m.MealFoods)
            .ThenInclude(mf => mf.Food)
            .FirstOrDefaultAsync();
    }

    // Get meals by date
    public async Task<IEnumerable<Meal>> GetByDateAsync(string date, int pageNumber = 1, int pageSize = 10)
    {
        if (!DateTime.TryParse(date, out var parsedDate))
        {
            throw new ArgumentException("Invalid date format.");
        }

        return await Context.Set<Meal>()
            .Where(m => m.UserId == AuthenticatedUserId && m.MealDate.Date == parsedDate.Date)
            .Include(m => m.MealFoods)
            .ThenInclude(mf => mf.Food)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
