
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFirstApi.Data;
using MyFirstApi.Models;
using MyFirstApi.DTOs;

namespace MyFirstApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/users
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(users);
    }

    // GET: api/users/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound("User not found");

        return Ok(user);
    }

    // POST: api/users
    [HttpPost]
    public async Task<IActionResult> AddUser(
        [FromBody] CreateUserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required");

        var user = new User
        {
            Name = request.Name
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetUser),
            new { id = user.Id },
            user);
    }

    // PUT: api/users/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(
        int id,
        [FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required");

        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound("User not found");

        user.Name = request.Name;
        await _context.SaveChangesAsync();

        return Ok(user);
    }

    // DELETE: api/users/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound("User not found");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateUserRequest
{
    public string Name { get; set; } = "";
}
