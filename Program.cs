
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using MyFirstApi.Data;
using MyFirstApi.Repositories;
using MyFirstApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Dependency Injection
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// PostgreSQL Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Database connection string is missing"
        )
    ));

// OpenAPI
builder.Services.AddOpenApi();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is missing");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var connection = db.Database.GetDbConnection();
    await connection.OpenAsync();

    using var command = connection.CreateCommand();

    command.CommandText = """
    SELECT
        current_database(),
        current_schema(),
        to_regclass('public."Users"')::text,
        to_regclass('public."__EFMigrationsHistory"')::text
    """;

    using var reader = await command.ExecuteReaderAsync();

    if (await reader.ReadAsync())
    {
        Console.WriteLine($"Database: {reader.GetString(0)}");
        Console.WriteLine($"Schema: {reader.GetString(1)}");
        Console.WriteLine($"Users table: {(reader.IsDBNull(2) ? "NOT FOUND" : reader.GetString(2))}");
        Console.WriteLine($"Migrations table: {(reader.IsDBNull(3) ? "NOT FOUND" : reader.GetString(3))}");
    }
}

app.Run();
