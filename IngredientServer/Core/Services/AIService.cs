using System.Text.Json;
using System.Text.Json.Serialization;
using Azure;
using Azure.AI.Inference;
using Azure.Core.Pipeline;
using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace IngredientServer.Core.Services
{
    public class AIService : IAIService, IDisposable
    {
        private readonly ChatCompletionsClient _chatClient;
        private readonly ILogger<AIService> _logger;
        private readonly IImageService _imageService;
        private readonly string _model;
        private readonly SemaphoreSlim _semaphore;
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _disposed;

        public AIService(IConfiguration configuration, IImageService _imageService, ILogger<AIService> logger)
        {
            _logger = logger;
            this._imageService = _imageService;

            var endpoint = new Uri(configuration["AzureOpenAI:Endpoint"]);
            var apiKey = configuration["AzureOpenAI:ApiKey"];
            _model = configuration["AzureOpenAI:Model"];

            var credential = new AzureKeyCredential(apiKey);

            _chatClient = new ChatCompletionsClient(
                endpoint,
                credential,
                new AzureAIInferenceClientOptions()
                {
                    Transport = new HttpClientTransport(new HttpClient
                    {
                        Timeout = TimeSpan.FromMinutes(2)
                    })
                }
            );

            _semaphore = new SemaphoreSlim(10, 10);

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

                var suggestions = ParseFoodSuggestions(response, requestDto, ingredients);

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
                return new List<int> { 2000, 150, 250, 65, 25 };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<FoodAnalysticResponseDto> GetFoodAnalysticAsync(FoodAnalysticRequestDto? request,
            CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                if (request?.Image == null)
                {
                    throw new ArgumentNullException(nameof(request.Image), "Hình ảnh không được để trống");
                }

                if (request.Image.Length == 0)
                {
                    throw new ArgumentException("Hình ảnh không được rỗng", nameof(request.Image));
                }


                var imageUrl = await _imageService.SaveImageAsync(request.Image);

                var systemPrompt = CreateFoodAnalysisSystemPrompt();
                var userPrompt = CreateFoodAnalysisUserPrompt(imageUrl);

                var response = await CallOpenAIWithImageAsync(systemPrompt, userPrompt, imageUrl, cancellationToken);

                var foodAnalysis = ParseFoodAnalysisResponse(response);

                foodAnalysis.ImageUrl = imageUrl;

                _logger.LogInformation("Successfully analyzed food image: {ImageUrl}", imageUrl);

                return foodAnalysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing food image");
                throw new HttpRequestException("Không thể phân tích hình ảnh, vui lòng chụp hoặc upload ảnh khác", ex);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IngredientAnalysticResponseDto> GetIngredientAnalysticAsync(
            IngredientAnalysticRequestDto? request, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                if (request?.Image == null)
                {
                    throw new ArgumentNullException(nameof(request.Image), "Hình ảnh không được để trống");
                }

                if (request.Image.Length == 0)
                {
                    throw new ArgumentException("Hình ảnh không được rỗng", nameof(request.Image));
                }
                
                
                var imageUrl = await _imageService.SaveImageAsync(request.Image);

                var systemPrompt = CreateIngredientAnalysisSystemPrompt();
                var userPrompt = CreateIngredientAnalysisUserPrompt(imageUrl);

                var response = await CallOpenAIWithImageAsync(systemPrompt, userPrompt, imageUrl, cancellationToken);

                var ingredientAnalysis = ParseIngredientAnalysisResponse(response);

                ingredientAnalysis.ImageUrl = imageUrl;

                _logger.LogInformation("Successfully analyzed ingredient image: {ImageUrl}", imageUrl);

                return ingredientAnalysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing ingredient image");
                throw new HttpRequestException("Không thể phân tích hình ảnh, vui lòng chụp hoặc upload ảnh khác", ex);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        private class TextChatMessageContentItem(string content) : ChatMessageContentItem
        {
            public string Type { get; } = "text";
            public string Content { get; } = content;
        }

        private class ImageChatMessageContentItem(string content) : ChatMessageContentItem
        {
            public string Type { get; } = "image";
            public string Content { get; } = content;
        }

        private async Task<string> CallOpenAIWithImageAsync(string systemPrompt, string userPrompt, string imageUrl,
            CancellationToken cancellationToken)
        {
            var options = new ChatCompletionsOptions
            {
                Model = _model,
                Messages =
                {
                    new ChatRequestSystemMessage(systemPrompt),
                    new ChatRequestSystemMessage(systemPrompt),
                    new ChatRequestUserMessage(new TextChatMessageContentItem(userPrompt)),
                    new ChatRequestUserMessage(new ImageChatMessageContentItem(imageUrl))
                },
                MaxTokens = 2000,
                Temperature = 0.3f,
                FrequencyPenalty = 0.1f,
                PresencePenalty = 0.1f
            };

            var response = await _chatClient.CompleteAsync(options, cancellationToken);

            if (response?.Value?.Content == null)
            {
                throw new InvalidOperationException("Không thể phân tích hình ảnh, vui lòng chụp hoặc upload ảnh khác");
            }

            return response.Value.Content;
        }
        
        private string CreateFoodAnalysisSystemPrompt()
        {
            return @"Bạn là một chuyên gia dinh dưỡng và đầu bếp chuyên nghiệp với khả năng phân tích hình ảnh món ăn. Nhiệm vụ của bạn là phân tích hình ảnh và nhận diện món ăn một cách chính xác, chỉ tập trung vào các món ăn phổ biến, được biết đến rộng rãi (ví dụ: phở, bánh mì, cơm tấm, salad gà, pasta). KHÔNG tạo ra hoặc gợi ý món ăn không có thật hoặc không phổ biến.

YÊU CẦU PHÂN TÍCH:
1. Nhận diện chính xác tên món ăn (chỉ các món phổ biến, có thật).
2. Mô tả chi tiết món ăn (nguyên liệu chính, cách trình bày).
3. Ước tính thời gian chuẩn bị và nấu nướng.
4. Tính toán thông tin dinh dưỡng (calories, protein, carbs, fat, fiber) với độ chính xác cao, dựa trên cơ sở khoa học.
5. Tạo hướng dẫn nấu nướng từng bước chi tiết.
6. Đưa ra tips hữu ích để nấu và bảo quản món ăn.
7. Đánh giá độ khó (1-5, 1=dễ, 5=khó).
8. Xác định loại bữa ăn phù hợp.
9. Phân tích nguyên liệu chính có trong món ăn.
10. Kiểm tra tính hợp lệ của hình ảnh: nếu hình ảnh không rõ, không chứa món ăn, hoặc có vấn đề (quá tối, mờ, không nhận diện được), trả về JSON với trường ""error"": ""Không thể phân tích hình ảnh, vui lòng chụp hoặc upload ảnh khác"".

LOẠI BỮA ĂN (MealType) - SỬ DỤNG SỐ NGUYÊN:
0=Breakfast, 1=Lunch, 2=Dinner, 3=Snack

ĐƠN VỊ NGUYÊN LIỆU (IngredientUnit) - SỬ DỤNG SỐ NGUYÊN:
0=Kilogram, 1=Liter, 2=Piece, 3=Box, 4=Gram, 5=Milliliter,
6=Can, 7=Cup, 8=Tablespoon, 9=Teaspoon, 10=Package, 11=Bottle, 12=Other

DANH MỤC NGUYÊN LIỆU (IngredientCategory) - SỬ DỤNG SỐ NGUYÊN:
0=Vegetables, 1=Fruits, 2=Meat, 3=Dairy, 4=Grains, 5=Spices, 6=Other

Trả về kết quả dưới dạng JSON với format sau:
{
  ""error"": ""Không thể phân tích hình ảnh, vui lòng chụp hoặc upload ảnh khác"", // Chỉ trả về nếu hình ảnh không hợp lệ
  ""name"": ""Tên món ăn"",
  ""description"": ""Mô tả chi tiết món ăn"",
  ""preparationTimeMinutes"": 15,
  ""cookingTimeMinutes"": 30,
  ""calories"": 350.0,
  ""protein"": 25.0,
  ""carbohydrates"": 45.0,
  ""fat"": 12.0,
  ""fiber"": 5.0,
  ""instructions"": [
    ""Bước 1: Chuẩn bị nguyên liệu"",
    ""Bước 2: Xử lý nguyên liệu"",
    ""Bước 3: Nấu nướng""
  ],
  ""tips"": [
    ""Tip 1: Lưu ý về nhiệt độ"",
    ""Tip 2: Cách bảo quản""
  ],
  ""difficultyLevel"": 2,
  ""mealType"": 1,
  ""ingredients"": [
    {
      ""ingredientId"": 0,
      ""name"": ""Tên nguyên liệu"",
      ""quantity"": 100.0,
      ""unit"": 4,
      ""category"": 0
    }
  ]
}

LƯU Ý:
- Tất cả số liệu phải là số thập phân (decimal).
- difficultyLevel: 1-5 (1=Dễ, 5=Khó).
- instructions và tips phải là mảng string.
- Chỉ trả về JSON object, KHÔNG kèm text giải thích.
- Nếu không thể nhận diện món ăn, trả về JSON với trường ""error"" như trên.";
        }

        private string CreateIngredientAnalysisSystemPrompt()
        {
            return @"Bạn là một chuyên gia dinh dưỡng với khả năng phân tích hình ảnh nguyên liệu thực phẩm. Nhiệm vụ của bạn là nhận diện nguyên liệu chính trong hình ảnh một cách chính xác, chỉ tập trung vào các nguyên liệu phổ biến, có thật (ví dụ: cà chua, thịt bò, gạo, sữa). KHÔNG tạo ra hoặc nhận diện nguyên liệu không có thật hoặc không phổ biến.

YÊU CẦU PHÂN TÍCH:
1. Nhận diện nguyên liệu chính trong hình ảnh.
2. Mô tả chi tiết nguyên liệu (tình trạng, màu sắc, đặc điểm).
3. Ước tính số lượng/khối lượng chính xác dựa trên hình ảnh.
4. Xác định đơn vị tính và danh mục phù hợp.
5. Ước tính hạn sử dụng dựa trên tình trạng hiện tại.
6. Kiểm tra tính hợp lệ của hình ảnh: nếu hình ảnh không rõ, không chứa nguyên liệu, hoặc có vấn vấn đề (quá tối, mờ, không nhận diện được), trả về JSON với trường ""error"": ""Không thể phân tích hình ảnh, vui lòng chụp hoặc upload ảnh khác"".

ĐƠN VỊ NGUYÊN LIỆU (IngredientUnit) - SỬ DỤNG SỐ NGUYÊN:
0=Kilogram, 1=Liter, 2=Piece, 3=Box, 4=Gram, 5=Milliliter,
6=Can, 7=Cup, 8=Tablespoon, 9=Teaspoon, 10=Package, 11=Bottle, 12=Other

DANH MỤC NGUYÊN LIỆU (IngredientCategory) - SỬ DỤNG SỐ NGUYÊN:
0=Vegetables, 1=Fruits, 2=Meat, 3=Dairy, 4=Grains, 5=Spices, 6=Other

Trả về kết quả dưới dạng JSON với format sau:
{
  ""error"": ""Không thể phân tích hình ảnh, vui lòng chụp hoặc upload ảnh khác"", // Chỉ trả về nếu hình ảnh không hợp lệ
  ""name"": ""Tên nguyên liệu chính"",
  ""description"": ""Mô tả chi tiết nguyên liệu"",
  ""quantity"": 500.0,
  ""unit"": 4,
  ""category"": 0,
  ""expiryDate"": ""2024-12-31T23:59:59Z""
}

LƯU Ý:
- Tất cả số liệu phải là số thập phân (decimal).
- expiryDate phải ở format ISO 8601 (UTC).
- Chỉ trả về JSON object, KHÔNG kèm text giải thích.
- Nếu không thể nhận diện nguyên liệu, trả về JSON với trường ""error"" như trên.";
        }

        private string CreateFoodAnalysisUserPrompt(string imageUrl)
        {
            return $@"Hãy phân tích hình ảnh món ăn sau và cung cấp thông tin chi tiết:

IMAGE URL: {imageUrl}

YÊU CẦU PHÂN TÍCH:
1. Nhận diện tên món ăn (chỉ các món phổ biến, có thật như phở, bánh mì, salad gà, v.v.).
2. Mô tả chi tiết món ăn (nguyên liệu chính, cách trình bày).
3. Ước tính thời gian chuẩn bị và nấu nướng.
4. Tính toán thông tin dinh dưỡng (calories, protein, carbs, fat, fiber) với độ chính xác cao.
5. Tạo hướng dẫn nấu nướng từng bước chi tiết.
6. Đưa ra tips hữu ích để nấu và bảo quản món ăn.
7. Đánh giá độ khó của món ăn (1-5).
8. Xác định loại bữa ăn phù hợp (0=Breakfast, 1=Lunch, 2=Dinner, 3=Snack).
9. Phân tích nguyên liệu chính có trong món ăn.
10. Nếu hình ảnh không rõ, không chứa món ăn, hoặc có vấn đề (quá tối, mờ), trả về JSON với trường ""error"": ""Không thể phân tích hình ảnh, vui lòng chụp hoặc upload ảnh khác"".

Hãy quan sát kỹ hình ảnh và đưa ra phân tích chính xác nhất có thể.";
        }

        private string CreateIngredientAnalysisUserPrompt(string imageUrl)
        {
            return $@"Hãy phân tích hình ảnh nguyên liệu thực phẩm sau và cung cấp thông tin chi tiết:

IMAGE URL: {imageUrl}

YÊU CẦU PHÂN TÍCH:
1. Nhận diện nguyên liệu chính (chỉ các nguyên liệu phổ biến, có thật như cà chua, thịt bò, gạo, v.v.).
2. Mô tả chi tiết nguyên liệu (tình trạng, màu sắc, đặc điểm).
3. Ước tính số lượng/khối lượng chính xác dựa trên hình ảnh.
4. Xác định đơn vị tính và danh mục phù hợp.
5. Ước tính hạn sử dụng dựa trên tình trạng hiện tại.
6. Nếu hình ảnh không rõ, không chứa nguyên liệu, hoặc có vấn đề (quá tối, mờ), trả về JSON với trường ""error"": ""Không thể phân tích hình ảnh, vui lòng chụp hoặc upload ảnh khác"".

Hãy quan sát kỹ hình ảnh và đưa ra phân tích chính xác nhất có thể.";
        }

        private FoodAnalysticResponseDto ParseFoodAnalysisResponse(string jsonResponse)
        {
            try
            {
                var jsonStart = jsonResponse.IndexOf('{');
                var jsonEnd = jsonResponse.LastIndexOf('}');

                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonContent = jsonResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);

                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        PropertyNameCaseInsensitive = true
                    };

                    var result = JsonSerializer.Deserialize<FoodAnalysticResponseDto>(jsonContent, options);

                    if (result != null)
                    {
                        result.Instructions ??= new List<string>();
                        result.Tips ??= new List<string>();
                        result.Ingredients ??= new List<FoodIngredientDto>();

                        if (result.DifficultyLevel < 1 || result.DifficultyLevel > 5)
                            result.DifficultyLevel = 1;

                        result.NormalizeConsumedAt();

                        return result;
                    }
                }

                _logger.LogWarning("Failed to parse food analysis response: {Response}", jsonResponse);
                throw new InvalidOperationException("Không thể phân tích hình ảnh, vui lòng chụp hoặc upload ảnh khác");
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse food analysis JSON: {Response}", jsonResponse);
                throw new InvalidOperationException("Không thể phân tích hình ảnh, vui lòng chụp hoặc upload ảnh khác");
            }
        }

        private IngredientAnalysticResponseDto ParseIngredientAnalysisResponse(string jsonResponse)
        {
            try
            {
                var jsonStart = jsonResponse.IndexOf('{');
                var jsonEnd = jsonResponse.LastIndexOf('}');

                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonContent = jsonResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);

                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        PropertyNameCaseInsensitive = true
                    };

                    var result = JsonSerializer.Deserialize<IngredientAnalysticResponseDto>(jsonContent, options);

                    if (result != null)
                    {
                        result.NormalizeExpiryDate();
                        return result;
                    }
                }

                _logger.LogWarning("Failed to parse ingredient analysis response: {Response}", jsonResponse);
                throw new InvalidOperationException("Không thể phân tích hình ảnh, vui lòng chụp hoặc upload ảnh khác");
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse ingredient analysis JSON: {Response}", jsonResponse);
                throw new InvalidOperationException("Không thể phân tích hình ảnh, vui lòng chụp hoặc upload ảnh khác");
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

        private List<int> ParseNutritionTargets(string jsonResponse)
        {
            try
            {
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
                        if (targets[0] >= 1000 && targets[0] <= 5000 &&
                            targets[1] >= 50 && targets[1] <= 400 &&
                            targets[2] >= 100 && targets[2] <= 600 &&
                            targets[3] >= 30 && targets[3] <= 200 &&
                            targets[4] >= 15 && targets[4] <= 80)
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
            return @"Bạn là một chuyên gia dinh dưỡng và đầu bếp chuyên nghiệp. Nhiệm vụ của bạn là gợi ý các món ăn PHỔ BIẾN, được biết đến rộng rãi (ví dụ: phở, bánh mì, cơm tấm, salad gà, pasta) dựa trên thông tin người dùng và danh sách nguyên liệu được cung cấp. KHÔNG gợi ý món ăn không có thật hoặc không phổ biến.

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
- Chỉ gợi ý món ăn phổ biến, được biết đến rộng rãi.
- kcal phải là số thập phân (decimal).
- quantity phải là số thập phân (decimal).
- Tất cả field names phải chính xác như trên (case-sensitive).
- Chỉ trả về JSON array, KHÔNG kèm text giải thích.";
        }

        private string CreateFoodSuggestionUserPrompt(FoodSuggestionRequestDto requestDto,
            List<FoodIngredientDto> ingredients)
        {
            var userInfo = requestDto.UserInformation;
            var prompt = $"Gợi ý {requestDto.MaxSuggestions} món ăn PHỔ BIẾN, được biết đến rộng rãi (ví dụ: phở, bánh mì, cơm tấm, salad gà, pasta) phù hợp cho người dùng:\n\n";

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
                "- Chỉ gợi ý món ăn phổ biến, được biết đến rộng rãi.\n";
            prompt += "- Khi sử dụng nguyên liệu từ danh sách trên: PHẢI giữ CHÍNH XÁC ingredientId, ingredientName, unit\n";
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
            return @"Bạn là một đầu bếp chuyên nghiệp với nhiều năm kinh nghiệm. Nhiệm vụ của bạn là cung cấp công thức nấu ăn chi tiết và chính xác cho các món ăn PHỔ BIẾN, được biết đến rộng rãi (ví dụ: phở, bánh mì, cơm tấm, salad gà, pasta). KHÔNG tạo ra hoặc cung cấp công thức cho món ăn không có thật hoặc không phổ biến.

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
- Chỉ cung cấp công thức cho món ăn phổ biến, được biết đến rộng rãi.
- Tất cả số liệu dinh dưỡng (calories, protein, carbohydrates, fat, fiber) phải là số thập phân (decimal).
- quantity cũng phải là số thập phân.
- mealDate phải có format ISO 8601 với UTC timezone.
- id luôn đặt = 0 (sẽ được generate ở server).
- Chỉ trả về JSON object, KHÔNG kèm text giải thích.";
        }

        private string CreateRecipeUserPrompt(FoodRecipeRequestDto recipeRequest)
        {
            var prompt = $"Tạo công thức nấu ăn chi tiết cho món PHỔ BIẾN: \"{recipeRequest.FoodName}\" (ví dụ: phở, bánh mì, cơm tấm, salad gà, pasta). KHÔNG tạo công thức cho món không có thật hoặc không phổ biến.\n\n";

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

                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        PropertyNameCaseInsensitive = true
                    };

                    var suggestions =
                        JsonSerializer.Deserialize<List<FoodSuggestionResponseDto>>(jsonContent, options) ?? new();

                    foreach (var suggestion in suggestions)
                    {
                        if (suggestion?.Ingredients == null) continue;

                        suggestion.Ingredients ??= new List<FoodIngredientDto>();

                        foreach (var ingredient in suggestion.Ingredients)
                        {
                            if (ingredient.IngredientId <= 0) continue;

                            var requestIngredient =
                                ingredients.FirstOrDefault(i => i.IngredientId == ingredient.IngredientId);
                            if (requestIngredient == null) continue;

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
                var jsonStart = jsonResponse.IndexOf('{');
                var jsonEnd = jsonResponse.LastIndexOf('}');

                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonContent = jsonResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);

                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        PropertyNameCaseInsensitive = true
                    };

                    var recipe = JsonSerializer.Deserialize<FoodDataResponseDto>(jsonContent, options);

                    if (recipe != null)
                    {
                        recipe.Instructions ??= new List<string>();
                        recipe.Tips ??= new List<string>();
                        recipe.Ingredients ??= new List<FoodIngredientDto>();

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