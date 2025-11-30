using IngredientServer.API.Middlewares;
using IngredientServer.Core.Helpers;
using IngredientServer.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IngredientServer.API.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Configure HTTP request pipeline
    /// </summary>
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Development middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ingredient Server API v1");
                c.RoutePrefix = string.Empty;
            });
        }

        app.UseHttpsRedirection();

        // Static files
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads")),
            RequestPath = "/uploads"
        });

        app.UseCors("AllowAll");

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Custom middlewares
        app.UseMiddleware<JwtMiddleware>();
        app.UseMiddleware<GlobalErrorHandlingMiddleware>();

        app.MapControllers();

        // Health check endpoint
        app.MapGet("/health", () => Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTimeHelper.UtcNow,
            version = "1.0.0"
        }));

        // API info endpoint
        app.MapGet("/api/info", () => Results.Ok(new
        {
            name = "Ingredient Server API",
            version = "1.0.0",
            description = "API for managing ingredients, foods, meals and nutrition tracking",
            endpoints = new[]
            {
                "/api/auth - Authentication endpoints",
                "/api/ingredient - Ingredient management",
                "/api/food - Food management",
                "/api/meal - Meal management",
                "/api/nutrition - Nutrition tracking and analytics"
            }
        }));

        return app;
    }

    /// <summary>
    /// Initialize database
    /// </summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Database initialized successfully");
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Database initialization failed");
            throw;
        }
    }

    /// <summary>
    /// Log startup information
    /// </summary>
    public static void LogStartupInfo(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Application starting on {Time}", DateTimeHelper.UtcNow);
        logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
    }
}

