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

    public Task<List<int>> GetTargetDailyNutritionAsync(UserInformationDto userInformation, CancellationToken cancellationToken = default)
    {
        //Todo: Implement logic to calculate daily nutrition targets based on user information
        throw new NotImplementedException();
    }

    public Task<List<int>> GetTargetWeeklyNutritionAsync(UserInformationDto userInformation, CancellationToken cancellationToken = default)
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

    private async Task<string> CallOpenAIAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
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
Nếu món ăn sử dụng nguyên liệu từ danh sách nguyên liệu của người dùng, hãy bao gồm ingredientId tương ứng và đảm bảo số lượng (quantity) không vượt quá số lượng tối đa được cung cấp. 
Các nguyên liệu bổ sung không cần ingredientId.
Trả về kết quả dưới dạng JSON array với format sau:
[
  {
    ""name"": ""Tên món ăn"",
    ""image"": ""URL hình ảnh (có thể để trống)"",
    ""difficulty"": 5, // Độ khó từ 1 đến 5
    ""kcal"": 15,  // Tổng số calo cho một phần ăn chuyển ra đơn vị kcal
    ""prepTimeMinutes"": 15,
    ""cookTimeMinutes"": 30,
    ""ingredients"": [
      {
        ""ingredientId"": 123, // Bao gồm nếu nguyên liệu khớp với danh sách nguyên liệu của người dùng, nếu không hãy để giá trị 0
        ""name"": ""Tên nguyên liệu"",
        ""quantity"": 1, // Đảm bảo không vượt quá số lượng tối đa trong yêu cầu
        ""unit"": 0 // Hãy đảm bảo giá trị sẽ tương ứng với thứ tự các đơn vị từ 0-12 như sau: Kilogram, Liter, Piece, Box, Gram, Milliliter, Can, Cup, Tablespoon, Teaspoon, Package, Bottle, hoặc giá trị khác: Other. Đơn vị phải là số nguyên, không được trả về dạng string.
      }
    ]
  }
]";
    }

    private string CreateFoodSuggestionUserPrompt(FoodSuggestionRequestDto requestDto)
    {
        var userInfo = requestDto.UserInformation;
        var prompt = $"Gợi ý {requestDto.MaxSuggestions} món ăn phù hợp cho người dùng với thông tin sau:\n";

        if (userInfo?.Gender.HasValue == true)
            prompt += $"- Giới tính: {userInfo.Gender}\n";

        if (userInfo?.DateOfBirth.HasValue == true)
        {
            var age = DateTime.Now.Year - userInfo.DateOfBirth.Value.Year;
            prompt += $"- Tuổi: {age}\n";
        }

        if (userInfo?.Height.HasValue == true)
            prompt += $"- Chiều cao: {userInfo.Height}cm\n";

        if (userInfo?.Weight.HasValue == true)
            prompt += $"- Cân nặng hiện tại: {userInfo.Weight}kg\n";

        if (userInfo?.TargetWeight.HasValue == true)
            prompt += $"- Cân nặng mục tiêu: {userInfo.TargetWeight}kg\n";

        if (userInfo?.PrimaryNutritionGoal.HasValue == true)
        prompt += $"- Mục tiêu dinh dưỡng: {userInfo.PrimaryNutritionGoal}\n";

        if (userInfo?.ActivityLevel.HasValue == true)
            prompt += $"- Mức độ hoạt động: {userInfo.ActivityLevel}\n";

        if (requestDto.Ingredients?.Any() == true)
        {
            prompt += "\nSử dụng các nguyên liệu sau nếu có thể (không vượt quá số lượng tối đa):\n";
            foreach (var ingredient in requestDto.Ingredients)
            {
                prompt += $"- {ingredient.IngredientName} (ID: {ingredient.IngredientId}, Tối đa: {ingredient.Quantity} {ingredient.Unit})\n";
            }
        }

        prompt += "\nHãy đưa ra các món ăn phù hợp với mục tiêu sức khỏe, dinh dưỡng của người dùng và danh sách nguyên liệu được cung cấp. Đảm bảo bao gồm ingredientId trong kết quả nếu nguyên liệu khớp với danh sách nguyên liệu của người dùng.";
    
        return prompt;
    }

    private string CreateRecipeSystemPrompt()
    {
        return @"Bạn là một đầu bếp chuyên nghiệp. 
Nhiệm vụ của bạn là  cung cấp công thức nấu ăn chi tiết dựa trên danh sách nguyên liệu được cung cấp.
Nếu món ăn sử dụng nguyên liệu được cung cấp hãy bao gồm ingredientId tương ứng và đảm bảo số lượng (quantity) không vượt quá số lượng tối đa được cung cấp. 
Các nguyên liệu bổ sung không cần ingredientId.
Trả về kết quả dưới dạng JSON với format sau:
{
  ""name"": ""Tên món ăn"",
  ""description"": ""Mô tả món ăn"",
  ""prepTimeMinutes"": 15,
  ""cookTimeMinutes"": 30,
  ""servings"": 4,
  ""difficulty"": 5, // Độ khó từ 1 đến 5
  ""ingredients"": [
    {
      ""ingredientId"": 123, // Bao gồm nếu nguyên liệu là nguyên liệu được cung cấp và = 0 nếu là nguyên liệu bổ sung
      ""name"": ""Tên nguyên liệu"",
      ""name"": ""Tên nguyên liệu"",
      ""quantity"": 1 // Số lượng nguyên liệu , Đảm bảo không vượt quá số lượng tối đa trong yêu cầu
      ""unit"": // Hãy đảm bảo giá trị sẽ tương ứng với thứ tự các đơn vị từ 0-12 như sau: Kilogram, Liter, Piece, Box, Gram, Milliliter, Can, Cup, Tablespoon, Teaspoon, Package, Bottle, hoặc giá trị khác: Other. Đơn vị phải là số nguyên, không được trả về dạng string.
    }
  ],
  ""instructions"": [
    ""Bước 1: Mô tả chi tiết"",
    ""Bước 2: Mô tả chi tiết""
  ],
  ""nutritionInfo"": {
    ""calories"": 250,
    ""protein"": 15,
    ""carbs"": 30,
    ""fat"": 8
  }
}";
    }

    private string CreateRecipeUserPrompt(FoodRecipeRequestDto recipeRequest)
    {
        var prompt = $"Cung cấp công thức nấu ăn chi tiết cho món: {recipeRequest.FoodName}\n";
        
        if (recipeRequest.Ingredients?.Any() == true)
        {
            prompt += "Sử dụng các nguyên liệu sau nếu có thể:\n";
            foreach (var ingredient in recipeRequest.Ingredients)
            {
                prompt += $"- {ingredient.IngredientName} ({ingredient.Quantity} {ingredient.Unit})\n";
            }
        }
        
        prompt += "\nHãy cung cấp hướng dẫn nấu ăn từng bước một cách chi tiết và dễ hiểu.";
        
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
                var suggestions = JsonSerializer.Deserialize<List<FoodSuggestionDto>>(jsonContent, _jsonOptions) ?? new();

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
                        if (requestIngredient == null || ingredient.Quantity <= requestIngredient.Quantity) continue;
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