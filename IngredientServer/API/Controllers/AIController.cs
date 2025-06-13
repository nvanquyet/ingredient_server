// // AIController.cs
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Authorization;
// using IngredientServer.Core.Interfaces.Services;
// using IngredientServer.Core.DTOs;
// using IngredientServer.Core.Entities;
// using System.Security.Claims;
// using IngredientServer.Core.Interfaces.Repositories;
//
// namespace IngredientServer.API.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize]
//     public class AIController : BaseController
//     {
//         private readonly IAIService _aiService;
//         private readonly IIngredientRepository _ingredientService;
//         private readonly IFoodService _foodService;
//
//         public AIController(
//             IAIService aiService,
//             IIngredientRepository ingredientService,
//             IFoodService foodService)
//         {
//             _aiService = aiService;
//             _ingredientService = ingredientService;
//             _foodService = foodService;
//         }
//
//         /// <summary>
//         /// Gợi ý món ăn dựa trên nguyên liệu có sẵn
//         /// </summary>
//         [HttpPost("food-suggestions")]
//         public async Task<ActionResult<ApiResponse<List<FoodSuggestion>>>> GetFoodSuggestions(
//             [FromBody] FoodSuggestionRequest request)
//         {
//             try
//             {
//                 var userId = GetCurrentUserId();
//                 
//                 // Lấy nguyên liệu của user
//                 List<Ingredient> ingredients;
//                 
//                 if (request.IngredientIds?.Any() == true)
//                 {
//                     // Lấy nguyên liệu theo IDs được chỉ định
//                     ingredients = await _ingredientService.GetIngredientsByIdsAsync(request.IngredientIds, userId);
//                 }
//                 else
//                 {
//                     // Lấy tất cả nguyên liệu của user
//                     ingredients = await _ingredientService.GetUserIngredientsAsync(userId);
//                 }
//
//                 if (!ingredients.Any())
//                 {
//                     return BadRequest(new ApiResponse<List<FoodSuggestion>>
//                     {
//                         Success = false,
//                         Message = "Không tìm thấy nguyên liệu nào"
//                     });
//                 }
//
//                 var suggestions = await _aiService.GetFoodSuggestionsAsync(
//                     ingredients,
//                     request.NutritionGoal,
//                     request.MaxSuggestions);
//
//                 return Ok(new ApiResponse<List<FoodSuggestion>>
//                 {
//                     Success = true,
//                     Data = suggestions,
//                     Message = $"Tìm thấy {suggestions.Count} gợi ý món ăn"
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return HandleException(ex);
//             }
//         }
//
//         /// <summary>
//         /// Tạo công thức nấu ăn chi tiết
//         /// </summary>
//         [HttpPost("generate-recipe")]
//         public async Task<ActionResult<ApiResponse<DetailedRecipe>>> GenerateRecipe(
//             [FromBody] GenerateRecipeRequest request)
//         {
//             try
//             {
//                 var userId = GetCurrentUserId();
//
//                 // Lấy nguyên liệu theo IDs
//                 var ingredients = await _ingredientService.GetIngredientsByIdsAsync(request.IngredientIds, userId);
//
//                 if (!ingredients.Any())
//                 {
//                     return BadRequest(new ApiResponse<DetailedRecipe>
//                     {
//                         Success = false,
//                         Message = "Không tìm thấy nguyên liệu nào"
//                     });
//                 }
//
//                 var recipe = await _aiService.GenerateRecipeAsync(
//                     request.FoodName,
//                     ingredients,
//                     request.NutritionGoal);
//
//                 return Ok(new ApiResponse<DetailedRecipe>
//                 {
//                     Success = true,
//                     Data = recipe,
//                     Message = "Tạo công thức thành công"
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return HandleException(ex);
//             }
//         }
//
//         /// <summary>
//         /// Phân tích dinh dưỡng của món ăn
//         /// </summary>
//         [HttpPost("analyze-nutrition/{foodId}")]
//         public async Task<ActionResult<ApiResponse<NutritionAnalysis>>> AnalyzeNutrition(int foodId)
//         {
//             try
//             {
//                 var userId = GetCurrentUserId();
//                 var food = await _foodService.GetFoodByIdAsync(foodId, userId);
//
//                 if (food == null)
//                 {
//                     return NotFound(new ApiResponse<NutritionAnalysis>
//                     {
//                         Success = false,
//                         Message = "Không tìm thấy món ăn"
//                     });
//                 }
//
//                 var analysis = await _aiService.AnalyzeNutritionAsync(food);
//
//                 return Ok(new ApiResponse<NutritionAnalysis>
//                 {
//                     Success = true,
//                     Data = analysis,
//                     Message = "Phân tích dinh dưỡng thành công"
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return HandleException(ex);
//             }
//         }
//
//         /// <summary>
//         /// Gợi ý thay thế nguyên liệu
//         /// </summary>
//         [HttpPost("ingredient-substitutions")]
//         public async Task<ActionResult<ApiResponse<List<IngredientSubstitution>>>> GetIngredientSubstitutions(
//             [FromBody] IngredientSubstitutionRequest request)
//         {
//             try
//             {
//                 var substitutions = await _aiService.GetIngredientSubstitutionsAsync(
//                     request.OriginalIngredient,
//                     request.NutritionGoal);
//
//                 return Ok(new ApiResponse<List<IngredientSubstitution>>
//                 {
//                     Success = true,
//                     Data = substitutions,
//                     Message = $"Tìm thấy {substitutions.Count} lựa chọn thay thế"
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return HandleException(ex);
//             }
//         }
//
//         /// <summary>
//         /// Chat với AI về nấu ăn và dinh dưỡng
//         /// </summary>
//         [HttpPost("chat")]
//         public async Task<ActionResult<ApiResponse<string>>> ChatWithAI([FromBody] ChatRequest request)
//         {
//             try
//             {
//                 var response = await _aiService.GetChatResponseAsync(request.Message);
//
//                 return Ok(new ApiResponse<string>
//                 {
//                     Success = true,
//                     Data = response,
//                     Message = "Chat thành công"
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return HandleException(ex);
//             }
//         }
//
//         /// <summary>
//         /// Gợi ý món ăn cho nguyên liệu sắp hết hạn
//         /// </summary>
//         [HttpGet("suggestions-for-expiring-ingredients")]
//         public async Task<ActionResult<ApiResponse<List<FoodSuggestion>>>> GetSuggestionsForExpiringIngredients(
//             [FromQuery] int daysUntilExpiry = 3,
//             [FromQuery] NutritionGoal nutritionGoal = NutritionGoal.Balanced,
//             [FromQuery] int maxSuggestions = 5)
//         {
//             try
//             {
//                 var userId = GetCurrentUserId();
//                 var expiringIngredients = await _ingredientService.GetExpiringIngredientsAsync(userId, daysUntilExpiry);
//
//                 if (!expiringIngredients.Any())
//                 {
//                     return Ok(new ApiResponse<List<FoodSuggestion>>
//                     {
//                         Success = true,
//                         Data = new List<FoodSuggestion>(),
//                         Message = "Không có nguyên liệu nào sắp hết hạn"
//                     });
//                 }
//
//                 var suggestions = await _aiService.GetFoodSuggestionsAsync(
//                     expiringIngredients,
//                     nutritionGoal,
//                     maxSuggestions);
//
//                 return Ok(new ApiResponse<List<FoodSuggestion>>
//                 {
//                     Success = true,
//                     Data = suggestions,
//                     Message = $"Gợi ý {suggestions.Count} món ăn cho nguyên liệu sắp hết hạn"
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return HandleException(ex);
//             }
//         }
//
//         /// <summary>
//         /// Tạo thực đơn hàng tuần
//         /// </summary>
//         [HttpPost("weekly-meal-plan")]
//         public async Task<ActionResult<ApiResponse<WeeklyMealPlan>>> GenerateWeeklyMealPlan(
//             [FromBody] WeeklyMealPlanRequest request)
//         {
//             try
//             {
//                 var userId = GetCurrentUserId();
//                 var availableIngredients = await _ingredientService.GetUserIngredientsAsync(userId);
//
//                 // Tạo prompt cho kế hoạch bữa ăn hàng tuần
//                 var mealPlan = await GenerateWeeklyMealPlanInternal(availableIngredients, request);
//
//                 return Ok(new ApiResponse<WeeklyMealPlan>
//                 {
//                     Success = true,
//                     Data = mealPlan,
//                     Message = "Tạo thực đơn tuần thành công"
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return HandleException(ex);
//             }
//         }
//
//         /// <summary>
//         /// Đánh giá độ tươi ngon của nguyên liệu dựa trên hình ảnh
//         /// </summary>
//         [HttpPost("assess-ingredient-freshness")]
//         public async Task<ActionResult<ApiResponse<IngredientFreshnessAssessment>>> AssessIngredientFreshness(
//             [FromBody] FreshnessAssessmentRequest request)
//         {
//             try
//             {
//                 // Placeholder cho tính năng phân tích hình ảnh
//                 // Cần tích hợp với Computer Vision API
//                 var assessment = new IngredientFreshnessAssessment
//                 {
//                     IngredientName = request.IngredientName,
//                     FreshnessScore = 85, // Placeholder
//                     FreshnessLevel = "Good",
//                     EstimatedShelfLife = 5,
//                     StorageTips = new List<string> { "Bảo quản trong tủ lạnh", "Sử dụng trong 5 ngày" },
//                     QualityIndicators = new List<string> { "Màu sắc tươi", "Không có vết thâm" }
//                 };
//
//                 return Ok(new ApiResponse<IngredientFreshnessAssessment>
//                 {
//                     Success = true,
//                     Data = assessment,
//                     Message = "Đánh giá độ tươi ngon thành công"
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return HandleException(ex);
//             }
//         }
//
//         private async Task<WeeklyMealPlan> GenerateWeeklyMealPlanInternal(
//             List<Ingredient> availableIngredients, 
//             WeeklyMealPlanRequest request)
//         {
//             var ingredientsList = string.Join(", ", availableIngredients.Select(i => 
//                 $"{i.Name} ({i.Quantity} {i.Unit})"));
//
//             var prompt = $@"
// Tạo thực đơn {request.DaysCount} ngày với:
// - Nguyên liệu có sẵn: {ingredientsList}
// - Mục tiêu dinh dưỡng: {request.NutritionGoal}
// - Số bữa ăn/ngày: {request.MealsPerDay}
// - Ngân sách: {request.Budget:C}
// - Ràng buộc: {string.Join(", ", request.DietaryRestrictions)}
//
// Tạo thực đơn cân bằng, đa dạng và tối ưu hóa việc sử dụng nguyên liệu.";
//
//             var response = await _aiService.GetChatResponseAsync(prompt);
//             
//             // Parse response và tạo WeeklyMealPlan object
//             // Placeholder implementation
//             return new WeeklyMealPlan
//             {
//                 StartDate = DateTime.Now.Date,
//                 DaysCount = request.DaysCount,
//                 TotalEstimatedCost = request.Budget * 0.8m,
//                 DailyMealPlans = GeneratePlaceholderDailyPlans(request.DaysCount, request.MealsPerDay)
//             };
//         }
//
//         private List<DailyMealPlan> GeneratePlaceholderDailyPlans(int daysCount, int mealsPerDay)
//         {
//             var dailyPlans = new List<DailyMealPlan>();
//             
//             for (int day = 0; day < daysCount; day++)
//             {
//                 var meals = new List<MealPlan>();
//                 var mealTypes = new[] { "Breakfast", "Lunch", "Dinner", "Snack" };
//                 
//                 for (int meal = 0; meal < Math.Min(mealsPerDay, 4); meal++)
//                 {
//                     meals.Add(new MealPlan
//                     {
//                         MealType = mealTypes[meal],
//                         FoodName = $"Món ăn {mealTypes[meal]} ngày {day + 1}",
//                         EstimatedCalories = 400,
//                         PreparationTime = 30,
//                         Ingredients = new List<string> { "Nguyên liệu 1", "Nguyên liệu 2" }
//                     });
//                 }
//                 
//                 dailyPlans.Add(new DailyMealPlan
//                 {
//                     Date = DateTime.Now.Date.AddDays(day),
//                     Meals = meals,
//                     TotalCalories = meals.Sum(m => m.EstimatedCalories),
//                     TotalCost = 50000
//                 });
//             }
//             
//             return dailyPlans;
//         }
//
//         private int GetCurrentUserId()
//         {
//             var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//             return int.TryParse(userIdClaim, out var userId) ? userId : 0;
//         }
//
//         private ActionResult<T> HandleException<T>(Exception ex)
//         {
//             // Log exception
//             return StatusCode(500, new ApiResponse<T>
//             {
//                 Success = false,
//                 Message = "Đã xảy ra lỗi hệ thống"
//             });
//         }
//     }
// }
