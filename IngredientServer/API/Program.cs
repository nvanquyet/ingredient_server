using IngredientServer.API.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddCustomControllers();
builder.Services.AddCustomDbContext(builder.Configuration);
builder.Services.AddCustomConfiguration(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddCustomCors();
builder.Services.AddCustomSwagger();

var app = builder.Build();

// Configure pipeline
app.ConfigurePipeline();

// Initialize database
await app.InitializeDatabaseAsync();

// Log startup info
app.LogStartupInfo();

app.Run();