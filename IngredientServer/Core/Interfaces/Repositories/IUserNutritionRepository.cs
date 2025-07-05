using IngredientServer.Core.Entities;

namespace IngredientServer.Core.Interfaces.Repositories;

public interface IUserNutritionRepository : IBaseRepository<UserNutritionTargets>
{
    Task<UserNutritionTargets?> GetByUserIdAsync();
    
    Task<UserNutritionTargets?> SaveNutrition(UserNutritionTargets targets);
}