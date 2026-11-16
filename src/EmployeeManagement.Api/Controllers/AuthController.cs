using EmployeeManagement.Api.Auth;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Email, string Role, DateTime ExpiresAtUtc);

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IJwtTokenService jwtTokenService, ILogger<AuthController> logger)
    {
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns a signed JWT Bearer token with role claims.
    /// Pre-configured test accounts:
    /// - admin@enterprise.io / Admin123! (Role: Admin)
    /// - manager@enterprise.io / Manager123! (Role: Manager)
    /// - employee@enterprise.io / Employee123! (Role: Employee)
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Standard mock role credential directory for Gate 2 demonstration
        string? role = (normalizedEmail, request.Password) switch
        {
            ("admin@enterprise.io", "Admin123!") => "Admin",
            ("manager@enterprise.io", "Manager123!") => "Manager",
            ("employee@enterprise.io", "Employee123!") => "Employee",
            _ => null
        };

        if (role == null)
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Authentication Failed",
                Detail = "Invalid email or password. Use test accounts: admin@enterprise.io / Admin123!, manager@enterprise.io / Manager123!, employee@enterprise.io / Employee123!",
                Instance = Request.Path
            });
        }

        var token = _jwtTokenService.GenerateToken(Guid.NewGuid().ToString(), normalizedEmail, role);
        var expiresAt = DateTime.UtcNow.AddHours(1);

        _logger.LogInformation("User {Email} logged in successfully with Role: {Role}", normalizedEmail, role);

        return Ok(new LoginResponse(token, normalizedEmail, role, expiresAt));
    }
}
