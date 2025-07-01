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

        public async Task<List<FoodSuggestionDto>> GetSuggestionsAsync(
            FoodSuggestionRequestDto requestDto,
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                var systemPrompt = CreateFoodSuggestionSystemPrompt();
                var userPrompt = CreateFoodSuggestionUserPrompt(requestDto);

                var response = await CallOpenAIAsync(systemPrompt, userPrompt, cancellationToken);

                var suggestions = ParseFoodSuggestions(response, requestDto); // Truyền requestDto

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

        public async Task<FoodDataDto> GetRecipeSuggestionsAsync(
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

        public Task<List<int>> GetTargetDailyNutritionAsync(UserInformationDto userInformation,
            CancellationToken cancellationToken = default)
        {
            //Todo: Implement logic to calculate daily nutrition targets based on user information
            throw new NotImplementedException();
        }

        public Task<List<int>> GetTargetWeeklyNutritionAsync(UserInformationDto userInformation,
            CancellationToken cancellationToken = default)
        {
            //Todo: Implement logic to calculate weekly nutrition targets based on user information
            throw new NotImplementedException();
        }

        public Task<List<int>> GetTargetOverviewNutritionAsync(UserInformationDto userInformation, int dayAmount,
            CancellationToken cancellationToken = default)
        {
            //Todo: Implement logic to calculate overview nutrition targets based on user information and day amount
            throw new NotImplementedException();
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

Trả về kết quả dưới dạng JSON array với format sau:
[
  {
    ""name"": ""Tên món ăn"",
    ""image"": """",
    ""difficulty"": 5,
    ""kcal"": 250,
    ""prepTimeMinutes"": 15,
    ""cookTimeMinutes"": 30,
    ""ingredients"": [
      {
        ""ingredientId"": 123,  // CHÍNH XÁC ID từ danh sách user hoặc 0 nếu bổ sung
        ""ingredientName"": ""Tên nguyên liệu"",  // CHÍNH XÁC tên từ danh sách user
        ""quantity"": 1,  // KHÔNG vượt quá giới hạn
        ""unit"": 0  // CHÍNH XÁC unit từ danh sách user
      }
    ]
  }
]

CHỈ TRẢ VỀ JSON ARRAY, KHÔNG KÈM TEXT GIẢI THÍCH.";
        }


        private string CreateFoodSuggestionUserPrompt(FoodSuggestionRequestDto requestDto)
        {
            var userInfo = requestDto.UserInformation;
            var prompt = $"Gợi ý {requestDto.MaxSuggestions} món ăn phù hợp cho người dùng:\n\n";

            // Thông tin người dùng
            if (userInfo != null)
            {
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
            }

            // Danh sách nguyên liệu có sẵn
            if (requestDto.Ingredients?.Any() == true)
            {
                prompt += "\n=== NGUYÊN LIỆU CÓ SẴN (ƯU TIÊN SỬ DỤNG) ===\n";
                foreach (var ingredient in requestDto.Ingredients)
                {
                    prompt +=
                        $"• ID: {ingredient.IngredientId} | Tên: \"{ingredient.IngredientName}\" | Số lượng tối đa: {ingredient.Quantity} | Unit: {ingredient.Unit}\n";
                }

                prompt += "\n⚠️ LƯU Ý QUAN TRỌNG:\n";
                prompt +=
                    "- Khi sử dụng nguyên liệu từ danh sách trên: PHẢI giữ CHÍNH XÁC ingredientId, ingredientName, unit\n";
                prompt += "- Quantity không được vượt quá số lượng tối đa đã cho\n";
                prompt += "- Nguyên liệu bổ sung (không có trong danh sách): ingredientId = 0\n";
            }

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

Trả về kết quả dưới dạng JSON với format sau:
{
  ""name"": ""Tên món ăn"",
  ""description"": ""Mô tả chi tiết món ăn"",
  ""preparationTimeMinutes"": 15,
  ""cookingTimeMinutes"": 30,
  ""difficultyLevel"": 5,
  ""instructions"": [
    ""Bước 1: Mô tả chi tiết từng thao tác"",
    ""Bước 2: Mô tả chi tiết từng thao tác""
  ],
  ""calories"": 250,
  ""protein"": 15,
  ""carbohydrates"": 30,
  ""fat"": 8,
  ""fiber"": 3,
  ""mealType"": 1,
  ""mealDate"": ""2024-07-01T00:00:00"",
  ""ingredients"": [
    {
      ""ingredientId"": 123,  // CHÍNH XÁC ID từ danh sách user hoặc 0 nếu bổ sung
      ""ingredientName"": ""Tên nguyên liệu"",  // CHÍNH XÁC tên từ danh sách user
      ""quantity"": 1,  // KHÔNG vượt quá giới hạn
      ""unit"": 0  // CHÍNH XÁC unit từ danh sách user
    }
  ]
}

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

        private List<FoodSuggestionDto> ParseFoodSuggestions(string jsonResponse, FoodSuggestionRequestDto requestDto)
        {
            try
            {
                var jsonStart = jsonResponse.IndexOf('[');
                var jsonEnd = jsonResponse.LastIndexOf(']');

                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonContent = jsonResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var suggestions = JsonSerializer.Deserialize<List<FoodSuggestionDto>>(jsonContent, _jsonOptions) ??
                                      new();

                    // Kiểm tra số lượng nguyên liệu
                    foreach (var suggestion in suggestions)
                    {
                        if (suggestion?.Ingredients == null) continue;
                        foreach (var ingredient in suggestion.Ingredients)
                        {
                            if (ingredient.IngredientId <= 0) continue; // Có IngredientId
                            var requestIngredient =
                                requestDto.Ingredients.FirstOrDefault(
                                    i => i.IngredientId == ingredient.IngredientId);
                            if (requestIngredient == null || ingredient.Quantity <= requestIngredient.Quantity)
                                continue;
                            _logger.LogWarning(
                                "Ingredient {Name} (ID: {Id}) exceeds maximum quantity {MaxQuantity} {Unit}",
                                ingredient.IngredientName, ingredient.IngredientId, requestIngredient.Quantity,
                                requestIngredient.Unit);
                            ingredient.Quantity = requestIngredient.Quantity; // Giới hạn số lượng
                        }
                    }

                    return suggestions;
                }

                return new List<FoodSuggestionDto>();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse food suggestions JSON, returning empty list");
                return [];
            }
        }

        private FoodDataDto ParseRecipeData(string jsonResponse)
        {
            try
            {
                // Trích xuất JSON từ response
                var jsonStart = jsonResponse.IndexOf('{');
                var jsonEnd = jsonResponse.LastIndexOf('}');

                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonContent = jsonResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    return JsonSerializer.Deserialize<FoodDataDto>(jsonContent, _jsonOptions) ?? new FoodDataDto();
                }

                return new FoodDataDto();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse recipe JSON, returning empty recipe");
                return new FoodDataDto();
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