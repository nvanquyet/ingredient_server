using System.Security.Claims;
using IngredientServer.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace IngredientServer.Core.Services;


public class UserContextService(IHttpContextAccessor httpContextAccessor) : IUserContextService
{
    // Lấy User ID từ JWT token trong HTTP request (từ client gửi lên)
    public int GetAuthenticatedUserId()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!int.TryParse(userIdClaim, out int userId) || userId <= 0)
        {
            throw new UnauthorizedAccessException("Invalid user ID in token.");
        }

        return userId;
    }

    // Safe method không throw exception
    public bool TryGetAuthenticatedUserId(out int userId)
    {
        userId = 0;
        var httpContext = httpContextAccessor.HttpContext;
        
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            return false;

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }

    public string GetAuthenticatedUsername()
    {
        var httpContext = httpContextAccessor.HttpContext;
        return httpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    }

    public string GetAuthenticatedUserEmail()
    {
        var httpContext = httpContextAccessor.HttpContext;
        return httpContext?.User?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    }

    public bool IsAuthenticated()
    {
        var httpContext = httpContextAccessor.HttpContext;
        return httpContext?.User?.Identity?.IsAuthenticated == true;
    }
}