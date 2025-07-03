using System.Text.Json;
using System.Text.Json.Serialization;
using Azure;
using Azure.AI.Inference;
using Azure.Core.Pipeline;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IngredientServer.Core.Services
{
    public class AIService : IAIService, IDisposable
    {
        private readonly ChatCompletionsClient _chatClient;
        private readonly ILogger<AIService> _logger;
        private readonly string _model;
        private readonly SemaphoreSlim _semaphore;
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _disposed;

        public AIService(IConfiguration configuration, ILogger<AIService> logger)
        {
            _logger = logger;

            // Lấy config từ appsettings
            var endpoint = new Uri(configuration["AzureOpenAI:Endpoint"]);
            var apiKey = configuration["AzureOpenAI:ApiKey"];
            _model = configuration["AzureOpenAI:Model"];

            var credential = new AzureKeyCredential(apiKey);

            _chatClient = new ChatCompletionsClient(
                endpoint,
                credential,
                new AzureAIInferenceClientOptions()
                {
                    // Tối ưu hóa connection pool
                    Transport = new HttpClientTransport(new HttpClient
                    {
                        Timeout = TimeSpan.FromMinutes(2)
                    })
                }
            );

            // Giới hạn số request đồng thời để tránh rate limiting
            _semaphore = new SemaphoreSlim(10, 10); // Tối đa 10 request cùng lúc

            // Cấu hình JSON serialization
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<List<FoodSuggestionResponseDto>> GetSuggestionsAsync(
            FoodSuggestionRequestDto requestDto,
            List<FoodIngredientDto> ingredients,
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                var systemPrompt = CreateFoodSuggestionSystemPrompt();
                var userPrompt = CreateFoodSuggestionUserPrompt(requestDto, ingredients);

                var response = await CallOpenAIAsync(systemPrompt, userPrompt, cancellationToken);

                var suggestions = ParseFoodSuggestions(response, requestDto, ingredients); // Truyền requestDto

                _logger.LogInformation("Successfully generated {Count} food suggestions", suggestions.Count);

                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating food suggestions");
                throw new HttpRequestException("Failed to generate food suggestions from AI service", ex);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<FoodDataResponseDto> GetRecipeSuggestionsAsync(
            FoodRecipeRequestDto recipeRequest,
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                var systemPrompt = CreateRecipeSystemPrompt();
                var userPrompt = CreateRecipeUserPrompt(recipeRequest);

                var response = await CallOpenAIAsync(systemPrompt, userPrompt, cancellationToken);

                var recipe = ParseRecipeData(response);

                _logger.LogInformation("Successfully generated recipe for {FoodName}", recipeRequest.FoodName);

                return recipe;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recipe for {FoodName}", recipeRequest.FoodName);
                throw new HttpRequestException("Failed to generate recipe from AI service", ex);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<int>> GetTargetDailyNutritionAsync(UserInformationDto userInformation,
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                var systemPrompt = CreateNutritionTargetSystemPrompt();
                var userPrompt = CreateDailyNutritionUserPrompt(userInformation);

                var response = await CallOpenAIAsync(systemPrompt, userPrompt, cancellationToken);

                var nutritionTargets = ParseNutritionTargets(response);

                _logger.LogInformation("Successfully generated daily nutrition targets: {Targets}",
                    string.Join(", ", nutritionTargets));

                return nutritionTargets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily nutrition targets");
                // Return safe default values
                return new List<int> { 2000, 150, 250, 65, 25 };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<int>> GetTargetWeeklyNutritionAsync(UserInformationDto userInformation,
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                var systemPrompt = CreateNutritionTargetSystemPrompt();
                var userPrompt = CreateWeeklyNutritionUserPrompt(userInformation);

                var response = await CallOpenAIAsync(systemPrompt, userPrompt, cancellationToken);

                var nutritionTargets = ParseNutritionTargets(response);

                _logger.LogInformation("Successfully generated weekly nutrition targets: {Targets}",
                    string.Join(", ", nutritionTargets));

                return nutritionTargets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating weekly nutrition targets");
                return new List<int> { 2000, 150, 250, 65, 25 };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<int>> GetTargetOverviewNutritionAsync(UserInformationDto userInformation, int dayAmount,
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                var systemPrompt = CreateNutritionTargetSystemPrompt();
                var userPrompt = CreateOverviewNutritionUserPrompt(userInformation, dayAmount);

                var response = await CallOpenAIAsync(systemPrompt, userPrompt, cancellationToken);

                var nutritionTargets = ParseNutritionTargets(response);

                _logger.LogInformation(
                    "Successfully generated overview nutrition targets for {DayAmount} days: {Targets}",
                    dayAmount, string.Join(", ", nutritionTargets));

                return nutritionTargets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating overview nutrition targets for {DayAmount} days", dayAmount);
                return new List<int> { 2000, 150, 250, 65, 25 };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private string CreateNutritionTargetSystemPrompt()
        {
            return @"Bạn là một chuyên gia dinh dưỡng và huấn luyện viên thể hình chuyên nghiệp.
Nhiệm vụ của bạn là tính toán mục tiêu dinh dưỡng hàng ngày chính xác dựa trên thông tin cá nhân của người dùng.

HƯỚNG DẪN TÍNH TOÁN:
1. Tính BMR (Basal Metabolic Rate) sử dụng công thức Mifflin-St Jeor:
   - Nam: BMR = (10 × cân nặng kg) + (6.25 × chiều cao cm) - (5 × tuổi) + 5
   - Nữ: BMR = (10 × cân nặng kg) + (6.25 × chiều cao cm) - (5 × tuổi) - 161

2. Tính TDEE (Total Daily Energy Expenditure) = BMR × hệ số hoạt động:
   - Sedentary (ít vận động): 1.2
   - LightlyActive (vận động nhẹ): 1.375
   - ModeratelyActive (vận động vừa): 1.55
   - VeryActive (vận động nhiều): 1.725
   - ExtraActive (vận động cực nhiều): 1.9

3. Điều chỉnh calories theo mục tiêu:
   - WeightLoss: TDEE - 500 (giảm ~0.5kg/tuần)
   - WeightGain: TDEE + 500 (tăng ~0.5kg/tuần)
   - MuscleGain: TDEE + 300 (tăng cơ từ từ)
   - FatLoss: TDEE - 300 (giảm mỡ giữ cơ)
   - Maintenance: TDEE (duy trì)
   - GeneralHealth: TDEE (sức khỏe tổng quát)

4. Phân bố Macronutrient:
   - Giảm cân/Giảm mỡ: 35% protein, 35% carbs, 30% fat
   - Tăng cơ/Tăng cân: 30% protein, 45% carbs, 25% fat
   - Duy trì/Sức khỏe: 25% protein, 45% carbs, 30% fat

5. Chuyển đổi sang gram:
   - Protein: (calories × % protein) ÷ 4
   - Carbs: (calories × % carbs) ÷ 4
   - Fat: (calories × % fat) ÷ 9
   - Fiber: calories × 14 ÷ 1000 (khuyến nghị 14g/1000 calories)

GIÁ TRỊ MẶC ĐỊNH khi thiếu thông tin:
- Tuổi: 30, Cân nặng: 70kg, Chiều cao: 170cm
- Giới tính: Nam, Hoạt động: LightlyActive, Mục tiêu: Maintenance

ĐỊNH DẠNG TRẢ VỀ:
Trả về kết quả dưới dạng JSON array với đúng 5 số nguyên theo thứ tự:
[calories, protein_grams, carbs_grams, fat_grams, fiber_grams]

Ví dụ: [2000, 150, 250, 65, 25]

CHỈ TRẢ VỀ JSON ARRAY, KHÔNG KÈM TEXT GIẢI THÍCH.";
        }

        private string CreateDailyNutritionUserPrompt(UserInformationDto userInfo)
        {
            var prompt = "Tính toán mục tiêu dinh dưỡng HÀNG NGÀY cho người dùng:\n\n";

            prompt += "=== THÔNG TIN NGƯỜI DÙNG ===\n";

            if (userInfo.Gender.HasValue)
                prompt += $"• Giới tính: {userInfo.Gender}\n";

            if (userInfo.DateOfBirth.HasValue)
            {
                var age = DateTime.Now.Year - userInfo.DateOfBirth.Value.Year;
                prompt += $"• Tuổi: {age}\n";
            }

            if (userInfo.Height.HasValue)
                prompt += $"• Chiều cao: {userInfo.Height}cm\n";

            if (userInfo.Weight.HasValue)
                prompt += $"• Cân nặng hiện tại: {userInfo.Weight}kg\n";

            if (userInfo.TargetWeight.HasValue)
                prompt += $"• Cân nặng mục tiêu: {userInfo.TargetWeight}kg\n";

            if (userInfo.PrimaryNutritionGoal.HasValue)
                prompt += $"• Mục tiêu dinh dưỡng: {userInfo.PrimaryNutritionGoal}\n";

            if (userInfo.ActivityLevel.HasValue)
                prompt += $"• Mức độ hoạt động: {userInfo.ActivityLevel}\n";

            prompt += "\n=== YÊU CẦU ===\n";
            prompt +=
                "Tính toán mục tiêu dinh dưỡng hàng ngày (calories và macronutrients) phù hợp với thông tin và mục tiêu của người dùng.\n";
            prompt += "Áp dụng các công thức khoa học chính xác như đã hướng dẫn.\n";
            prompt += "Đảm bảo kết quả an toàn và phù hợp với sức khỏe.\n";

            return prompt;
        }

        private string CreateWeeklyNutritionUserPrompt(UserInformationDto userInfo)
        {
            var prompt = "Tính toán mục tiêu dinh dưỡng TRUNG BÌNH HÀNG NGÀY cho tuần (7 ngày) của người dùng:\n\n";

            prompt += "=== THÔNG TIN NGƯỜI DÙNG ===\n";

            if (userInfo.Gender.HasValue)
                prompt += $"• Giới tính: {userInfo.Gender}\n";

            if (userInfo.DateOfBirth.HasValue)
            {
                var age = DateTime.Now.Year - userInfo.DateOfBirth.Value.Year;
                prompt += $"• Tuổi: {age}\n";
            }

            if (userInfo.Height.HasValue)
                prompt += $"• Chiều cao: {userInfo.Height}cm\n";

            if (userInfo.Weight.HasValue)
                prompt += $"• Cân nặng hiện tại: {userInfo.Weight}kg\n";

            if (userInfo.TargetWeight.HasValue)
                prompt += $"• Cân nặng mục tiêu: {userInfo.TargetWeight}kg\n";

            if (userInfo.PrimaryNutritionGoal.HasValue)
                prompt += $"• Mục tiêu dinh dưỡng: {userInfo.PrimaryNutritionGoal}\n";

            if (userInfo.ActivityLevel.HasValue)
                prompt += $"• Mức độ hoạt động: {userInfo.ActivityLevel}\n";

            prompt += "\n=== YÊU CẦU ===\n";
            prompt += "Tính toán mục tiêu dinh dưỡng trung bình hàng ngày cho một tuần.\n";
            prompt += "Kết quả sẽ được sử dụng để đánh giá nutrition trung bình hàng ngày trong tuần.\n";
            prompt += "Áp dụng các công thức khoa học chính xác như đã hướng dẫn.\n";

            return prompt;
        }

        private string CreateOverviewNutritionUserPrompt(UserInformationDto userInfo, int dayAmount)
        {
            var prompt =
                $"Tính toán mục tiêu dinh dưỡng TRUNG BÌNH HÀNG NGÀY cho tổng quan {dayAmount} ngày của người dùng:\n\n";

            prompt += "=== THÔNG TIN NGƯỜI DÙNG ===\n";

            if (userInfo.Gender.HasValue)
                prompt += $"• Giới tính: {userInfo.Gender}\n";

            if (userInfo.DateOfBirth.HasValue)
            {
                var age = DateTime.Now.Year - userInfo.DateOfBirth.Value.Year;
                prompt += $"• Tuổi: {age}\n";
            }

            if (userInfo.Height.HasValue)
                prompt += $"• Chiều cao: {userInfo.Height}cm\n";

            if (userInfo.Weight.HasValue)
                prompt += $"• Cân nặng hiện tại: {userInfo.Weight}kg\n";

            if (userInfo.TargetWeight.HasValue)
                prompt += $"• Cân nặng mục tiêu: {userInfo.TargetWeight}kg\n";

            if (userInfo.PrimaryNutritionGoal.HasValue)
                prompt += $"• Mục tiêu dinh dưỡng: {userInfo.PrimaryNutritionGoal}\n";

            if (userInfo.ActivityLevel.HasValue)
                prompt += $"• Mức độ hoạt động: {userInfo.ActivityLevel}\n";

            prompt += $"\n=== THÔNG TIN THỜI GIAN ===\n";
            prompt += $"• Tổng số ngày: {dayAmount} ngày\n";

            prompt += "\n=== YÊU CẦU ===\n";
            prompt += "Tính toán mục tiêu dinh dưỡng trung bình hàng ngày cho tổng quan dinh dưỡng.\n";
            prompt += "Kết quả sẽ được sử dụng để đánh giá nutrition trung bình hàng ngày trong toàn bộ thời gian.\n";
            prompt += "Áp dụng các công thức khoa học chính xác như đã hướng dẫn.\n";

            return prompt;
        }

        private List<int> ParseNutritionTargets(string jsonResponse)
        {
            try
            {
                // Tìm JSON array trong response
                var jsonStart = jsonResponse.IndexOf('[');
                var jsonEnd = jsonResponse.LastIndexOf(']');

                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonContent = jsonResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var targets = JsonSerializer.Deserialize<List<int>>(jsonContent, options);

                    if (targets != null && targets.Count == 5)
                    {
                        // Validate reasonable ranges
                        if (targets[0] >= 1000 && targets[0] <= 5000 && // Calories: 1000-5000
                            targets[1] >= 50 && targets[1] <= 400 && // Protein: 50-400g
                            targets[2] >= 100 && targets[2] <= 600 && // Carbs: 100-600g
                            targets[3] >= 30 && targets[3] <= 200 && // Fat: 30-200g
                            targets[4] >= 15 && targets[4] <= 80) // Fiber: 15-80g
                        {
                            return targets;
                        }
                    }
                }

                _logger.LogWarning("Invalid nutrition targets format or values out of range: {Response}", jsonResponse);
                return new List<int> { 2000, 150, 250, 65, 25 };
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse nutrition targets JSON: {Response}", jsonResponse);
                return new List<int> { 2000, 150, 250, 65, 25 };
            }
        }

        private async Task<string> CallOpenAIAsync(string systemPrompt, string userPrompt,
            CancellationToken cancellationToken)
        {
            var options = new ChatCompletionsOptions
            {
                Model = _model,
                Messages =
                {
                    new ChatRequestSystemMessage(systemPrompt),
                    new ChatRequestUserMessage(userPrompt)
                },
                MaxTokens = 2000,
                Temperature = 0.7f,
                FrequencyPenalty = 0.1f,
                PresencePenalty = 0.1f
            };

            var response = await _chatClient.CompleteAsync(options, cancellationToken);

            if (response?.Value?.Content == null)
            {
                throw new InvalidOperationException("Received empty response from OpenAI");
            }

            return response.Value.Content;
        }


        private string CreateFoodSuggestionSystemPrompt()
        {
            return @"Bạn là một chuyên gia dinh dưỡng và đầu bếp chuyên nghiệp. 
Nhiệm vụ của bạn là đưa ra gợi ý món ăn phù hợp dựa trên thông tin người dùng và danh sách nguyên liệu được cung cấp.

QUAN TRỌNG - QUY TẮC VỀ NGUYÊN LIỆU:
1. NGUYÊN LIỆU CÓ SẴN: Nếu sử dụng nguyên liệu từ danh sách người dùng cung cấp:
   - ingredientId: PHẢI CHÍNH XÁC khớp với ID trong danh sách (KHÔNG tự tạo ID mới)
   - ingredientName: PHẢI CHÍNH XÁC khớp với tên trong danh sách
   - unit: PHẢI CHÍNH XÁC khớp với đơn vị trong danh sách
   - quantity: KHÔNG ĐƯỢC vượt quá số lượng tối đa đã cung cấp

2. NGUYÊN LIỆU BỔ SUNG: Nếu cần thêm nguyên liệu không có trong danh sách:
   - ingredientId: ĐẶT CHÍNH XÁC = 0
   - ingredientName: Tên nguyên liệu bổ sung
   - unit: Đơn vị phù hợp (0-12)
   - quantity: Số lượng cần thiết

3. ĐƠN VỊ TÍNH (unit) - SỬ DỤNG SỐ NGUYÊN:
   0=Kilogram, 1=Liter, 2=Piece, 3=Box, 4=Gram, 5=Milliliter, 
   6=Can, 7=Cup, 8=Tablespoon, 9=Teaspoon, 10=Package, 11=Bottle, 12=Other

Trả về kết quả dưới dạng JSON array với format sau (CHÍNH XÁC theo tên field):
[
  {
    ""name"": ""Tên món ăn"",
    ""image"": ""https://example.com/path/to/food-image.jpg"",
    ""difficulty"": 5,
    ""kcal"": 250.0,
    ""prepTimeMinutes"": 15,
    ""cookTimeMinutes"": 30,
    ""ingredients"": [
      {
        ""ingredientId"": 123,
        ""ingredientName"": ""Tên nguyên liệu"",
        ""quantity"": 1.0,
        ""unit"": 0
      }
    ]
  }
]

LƯU Ý QUAN TRỌNG:
- kcal phải là số thập phân (decimal)
- quantity phải là số thập phân (decimal)
- Tất cả field names phải chính xác như trên (case-sensitive)

CHỈ TRẢ VỀ JSON ARRAY, KHÔNG KÈM TEXT GIẢI THÍCH.";
        }

        private string CreateFoodSuggestionUserPrompt(FoodSuggestionRequestDto requestDto,
            List<FoodIngredientDto> ingredients)
        {
            var userInfo = requestDto.UserInformation;
            var prompt = $"Gợi ý {requestDto.MaxSuggestions} món ăn phù hợp cho người dùng:\n\n";

            // Thông tin người dùng
            prompt += "=== THÔNG TIN NGƯỜI DÙNG ===\n";

            if (userInfo.Gender.HasValue)
                prompt += $"• Giới tính: {userInfo.Gender}\n";

            if (userInfo.DateOfBirth.HasValue)
            {
                var age = DateTime.Now.Year - userInfo.DateOfBirth.Value.Year;
                prompt += $"• Tuổi: {age}\n";
            }

            if (userInfo.Height.HasValue)
                prompt += $"• Chiều cao: {userInfo.Height}cm\n";

            if (userInfo.Weight.HasValue)
                prompt += $"• Cân nặng hiện tại: {userInfo.Weight}kg\n";

            if (userInfo.TargetWeight.HasValue)
                prompt += $"• Cân nặng mục tiêu: {userInfo.TargetWeight}kg\n";

            if (userInfo.PrimaryNutritionGoal.HasValue)
                prompt += $"• Mục tiêu dinh dưỡng: {userInfo.PrimaryNutritionGoal}\n";

            if (userInfo.ActivityLevel.HasValue)
                prompt += $"• Mức độ hoạt động: {userInfo.ActivityLevel}\n";

            prompt += "\n=== NGUYÊN LIỆU CÓ SẴN (ƯU TIÊN SỬ DỤNG) ===\n";
            prompt = ingredients.Aggregate(prompt,
                (current, ingredient) =>
                    current +
                    $"• ID: {ingredient.IngredientId} | Tên: \"{ingredient.IngredientName}\" | Số lượng tối đa: {ingredient.Quantity} | Unit: {ingredient.Unit}\n");

            prompt += "\n⚠️ LƯU Ý QUAN TRỌNG:\n";
            prompt +=
                "- Khi sử dụng nguyên liệu từ danh sách trên: PHẢI giữ CHÍNH XÁC ingredientId, ingredientName, unit\n";
            prompt += "- Quantity không được vượt quá số lượng tối đa đã cho\n";
            prompt += "- Nguyên liệu bổ sung (không có trong danh sách): ingredientId = 0\n";


            prompt += "\n=== YÊU CẦU ===\n";
            prompt += "Đưa ra các món ăn phù hợp với mục tiêu sức khỏe và dinh dưỡng của người dùng.\n";
            prompt += "Ưu tiên sử dụng nguyên liệu có sẵn trong danh sách.\n";
            prompt += "Đảm bảo tuân thủ CHÍNH XÁC các quy tắc về ingredientId, ingredientName, unit đã nêu.\n";

            return prompt;
        }

        private string CreateRecipeSystemPrompt()
        {
            return @"Bạn là một đầu bếp chuyên nghiệp với nhiều năm kinh nghiệm. 
Nhiệm vụ của bạn là cung cấp công thức nấu ăn chi tiết và chính xác.

QUAN TRỌNG - QUY TẮC VỀ NGUYÊN LIỆU:
1. NGUYÊN LIỆU CÓ SẴN: Nếu sử dụng nguyên liệu từ danh sách người dùng cung cấp:
   - ingredientId: PHẢI CHÍNH XÁC khớp với ID trong danh sách (KHÔNG tự tạo ID mới)
   - ingredientName: PHẢI CHÍNH XÁC khớp với tên trong danh sách  
   - unit: PHẢI CHÍNH XÁC khớp với đơn vị trong danh sách
   - quantity: KHÔNG ĐƯỢC vượt quá số lượng tối đa đã cung cấp

2. NGUYÊN LIỆU BỔ SUNG: Nếu cần thêm nguyên liệu không có trong danh sách:
   - ingredientId: ĐẶT CHÍNH XÁC = 0
   - ingredientName: Tên nguyên liệu bổ sung
   - unit: Đơn vị phù hợp (0-12)
   - quantity: Số lượng cần thiết

3. ĐƠN VỊ TÍNH (unit) - SỬ DỤNG SỐ NGUYÊN:
   0=Kilogram, 1=Liter, 2=Piece, 3=Box, 4=Gram, 5=Milliliter,
   6=Can, 7=Cup, 8=Tablespoon, 9=Teaspoon, 10=Package, 11=Bottle, 12=Other

4. LOẠI BỮA ĂN (mealType) - SỬ DỤNG SỐ NGUYÊN:
   0=Breakfast, 1=Lunch, 2=Dinner, 3=Snack

Trả về kết quả dưới dạng JSON với format sau (CHÍNH XÁC theo tên field):
{
  ""id"": 0,
  ""name"": ""Tên món ăn"",
  ""description"": ""Mô tả chi tiết món ăn"",
  ""imageUrl"": ""https://example.com/path/to/recipe-image.jpg"",
  ""preparationTimeMinutes"": 15,
  ""cookingTimeMinutes"": 30,
  ""calories"": 250.0,
  ""protein"": 15.0,
  ""carbohydrates"": 30.0,
  ""fat"": 8.0,
  ""fiber"": 3.0,
  ""instructions"": [
    ""Bước 1: Mô tả chi tiết từng thao tác"",
    ""Bước 2: Mô tả chi tiết từng thao tác""
  ],
  ""tips"": [
    ""Mẹo 1: Gợi ý hữu ích"",
    ""Mẹo 2: Lưu ý quan trọng""
  ],
  ""difficultyLevel"": 5,
  ""mealType"": 1,
  ""mealDate"": ""2024-07-01T00:00:00Z"",
  ""ingredients"": [
    {
      ""ingredientId"": 123,
      ""ingredientName"": ""Tên nguyên liệu"",
      ""quantity"": 1.0,
      ""unit"": 0
    }
  ]
}

LƯU Ý QUAN TRỌNG:
- Tất cả số liệu dinh dưỡng (calories, protein, carbohydrates, fat, fiber) phải là số thập phân (decimal)
- quantity cũng phải là số thập phân
- mealDate phải có format ISO 8601 với UTC timezone
- id luôn đặt = 0 (sẽ được generate ở server)

CHỈ TRẢ VỀ JSON OBJECT, KHÔNG KÈM TEXT GIẢI THÍCH.";
        }

        private string CreateRecipeUserPrompt(FoodRecipeRequestDto recipeRequest)
        {
            var prompt = $"Tạo công thức nấu ăn chi tiết cho món: \"{recipeRequest.FoodName}\"\n\n";

            // Danh sách nguyên liệu có sẵn
            if (recipeRequest.Ingredients?.Any() == true)
            {
                prompt += "=== NGUYÊN LIỆU CÓ SẴN (ƯU TIÊN SỬ DỤNG) ===\n";
                foreach (var ingredient in recipeRequest.Ingredients)
                {
                    prompt +=
                        $"• ID: {ingredient.IngredientId} | Tên: \"{ingredient.IngredientName}\" | Số lượng tối đa: {ingredient.Quantity} | Unit: {ingredient.Unit}\n";
                }

                prompt += "\n⚠️ LƯU Ý QUAN TRỌNG:\n";
                prompt +=
                    "- Khi sử dụng nguyên liệu từ danh sách trên: PHẢI giữ CHÍNH XÁC ingredientId, ingredientName, unit\n";
                prompt += "- Quantity không được vượt quá số lượng tối đa đã cho\n";
                prompt += "- Nguyên liệu bổ sung (không có trong danh sách): ingredientId = 0\n\n";
            }

            prompt += "=== YÊU CẦU ===\n";
            prompt += "1. Cung cấp hướng dẫn nấu ăn từng bước một cách chi tiết và dễ hiểu\n";
            prompt += "2. Ưu tiên sử dụng nguyên liệu có sẵn trong danh sách\n";
            prompt += "3. Đảm bảo tuân thủ CHÍNH XÁC các quy tắc về ingredientId, ingredientName, unit\n";
            prompt += "4. Ước tính chính xác thông tin dinh dưỡng (calories, protein, carbs, fat, fiber)\n";
            prompt += "5. Xác định đúng loại bữa ăn (mealType) phù hợp với món ăn\n";

            return prompt;
        }


        private List<FoodSuggestionResponseDto> ParseFoodSuggestions(string jsonResponse,
            FoodSuggestionRequestDto requestDto, List<FoodIngredientDto> ingredients)
        {
            try
            {
                var jsonStart = jsonResponse.IndexOf('[');
                var jsonEnd = jsonResponse.LastIndexOf(']');

                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonContent = jsonResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);

                    // Sử dụng JsonSerializerOptions phù hợp với DTO
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        PropertyNameCaseInsensitive = true // Thêm để xử lý case mismatch
                    };

                    var suggestions =
                        JsonSerializer.Deserialize<List<FoodSuggestionResponseDto>>(jsonContent, options) ?? new();

                    // Kiểm tra và fix số lượng nguyên liệu
                    foreach (var suggestion in suggestions)
                    {
                        if (suggestion?.Ingredients == null) continue;

                        // Đảm bảo Ingredients không null
                        suggestion.Ingredients ??= new List<FoodIngredientDto>();

                        foreach (var ingredient in suggestion.Ingredients)
                        {
                            if (ingredient.IngredientId <= 0) continue; // Skip nguyên liệu bổ sung

                            var requestIngredient =
                                ingredients.FirstOrDefault(i => i.IngredientId == ingredient.IngredientId);
                            if (requestIngredient == null) continue;

                            // Kiểm tra quantity không vượt quá giới hạn
                            if (ingredient.Quantity > requestIngredient.Quantity)
                            {
                                _logger.LogWarning(
                                    "Ingredient {Name} (ID: {Id}) exceeds maximum quantity {MaxQuantity} {Unit}, adjusting to {AdjustedQuantity}",
                                    ingredient.IngredientName, ingredient.IngredientId,
                                    requestIngredient.Quantity, requestIngredient.Unit, requestIngredient.Quantity);
                                ingredient.Quantity = requestIngredient.Quantity;
                            }
                        }
                    }

                    return suggestions;
                }

                return new List<FoodSuggestionResponseDto>();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse food suggestions JSON: {Response}", jsonResponse);
                return new List<FoodSuggestionResponseDto>();
            }
        }

        private FoodDataResponseDto ParseRecipeData(string jsonResponse)
        {
            try
            {
                // Trích xuất JSON từ response
                var jsonStart = jsonResponse.IndexOf('{');
                var jsonEnd = jsonResponse.LastIndexOf('}');

                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonContent = jsonResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);

                    // Sử dụng JsonSerializerOptions phù hợp với DTO
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        PropertyNameCaseInsensitive = true // Thêm để xử lý case mismatch
                    };

                    var recipe = JsonSerializer.Deserialize<FoodDataResponseDto>(jsonContent, options);

                    if (recipe != null)
                    {
                        // Đảm bảo các collection không null
                        recipe.Instructions ??= new List<string>();
                        recipe.Tips ??= new List<string>();
                        recipe.Ingredients ??= new List<FoodIngredientDto>();

                        // Set default MealDate nếu cần
                        if (recipe.MealDate == default(DateTime))
                        {
                            recipe.MealDate = DateTime.UtcNow;
                        }

                        return recipe;
                    }
                }

                return new FoodDataResponseDto();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse recipe JSON: {Response}", jsonResponse);
                return new FoodDataResponseDto();
            }
        }


        public void Dispose()
        {
            if (!_disposed)
            {
                _semaphore?.Dispose();
                _disposed = true;
            }
        }
    }
}