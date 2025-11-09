using FantaScommesse.Models.DTOs;
using FantaScommesse.Services;
using Microsoft.AspNetCore.Mvc;

namespace FantaScommesse.API;

/// <summary>
/// API Controller for authentication operations
/// Base path: /api/v1/auth
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// User login endpoint
    /// POST /api/v1/auth/login
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _authService.LoginAsync(request);

        if (response == null)
            return Unauthorized(new { message = "Credenziali non valide" });

        return Ok(response);
    }

    /// <summary>
    /// User registration endpoint
    /// POST /api/v1/auth/register
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _authService.RegisterAsync(request);

        if (user == null)
            return BadRequest(new { message = "Email già registrata" });

        var token = _authService.GenerateJwtToken(user);

        return Ok(new LoginResponseDto
        {
            Token = token,
            DisplayName = user.DisplayName,
            Email = user.Email,
            IsAdmin = user.IsAdmin,
            IsOrganizer = user.IsOrganizer
        });
    }
}
