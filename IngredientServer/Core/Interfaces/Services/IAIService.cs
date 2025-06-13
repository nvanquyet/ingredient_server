// using IngredientServer.Core.Entities;
// using OpenAI.Chat;
//
// namespace IngredientServer.Core.Interfaces.Services
// {
//     public interface IAIService
//     {
//         // Gợi ý món ăn dựa trên nguyên liệu
//         Task<List<FoodSuggestion>> GetFoodSuggestionsAsync(
//             List<Ingredient> availableIngredients, 
//             NutritionGoal nutritionGoal = NutritionGoal.Balanced,
//             int maxSuggestions = 5);
//         
//         // Tạo công thức nấu ăn chi tiết
//         Task<DetailedRecipe> GenerateRecipeAsync(
//             string foodName, 
//             List<Ingredient> ingredients,
//             NutritionGoal nutritionGoal = NutritionGoal.Balanced);
//         
//         // Phân tích dinh dưỡng của món ăn
//         Task<NutritionAnalysis> AnalyzeNutritionAsync(Food food);
//         
//         //Analyze nutrition for a list of meals in a meal plan
//         Task<NutritionAnalysis> AnalyzeNutritionAsync(List<Meal> meals, NutritionGoal targetGoal = NutritionGoal.Balanced);
//     }
// }
