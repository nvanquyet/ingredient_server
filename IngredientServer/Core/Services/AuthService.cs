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
    INutritionTargetsService nutritionTargetsService,
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

            user.NormalizeDateTimes();
            await userRepository.UpdateForLoginAsync(user);

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
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token); // ❗ không verify chữ ký

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId) || userId <= 0)
            {
                return new ResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "Invalid user ID in token"
                };
            }

            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            user.NormalizeDateTimes();

            var response = new AuthResponseDto
            {
                User = UserProfileDto.FromUser(user),
                Token = token
            };

            return new ResponseDto<AuthResponseDto>
            {
                Success = true,
                Message = "Token is valid",
                Data = response
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error decoding token");
            return new ResponseDto<AuthResponseDto>
            {
                Success = false,
                Message = "An error occurred while decoding token"
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

            // Normalize DateTime properties
            user.NormalizeDateTimes();

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

        user.NormalizeDateTimes();
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
        logger.LogInformation("Start updating user profile for UserId: {UserId}", userId);

        if (updateUserProfileDto == null)
        {
            logger.LogWarning("Update failed: updateUserProfileDto is null for UserId: {UserId}", userId);
            return await Task.FromResult(new ResponseDto<UserProfileDto>
            {
                Success = false,
                Message = "Invalid user profile data"
            });
        }

        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            logger.LogWarning("Update failed: User not found with UserId: {UserId}", userId);
            return await Task.FromResult(new ResponseDto<UserProfileDto>
            {
                Success = false,
                Message = "User not found"
            });
        }

        if (!string.IsNullOrEmpty(updateUserProfileDto.Username) &&
            updateUserProfileDto.Username != user.Username)
        {
            logger.LogInformation("Checking if username '{Username}' already exists", updateUserProfileDto.Username);

            var existingUser = await userRepository.GetByUsernameAsync(updateUserProfileDto.Username);
            if (existingUser != null)
            {
                logger.LogWarning("Username already exists: {Username}", updateUserProfileDto.Username);
                return new ResponseDto<UserProfileDto>
                {
                    Success = false,
                    Message = "Username already exists"
                };
            }
        }

        logger.LogInformation("Updating user profile for UserId: {UserId}", userId);

        user.UpdateUserProfile(updateUserProfileDto);
        user.NormalizeDateTimes();

        await userRepository.UpdateAsync(user);

        logger.LogInformation("Updating nutrition targets for UserId: {UserId}", userId);

        var userInfor = new UserInformationDto
        {
            ActivityLevel = user.ActivityLevel,
            PrimaryNutritionGoal = user.PrimaryNutritionGoal,
            Height = user.Height,
            Weight = user.Weight,
            DateOfBirth = user.DateOfBirth,
            Gender = user.gender,
            TargetWeight = user.TargetWeight,
        };

        await nutritionTargetsService.UpdateNutritionTargetAsync(userInfor);

        logger.LogInformation("User profile updated successfully for UserId: {UserId}", userId);

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