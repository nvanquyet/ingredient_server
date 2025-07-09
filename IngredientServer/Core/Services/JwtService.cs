using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IngredientServer.Core.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration configuration;
    private readonly TokenValidationParameters tokenValidationParameters;

    public JwtService(IConfiguration configuration)
    {
        this.configuration = configuration;

        // Setup token validation parameters
        this.tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
            ValidateIssuer = false,  // Nếu bạn không kiểm tra Issuer có thể để false
            ValidateAudience = false, // Nếu bạn không kiểm tra Audience có thể để false
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Token algorithm is invalid");
                return null;
            }

            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            Console.WriteLine($"Token expired: {ex.Message}");
            return null;
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            Console.WriteLine($"Invalid signature: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
            return null;
        }
    }

    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(configuration["Jwt:Secret"] ?? "");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
