using IngredientServer.API.Extensions;

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