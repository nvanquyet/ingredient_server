using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Auth;
using IngredientServer.Utils.DTOs.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace IngredientServer.Core.Services;

public class AuthService(
    IUserRepository userRepository,
    IJwtService jwtService,
    IConfiguration configuration,
    ILogger<AuthService> logger)
    : IAuthService
{
    public async Task<ResponseDto<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        try
        {
            var user = await userRepository.GetByUsernameAsync(loginDto.Username);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return new ResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            if (!user.IsActive)
            {
                return new ResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "Account is deactivated"
                };
            }

            await userRepository.UpdateForLoginAsync(user);

            //Check format create at 
            if (user.CreatedAt == default(DateTime))
            {
                user.CreatedAt = DateTime.UtcNow;
            }
            else if (user.CreatedAt.Kind != DateTimeKind.Utc)
            {
                user.CreatedAt = DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc);
            }

            var token = jwtService.GenerateToken(user);
            var response = new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = UserProfileDto.FromUser(user)
            };

            return new ResponseDto<AuthResponseDto>
            {
                Success = true,
                Message = "Login successful",
                Data = response
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login");
            return new ResponseDto<AuthResponseDto>
            {
                Success = false,
                Message = "An error occurred during login"
            };
        }
    }

    public async Task<ResponseDto<AuthResponseDto>> ValidateTokenAsync(string token)
    {
        try
        {
            logger.LogInformation("Starting token validation process");

            // Parse và validate JWT token
            var principal = jwtService.ValidateToken(token);
            if (principal == null)
            {
                logger.LogWarning("JWT validation failed - token is null or invalid");
                return new ResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "Invalid or expired token"
                };
            }

            // Lấy userId từ token đã được validate
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            logger.LogInformation("Extracted user ID claim: {UserIdClaim}", userIdClaim);

            if (!int.TryParse(userIdClaim, out int userId) || userId <= 0)
            {
                logger.LogWarning("Invalid user ID in token: {UserIdClaim}", userIdClaim);
                return new ResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "Invalid user ID in token"
                };
            }

            logger.LogInformation("Parsed user ID: {UserId}", userId);

            // Kiểm tra user có tồn tại trong database không
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning("User not found in database: {UserId}", userId);
                return new ResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            logger.LogInformation("User found: {Username}", user.Username);

            // Fix DateTime format
            if (user.CreatedAt == default(DateTime))
            {
                user.CreatedAt = DateTime.UtcNow;
                logger.LogInformation("Set default CreatedAt for user {UserId}", userId);
            }
            else if (user.CreatedAt.Kind != DateTimeKind.Utc)
            {
                user.CreatedAt = DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc);
            }

            var response = new AuthResponseDto
            {
                User = UserProfileDto.FromUser(user),
                Token = token // Trả lại token nếu cần
            };

            logger.LogInformation("Token validation successful for user {UserId}", userId);
            return new ResponseDto<AuthResponseDto>
            {
                Success = true,
                Message = "Token is valid",
                Data = response
            };
        }
        catch (SecurityTokenException ex)
        {
            logger.LogWarning(ex, "Security token exception during validation");
            return new ResponseDto<AuthResponseDto>
            {
                Success = false,
                Message = "Invalid token format"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during token validation");
            return new ResponseDto<AuthResponseDto>
            {
                Success = false,
                Message = "An error occurred during token validation"
            };
        }
    }

    public async Task<ResponseDto<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            // Check if user exists
            if (await userRepository.ExistsAsync(registerDto.Username, registerDto.Email))
            {
                return new ResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "Username or email already exists"
                };
            }

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            // Use AddForRegistrationAsync instead of AddAsync to avoid authentication context issues
            await userRepository.AddForRegistrationAsync(user);

            var token = jwtService.GenerateToken(user);
            var response = new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = UserProfileDto.FromUser(user)
            };

            return new ResponseDto<AuthResponseDto>
            {
                Success = true,
                Message = "Registration successful",
                Data = response
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during registration");
            return new ResponseDto<AuthResponseDto>
            {
                Success = false,
                Message = "An error occurred during registration"
            };
        }
    }

    public Task<ResponseDto<bool>> LogoutAsync(int userId)
    {
        try
        {
            // In a stateless JWT authentication system, logout is typically handled on the client side.
            // However, if you want to implement server-side logout, you can invalidate the token or update user status.
            // Here we can just return success as JWT tokens are stateless.

            return Task.FromResult(new ResponseDto<bool>
            {
                Success = true,
                Message = "Logout successful",
                Data = true
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout");
            return Task.FromResult(new ResponseDto<bool>
            {
                Success = false,
                Message = "An error occurred during logout",
                Data = false
            });
        }
    }

    public async Task<ResponseDto<UserProfileDto>> GetUserProfileAsync(int userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return new ResponseDto<UserProfileDto>
            {
                Success = false,
                Message = "User not found",
                Data = null
            };
        }

        var userProfileDto = user.ToDto();
        return new ResponseDto<UserProfileDto>
        {
            Success = true,
            Message = "User profile retrieved successfully",
            Data = userProfileDto
        };
    }

    public async Task<ResponseDto<UserProfileDto>> UpdateUserProfileAsync(int userId,
        UserProfileDto? updateUserProfileDto)
    {
        if (updateUserProfileDto == null)
        {
            return await Task.FromResult(new ResponseDto<UserProfileDto>
            {
                Success = false,
                Message = "Invalid user profile data"
            });
        }

        var user = await userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return await Task.FromResult(new ResponseDto<UserProfileDto>
            {
                Success = false,
                Message = "User not found"
            });
        }

        user.UpdateUserProfile(updateUserProfileDto);
        await userRepository.UpdateAsync(user);
        return await Task.FromResult(new ResponseDto<UserProfileDto>
        {
            Success = true,
            Message = "User profile updated successfully",
            Data = user.ToDto()
        });
    }


    public async Task<ResponseDto<bool>> ChangePasswordAsync(int userId, ChangePasswordDto? changePasswordDto)
    {
        if (changePasswordDto == null)
        {
            return await Task.FromResult(new ResponseDto<bool>
            {
                Success = false,
                Message = "Invalid password change data"
            });
        }

        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return new ResponseDto<bool>
            {
                Success = false,
                Message = "User not found"
            };
        }

        if (string.IsNullOrWhiteSpace(changePasswordDto.CurrentPassword) ||
            string.IsNullOrWhiteSpace(changePasswordDto.NewPassword) ||
            changePasswordDto.NewPassword.Length < 6)
        {
            return new ResponseDto<bool>
            {
                Success = false,
                Message = "Invalid password change request"
            };
        }

        if (changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
        {
            return new ResponseDto<bool>
            {
                Success = false,
                Message = "New password and confirmation do not match"
            };
        }

        if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            return new ResponseDto<bool>
            {
                Success = false,
                Message = "Old password is incorrect"
            };
        }

        user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
        await userRepository.UpdateAsync(user);
        return new ResponseDto<bool>
        {
            Success = true,
            Message = "Password changed successfully",
            Data = true
        };
    }

    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}