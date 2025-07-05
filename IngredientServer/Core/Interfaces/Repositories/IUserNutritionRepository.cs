using IngredientServer.Core.Entities;

namespace IngredientServer.Core.Interfaces.Repositories;

public interface IUserNutritionRepository : IBaseRepository<UserNutritionTargets>
{
    Task<UserNutritionTargets?> GetByUserIdAsync(int userId);
}