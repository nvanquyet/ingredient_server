using Microsoft.AspNetCore.Mvc;

namespace IngredientServer.API.Controllers;

public class BaseController : ControllerBase
{
    protected IActionResult HandleResponse<T>(bool success, string message, T? data = default)
    {
        var response = new
        {
            Success = success,
            Message = message,
            Data = data
        };

        if (success)
        {
            return Ok(response);
        }

        return BadRequest(response);
    }

    protected IActionResult HandleException(Exception ex, string message = "An error occurred")
    {
        // Log the exception here if needed
        
        var response = new
        {
            Success = false,
            Message = message,
            Data = (object?)null
        };

        return StatusCode(500, response);
    }
}