using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Core.Entities;
using OpenAI.Chat;
using Azure;
using Azure.AI.OpenAI;
using System.Text.Json;

namespace IngredientServer.Core.Services
{
    public class AIService : IAIService
    {
        private readonly Uri endpoint = new Uri("https://nvanq-mbrhssqv-eastus2.cognitiveservices.azure.com/");
        private readonly string deploymentName = "gpt-4.1";
        private readonly string apiKey = "APIKEY";
        
        private readonly AzureOpenAIClient azureClient;
        private readonly ChatClient chatClient;

        public AIService()
        {
            azureClient = new(endpoint, new AzureKeyCredential(apiKey));
            chatClient = azureClient.GetChatClient(deploymentName);
        }

        public async Task<string> GetChatResponseAsync(string prompt, List<ChatMessage> messages = null)
        {
            var requestOptions = new ChatCompletionOptions()
            {
                Temperature = 0.7f,
                TopP = 0.9f,
                FrequencyPenalty = 0.0f,
                PresencePenalty = 0.0f,
            };

            messages ??= new List<ChatMessage>()
            {
                new SystemChatMessage("You are a helpful cooking and nutrition assistant."),
                new UserChatMessage(prompt),
            };

            var response = await chatClient.CompleteChatAsync(messages, requestOptions);
            return response.Value.Content[0].Text;
        }

        public async Task<List<FoodSuggestion>> GetFoodSuggestionsAsync(
            List<Ingredient> availableIngredients, 
            NutritionGoal nutritionGoal = NutritionGoal.Balanced,
            int maxSuggestions = 5)
        {
            var ingredientsList = string.Join(", ", availableIngredients.Select(i => 
                $"{i.Name} ({i.Quantity} {i.Unit})"));

            var prompt = $@"Dựa trên các nguyên liệu sau: {ingredientsList}
                            Mục tiêu dinh dưỡng: {GetNutritionGoalDescription(nutritionGoal)}

                            Hãy gợi ý {maxSuggestions} món ăn phù hợp nhất. Trả về kết quả dưới dạng JSON với format sau:
                            {{
                              ""suggestions"": [
                                {{
                                  ""name"": ""Tên món ăn"",
                                  ""description"": ""Mô tả ngắn gọn"",
                                  ""category"": ""MainDish/SideDish/Soup/etc"",
                                  ""cookingMethod"": ""Fried/Boiled/Grilled/etc"",
                                  ""preparationTimeMinutes"": 30,
                                  ""requiredIngredients"": [""nguyên liệu bắt buộc""],
                                  ""optionalIngredients"": [""nguyên liệu tùy chọn""],
                                  ""shortRecipe"": ""Cách làm ngắn gọn"",
                                  ""estimatedNutrition"": {{
                                    ""calories"": 350,
                                    ""protein"": 25,
                                    ""carbs"": 30,
                                    ""fats"": 15,
                                    ""fiber"": 5,
                                    ""sugar"": 8,
                                    ""sodium"": 800
                                  }},
                                  ""matchScore"": 85,
                                  ""whyRecommended"": ""Lý do gợi ý""
                                }}
                              ]
                            }}

                            Ưu tiên các món ăn:
                            1. Sử dụng tối đa nguyên liệu hiện có
                            2. Phù hợp với mục tiêu dinh dưỡng
                            3. Dễ làm và ngon miệng
                            4. Cân bằng dinh dưỡng";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are an expert chef and nutritionist. Always respond in valid JSON format as requested."),
                new UserChatMessage(prompt)
            };

            var response = await GetChatResponseAsync(prompt, messages);
            
