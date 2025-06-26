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
    Task<ResponseDto<User>> UpdateUserProfileAsync(int userId, UpdateUserProfileDto? updateUserProfileDto);
    
    Task<ResponseDto<bool>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
}