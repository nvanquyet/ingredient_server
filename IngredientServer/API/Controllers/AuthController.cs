using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs.Auth;
using IngredientServer.Utils.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IngredientServer.Utils.DTOs;

namespace IngredientServer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, IUserContextService userContextService) : BaseController
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await authService.LoginAsync(loginDto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await authService.RegisterAsync(registerDto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(Register), result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = userContextService.GetAuthenticatedUserId();
       
        var result = await authService.LogoutAsync(userIdClaim);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetUserProfile()
    {
        var userIdClaim = userContextService.GetAuthenticatedUserId();
        var user = await authService.GetUserProfileAsync(userIdClaim);
        return Ok(user);
    }
    
    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UserProfileDto userProfileDto)
    {
        // Thêm validation check
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = userContextService.GetAuthenticatedUserId();

        var user = await authService.UpdateUserProfileAsync(userIdClaim, userProfileDto);
        return Ok(user);
    }
    
    
    [HttpPut("change_password")]
    [Authorize]
    public async Task<IActionResult> OnChangePassword([FromBody] ChangePasswordDto? dto)
    {
        // Thêm validation check
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = userContextService.GetAuthenticatedUserId();

        var user = await authService.ChangePasswordAsync(userIdClaim, dto);
        return Ok(user);
    }
}