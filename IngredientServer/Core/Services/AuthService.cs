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
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ResponseDto<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
            
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

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var token = GenerateJwtToken(user);
            var response = new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = MapToUserDto(user)
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
            _logger.LogError(ex, "Error during login");
            return new ResponseDto<AuthResponseDto>
            {
                Success = false,
                Message = "An error occurred during login"
            };
        }
    }

    public async Task<ResponseDto<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            // Check if user exists
            if (await _userRepository.ExistsAsync(registerDto.Username, registerDto.Email))
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
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName
            };

            await _userRepository.AddAsync(user);

            var token = GenerateJwtToken(user);
            var response = new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = MapToUserDto(user)
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
            _logger.LogError(ex, "Error during registration");
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
            _logger.LogError(ex, "Error during logout");
            return Task.FromResult(new ResponseDto<bool>
            {
                Success = false,
                Message = "An error occurred during logout",
                Data = false
            });
        }
    }

    public string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "");
        
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

    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }
}