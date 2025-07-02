using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

namespace IngredientServer.Core.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration configuration;
    private readonly TokenValidationParameters tokenValidationParameters;
    private readonly ILogger<JwtService> logger;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        this.configuration = configuration;
        this.logger = logger;

        // Setup token validation parameters - CONSISTENT KEY ENCODING
        var secretKey = configuration["Jwt:Secret"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT Secret is not configured");
        }

        this.tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)), // Changed to UTF8
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            logger.LogInformation("Starting token validation for token: {Token}", token.Substring(0, Math.Min(50, token.Length)) + "...");
            
            var tokenHandler = new JwtSecurityTokenHandler();

            // First, try to read the token without validation to check its structure
            var jsonToken = tokenHandler.ReadJwtToken(token);
            logger.LogInformation("Token parsed successfully. Claims count: {ClaimCount}, Expires: {Expiry}", 
                jsonToken.Claims.Count(), jsonToken.ValidTo);

            // Check if token is expired manually for better logging
            if (jsonToken.ValidTo < DateTime.UtcNow)
            {
                logger.LogWarning("Token is expired. Expiry: {Expiry}, Current: {Current}", 
                    jsonToken.ValidTo, DateTime.UtcNow);
                return null;
            }

            // Now validate the token
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                logger.LogWarning("Token is not a valid JWT token.");
                return null;
            }

            if (!jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                logger.LogWarning("Token algorithm is invalid. Algorithm: {Algorithm}", jwtToken.Header.Alg);
                return null;
            }


            logger.LogInformation("Token validation successful for user: {UserId}", 
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            logger.LogWarning(ex, "Token expired: {Message}", ex.Message);
            return null;
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            logger.LogWarning(ex, "Invalid signature: {Message}", ex.Message);
            return null;
        }
        catch (SecurityTokenMalformedException ex)
        {
            logger.LogWarning(ex, "Malformed token: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Token validation failed: {Message}", ex.Message);
            return null;
        }
    }

    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Secret"] ?? ""); // Changed to UTF8 for consistency

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
        var tokenString = tokenHandler.WriteToken(token);
        
        logger.LogInformation("Generated token for user {UserId} with expiry {Expiry}", 
            user.Id, tokenDescriptor.Expires);
        
        return tokenString;
    }
}