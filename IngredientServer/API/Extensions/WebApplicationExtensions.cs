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
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            
            // Check if database exists, if not create it
            if (!await context.Database.CanConnectAsync())
            {
                logger.LogInformation("Database does not exist, creating...");
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database created successfully");
            }
            else
            {
                // Apply pending migrations
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying {Count} pending migrations: {Migrations}", 
                        pendingMigrations.Count(), string.Join(", ", pendingMigrations));
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully");
                }
                else
                {
                    logger.LogInformation("Database is up to date");
                    
                    // FIX: Check if CachedFoods table exists, if not create it manually
                    try
                    {
                        var tableExists = await context.Database.ExecuteSqlRawAsync(
                            "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'CachedFoods'");
                        
                        // If query returns no rows, table doesn't exist - create it
                        var cachedFoodExists = context.Model.FindEntityType(typeof(Core.Entities.CachedFood)) != null;
                        if (cachedFoodExists)
                        {
                            logger.LogInformation("CachedFoods table check - ensuring table exists...");
                            // Try to query the table - if it fails, table doesn't exist
                            try
                            {
                                await context.Set<Core.Entities.CachedFood>().CountAsync();
                                logger.LogInformation("CachedFoods table exists");
                            }
                            catch
                            {
                                logger.LogWarning("CachedFoods table does not exist, creating...");
                                await context.Database.ExecuteSqlRawAsync(@"
                                    CREATE TABLE IF NOT EXISTS `CachedFoods` (
                                        `Id` int NOT NULL AUTO_INCREMENT,
                                        `Name` varchar(200) NOT NULL,
                                        `SearchKey` varchar(500) NOT NULL,
                                        `Description` varchar(1000) NULL,
                                        `PreparationTimeMinutes` int NOT NULL,
                                        `CookingTimeMinutes` int NOT NULL,
                                        `Calories` decimal(8,2) NOT NULL,
                                        `Protein` decimal(8,2) NOT NULL,
                                        `Carbohydrates` decimal(8,2) NOT NULL,
                                        `Fat` decimal(8,2) NOT NULL,
                                        `Fiber` decimal(8,2) NOT NULL,
                                        `ImageUrl` varchar(500) NULL,
                                        `Instructions` json NOT NULL,
                                        `Tips` json NOT NULL,
                                        `Ingredients` json NOT NULL,
                                        `DifficultyLevel` int NOT NULL DEFAULT 1,
                                        `HitCount` int NOT NULL DEFAULT 0,
                                        `LastAccessedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
                                        `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
                                        `UpdatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
                                        PRIMARY KEY (`Id`),
                                        UNIQUE KEY `IX_CachedFoods_SearchKey` (`SearchKey`),
                                        KEY `IX_CachedFoods_Name` (`Name`),
                                        KEY `IX_CachedFoods_LastAccessedAt` (`LastAccessedAt`),
                                        KEY `IX_CachedFoods_HitCount` (`HitCount`)
                                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
                                ");
                                logger.LogInformation("CachedFoods table created successfully");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Could not verify/create CachedFoods table, will rely on migrations");
                    }
                }
            }
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

