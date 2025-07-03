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

public class AuthService(IUserRepository userRepository, IJwtService jwtService, IConfiguration configuration, ILogger<AuthService> logger)
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

            var token = GenerateJwtToken(user);
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
            // Parse và validate JWT token
            var principal = ValidateToken(token);
            // Lấy userId từ token đã được validate
            if (principal == null)
            {
                return new ResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "Invalid token"
                };
            }
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId) || userId <= 0)
            {
                return new ResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "Invalid user ID in token"
                };
            }

            // Kiểm tra user có tồn tại trong database không
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Fix DateTime format
            if (user.CreatedAt == default(DateTime))
            {
                user.CreatedAt = DateTime.UtcNow;
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

            return new ResponseDto<AuthResponseDto>
            {
                Success = true,
                Message = "Token is valid",
                Data = response
            };
        }
        catch (SecurityTokenException ex)
        {
            logger.LogWarning(ex, "Invalid token format");
            return new ResponseDto<AuthResponseDto>
            {
                Success = false,
                Message = "Invalid token format"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during token validation");
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
                LastName = registerDto.Username,
                FirstName = registerDto.Username,
               
                
                gender = Gender.Male,
                DateOfBirth = DateTime.UtcNow, 
                Height = 160,                 
                Weight = 60,                 
                TargetWeight = 50,           
                PrimaryNutritionGoal = NutritionGoal.Balanced,
                ActivityLevel = ActivityLevel.Sedentary,
                HasFoodAllergies = false,
                FoodAllergies = string.Empty,
                FoodPreferences = string.Empty,
                EnableNotifications = true,
                EnableMealReminders = true
                
            };

            // Use AddForRegistrationAsync instead of AddAsync to avoid authentication context issues
            await userRepository.AddForRegistrationAsync(user);

            var token = GenerateJwtToken(user);
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


    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(configuration["Jwt:Secret"] ?? "");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            ]),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    //Validate Token without service 
    private ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["Jwt:Secret"] ?? "");
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero // No clock skew for immediate expiration
            };

            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch (Exception)
        {
            return null; // Token is invalid
        }
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