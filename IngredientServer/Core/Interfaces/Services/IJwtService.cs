using System.Security.Claims;
using IngredientServer.Core.Entities;

namespace IngredientServer.Core.Interfaces.Services;
public interface IJwtService
{
    ClaimsPrincipal? ValidateToken(string token);
    string GenerateToken(User user);
}