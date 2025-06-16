// BaseController.cs
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IngredientServer.API.Controllers;

public abstract class BaseController : ControllerBase
{
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!int.TryParse(userIdClaim, out int userId) || userId <= 0)
        {
            throw new UnauthorizedAccessException("Invalid or missing user authentication.");
        }

        return userId;
    }

    protected string GetCurrentUsername()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    }

    protected string GetCurrentUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    }

    protected bool IsAuthenticated()
    {
        return User?.Identity?.IsAuthenticated == true;
    }
}