using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories
{
    public class FoodRepository : BaseRepository<Food>, IFoodRepository
    {
        public FoodRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Food>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(f => f.UserId == userId)
                .Include(f => f.FoodIngredients)
                .ThenInclude(fi => fi.Ingredient)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<Food?> GetByIdAndUserIdAsync(int foodId, int userId)
        {
            return await _dbSet
                .Include(f => f.FoodIngredients)
                .ThenInclude(fi => fi.Ingredient)
                .FirstOrDefaultAsync(f => f.Id == foodId && f.UserId == userId);
        }

        public async Task<List<Food>> SearchAsync(int userId, string searchTerm)
        {
            return await _dbSet
                .Where(f => f.UserId == userId && 
                           (f.Name.Contains(searchTerm) || 
                            (f.Description != null && f.Description.Contains(searchTerm))))
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<List<Food>> GetByCategoryAsync(int userId, FoodCategory category)
        {
            return await _dbSet
                .Where(f => f.UserId == userId && f.Category == category)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<List<Food>> GetByIngredientsAsync(int userId, List<int> ingredientIds)
        {
            return await _dbSet
                .Where(f => f.UserId == userId && 
                           f.FoodIngredients.Any(fi => ingredientIds.Contains(fi.IngredientId)))
                .Include(f => f.FoodIngredients)
                .ThenInclude(fi => fi.Ingredient)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }
    }
}