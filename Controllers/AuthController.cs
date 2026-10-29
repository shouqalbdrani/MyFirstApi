
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MyFirstApi.DTOs;
using MyFirstApi.Services;

namespace MyFirstApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IAuthService _authService;

    public AuthController(
        IConfiguration configuration,
        IAuthService authService)
    {
        _configuration = configuration;
        _authService = authService;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterDto request)
    {
        var user = await _authService.RegisterAsync(request);

        if (user == null)
        {
            return Conflict(new
            {
                message = "Email already exists"
            });
        }

        return StatusCode(201, new
        {
            message = "User registered successfully",
            user.Id,
            user.Name,
            user.Email
        });
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto request)
    {
        var user = await _authService.LoginAsync(request);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password"
            });
        }

        // JWT Claims
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!
            )
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return Ok(new
        {
            message = "Login successful",
            accessToken = tokenString,
            tokenType = "Bearer",
            expiresIn = 1800,
            user = new
            {
                user.Id,
                user.Name,
                user.Email
            }
        });
    }

    // GET: api/auth/profile
    [Authorize]
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        return Ok(new
        {
            id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            name = User.FindFirst(ClaimTypes.Name)?.Value,
            email = User.FindFirst(ClaimTypes.Email)?.Value,
            message = "Welcome to your profile"
        });
    }
}
