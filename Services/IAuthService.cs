using FantaScommesse.Models.DTOs;
using FantaScommesse.Models.Entities;

namespace FantaScommesse.Services;

/// <summary>
/// Service interface for authentication operations
/// </summary>
public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    Task<User?> RegisterAsync(RegisterRequestDto request);
    string GenerateJwtToken(User user);
}
