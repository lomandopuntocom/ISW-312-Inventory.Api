using Inventory.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore; // 👈 Agregar
using DotNetEnv;

// Load .env from current directory or parent directory
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (!File.Exists(envPath))
{
    envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
}
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(Path.GetFullPath(envPath));
}

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
if (string.IsNullOrEmpty(connectionString))
{
    var dbHost = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "localhost";
    var dbPort = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
    var dbName = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? throw new InvalidOperationException("DATABASE_NAME not configured");
    var dbUser = Environment.GetEnvironmentVariable("DATABASE_USER") ?? throw new InvalidOperationException("DATABASE_USER not configured");
    var dbPassword = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? throw new InvalidOperationException("DATABASE_PASSWORD not configured");

    connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<Inventory.Api.Infrastructure.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<Inventory.Api.Infrastructure.RestockEventBroadcaster>();
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "inventory")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // 👈 Agregar
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowFrontend");
app.UseExceptionHandler();
app.MapControllers();
app.Run();