using MyFirstApi.DTOs;
using MyFirstApi.Models;

namespace MyFirstApi.Services;

public interface IAuthService
{
    Task<User?> RegisterAsync(RegisterDto request);

    Task<User?> LoginAsync(LoginDto request);
}