            try
            {
                var result = JsonSerializer.Deserialize<SuggestionResponse>(response);
                return result?.Suggestions ?? new List<FoodSuggestion>();
            }
            catch (JsonException)
            {
                // Fallback if JSON parsing fails
                return new List<FoodSuggestion>();
            }
        }

        public async Task<DetailedRecipe> GenerateRecipeAsync(
            string foodName, 
            List<Ingredient> ingredients,
            NutritionGoal nutritionGoal = NutritionGoal.Balanced)
        {
            var ingredientsList = string.Join(", ", ingredients.Select(i => 
                $"{i.Name} ({i.Quantity} {i.Unit})"));

            var prompt = $@"Tạo công thức nấu ăn chi tiết cho món: {foodName}
                        Nguyên liệu có sẵn: {ingredientsList}
                        Mục tiêu dinh dưỡng: {GetNutritionGoalDescription(nutritionGoal)}

                        Trả về JSON với format:
                        {{
                          ""name"": ""{foodName}"",
                          ""description"": ""Mô tả món ăn"",
                          ""ingredients"": [
                            {{
                              ""name"": ""Tên nguyên liệu"",
                              ""quantity"": 200,
                              ""unit"": ""gram"",
                              ""isOptional"": false,
                              ""preparationNote"": ""cắt nhỏ""
                            }}
                          ],
                          ""steps"": [
                            {{
                              ""stepNumber"": 1,
                              ""instruction"": ""Hướng dẫn từng bước"",
                              ""timeMinutes"": 5,
                              ""temperature"": ""Lửa vừa"",
                              ""tips"": [""Mẹo nấu ăn""]
                            }}
                          ],
                          ""preparationTimeMinutes"": 15,
                          ""cookingTimeMinutes"": 20,
                          ""servings"": 2,
                          ""difficulty"": ""Easy"",
                          ""nutritionPerServing"": {{
                            ""calories"": 400,
                            ""protein"": 30,
                            ""carbs"": 35,
                            ""fats"": 18,
                            ""fiber"": 6,
                            ""sugar"": 10,
                            ""sodium"": 900
                          }},
                          ""tips"": [""Mẹo tổng quát""]
                        }}";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a professional chef. Provide detailed, accurate recipes in valid JSON format."),
                new UserChatMessage(prompt)
            };

            var response = await GetChatResponseAsync(prompt, messages);
            
            try
            {
                return JsonSerializer.Deserialize<DetailedRecipe>(response) ?? new DetailedRecipe();
            }
            catch (JsonException)
            {
                return new DetailedRecipe { Name = foodName };
            }
        }

        public async Task<NutritionAnalysis> AnalyzeNutritionAsync(Food food)
        {
            var prompt = $@"
Phân tích dinh dưỡng cho món ăn:
Tên: {food.Name}
Calories/100g: {food.CaloriesPer100G}
Protein/100g: {food.ProteinPer100G}g
Carbs/100g: {food.CarbsPer100G}g
Fats/100g: {food.FatsPer100G}g
Fiber/100g: {food.FiberPer100G}g
Sugar/100g: {food.SugarPer100G}g
Sodium/100g: {food.SodiumPer100G}mg

Trả về phân tích dưới dạng JSON:
{{
  ""nutrition"": {{
    ""calories"": {food.CaloriesPerPortion},
    ""protein"": {food.ProteinPerPortion},
    ""carbs"": {food.CarbsPerPortion},
    ""fats"": {food.FatsPerPortion},
    ""fiber"": {food.FiberPer100G ?? 0},
    ""sugar"": {food.SugarPer100G ?? 0},
    ""sodium"": {food.SodiumPer100G ?? 0}
  }},
  ""healthScore"": ""Good/Excellent/Fair/Poor"",
  ""healthBenefits"": [""Lợi ích sức khỏe""],
  ""nutritionalConcerns"": [""Mối quan tâm dinh dưỡng""],
  ""improvementSuggestions"": [""Gợi ý cải thiện""],
  ""isAlignedWithGoal"": true,
  ""goalAlignment"": ""Đánh giá phù hợp với mục tiêu""
}}";

            var response = await GetChatResponseAsync(prompt);
            
            try
            {
                return JsonSerializer.Deserialize<NutritionAnalysis>(response) ?? new NutritionAnalysis();
            }
            catch (JsonException)
            {
                return new NutritionAnalysis();
            }
        }

        public async Task<List<IngredientSubstitution>> GetIngredientSubstitutionsAsync(
            string originalIngredient, 
            NutritionGoal nutritionGoal)
        {
            var prompt = $@"
Tìm các nguyên liệu thay thế cho: {originalIngredient}
Mục tiêu dinh dưỡng: {GetNutritionGoalDescription(nutritionGoal)}

Trả về JSON với 3-5 lựa chọn thay thế:
{{
  ""substitutions"": [
    {{
      ""originalIngredient"": ""{originalIngredient}"",
      ""substituteIngredient"": ""Nguyên liệu thay thế"",
      ""conversionRatio"": ""1:1"",
      ""reason"": ""Lý do thay thế"",
      ""tasteImpact"": ""Ảnh hưởng đến vị"",
      ""nutritionImpact"": ""Ảnh hưởng dinh dưỡng""
    }}
  ]
}}";

            var response = await GetChatResponseAsync(prompt);
            
            try
            {
                var result = JsonSerializer.Deserialize<SubstitutionResponse>(response);
                return result?.Substitutions ?? new List<IngredientSubstitution>();
            }
            catch (JsonException)
            {
                return new List<IngredientSubstitution>();
            }
        }

        private string GetNutritionGoalDescription(NutritionGoal goal)
        {
            return goal switch
            {
                NutritionGoal.Balanced => "Cân bằng dinh dưỡng",
                NutritionGoal.WeightLoss => "Giảm cân (ít calories, nhiều protein, ít carb)",
                NutritionGoal.WeightGain => "Tăng cân (nhiều calories, protein cao)",
                NutritionGoal.MuscleGain => "Tăng cơ (protein cao, carb vừa phải)",
                NutritionGoal.LowCarb => "Ít carb (dưới 50g carb/ngày)",
                NutritionGoal.HighProtein => "Nhiều protein (trên 1.5g/kg thể trọng)",
                NutritionGoal.Vegetarian => "Ăn chay (không thịt, có trứng sữa)",
                NutritionGoal.Vegan => "Ăn thuần chay (hoàn toàn thực vật)",
                NutritionGoal.Keto => "Keto (75% fat, 20% protein, 5% carb)",
                NutritionGoal.LowSodium => "Ít muối (dưới 1500mg sodium/ngày)",
                NutritionGoal.DiabeticFriendly => "Phù hợp tiểu đường (ít đường, carb phức)",
                NutritionGoal.HeartHealthy => "Bảo vệ tim mạch (ít fat bão hòa, nhiều omega-3)",
                _ => "Dinh dưỡng cân bằng"
            };
        }
    }

    // Helper classes for JSON deserialization
    public class SuggestionResponse
    {
        public List<FoodSuggestion> Suggestions { get; set; } = new();
    }

    public class SubstitutionResponse
    {
        public List<IngredientSubstitution> Substitutions { get; set; } = new();
    }
}