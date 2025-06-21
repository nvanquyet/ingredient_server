using IngredientServer.Core.Entities;
using IngredientServer.Utils.DTOs.Auth;
using IngredientServer.Utils.DTOs.Common;

namespace IngredientServer.Core.Interfaces.Services;

public interface IAuthService
{
    Task<ResponseDto<AuthResponseDto>> LoginAsync(LoginDto loginDto);
    Task<ResponseDto<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<ResponseDto<bool>> LogoutAsync(int userId);
    
    Task<ResponseDto<User>> GetUserProfileAsync(int userId);
    Task<ResponseDto<bool>> UpdateUserProfileAsync(int userId, UpdateUserProfileDto? updateUserProfileDto);
}