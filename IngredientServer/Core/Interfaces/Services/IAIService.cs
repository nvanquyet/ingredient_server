using IngredientServer.Core.Entities;
using OpenAI.Chat;

namespace IngredientServer.Core.Interfaces.Services
{
    public interface IAIService
    {
        Task<string> GetChatResponseAsync(string prompt, List<ChatMessage> messages = null);
        
        // Gợi ý món ăn dựa trên nguyên liệu
        Task<List<FoodSuggestion>> GetFoodSuggestionsAsync(
            List<Ingredient> availableIngredients, 
            NutritionGoal nutritionGoal = NutritionGoal.Balanced,
            int maxSuggestions = 5);
        
        // Tạo công thức nấu ăn chi tiết
        Task<DetailedRecipe> GenerateRecipeAsync(
            string foodName, 
            List<Ingredient> ingredients,
            NutritionGoal nutritionGoal = NutritionGoal.Balanced);
        
        // Phân tích dinh dưỡng của món ăn
        Task<NutritionAnalysis> AnalyzeNutritionAsync(Food food);
        
        // Gợi ý thay thế nguyên liệu
        Task<List<IngredientSubstitution>> GetIngredientSubstitutionsAsync(
            string originalIngredient, 
            NutritionGoal nutritionGoal);
    }
}
