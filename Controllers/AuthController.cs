using Microsoft.AspNetCore.Mvc;
using ProductsApi.Models;
using ProductsApi.Services;

namespace ProductsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private static List<User> _users = new List<User>();
    private static int _nextId = 1;
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (_users.Any(u => u.Email.ToLower() == request.Email.ToLower()))
        {
            return BadRequest(new { message = "User with this email already exists" });
        }

        var user = new User
        {
            Id = _nextId++,
            Email = request.Email,
            Name = request.Name,
            PasswordHash = _authService.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        _users.Add(user);

        var token = _authService.GenerateJwtToken(user);
        var expiryMinutes = Convert.ToDouble(_configuration["Jwt:ExpiryMinutes"] ?? "60");

        var response = new AuthResponse
        {
            Token = token,
            Email = user.Email,
            Name = user.Name,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };

        return Ok(response);
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _users.FirstOrDefault(u => u.Email.ToLower() == request.Email.ToLower());
        
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        if (!_authService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var token = _authService.GenerateJwtToken(user);
        var expiryMinutes = Convert.ToDouble(_configuration["Jwt:ExpiryMinutes"] ?? "60");

        var response = new AuthResponse
        {
            Token = token,
            Email = user.Email,
            Name = user.Name,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };

        return Ok(response);
    }
}
