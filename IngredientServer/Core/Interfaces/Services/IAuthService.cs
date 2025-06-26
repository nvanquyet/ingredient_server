using IngredientServer.Core.Entities;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Auth;
using IngredientServer.Utils.DTOs.Common;

namespace IngredientServer.Core.Interfaces.Services;

public interface IAuthService
{
    Task<ResponseDto<AuthResponseDto>> LoginAsync(LoginDto loginDto);
    Task<ResponseDto<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<ResponseDto<bool>> LogoutAsync(int userId);
    
    Task<ResponseDto<UserProfileDto>> GetUserProfileAsync(int userId);
    Task<ResponseDto<User>> UpdateUserProfileAsync(int userId, UserProfileDto? updateUserProfileDto);
    
    Task<ResponseDto<bool>> ChangePasswordAsync(int userId, ChangePasswordDto? changePasswordDto);
}