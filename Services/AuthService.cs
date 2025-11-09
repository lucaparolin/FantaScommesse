using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FantaScommesse.Models.DTOs;
using FantaScommesse.Models.Entities;
using FantaScommesse.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace FantaScommesse.Services;

/// <summary>
/// Service implementation for authentication operations
/// Handles user login, registration, and JWT token generation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || user.Status != 1)
            return null;

        // Verify password (using BCrypt)
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var token = GenerateJwtToken(user);

        return new LoginResponseDto
        {
            Token = token,
            DisplayName = user.DisplayName,
            Email = user.Email,
            IsAdmin = user.IsAdmin,
            IsOrganizer = user.IsOrganizer
        };
    }

    public async Task<User?> RegisterAsync(RegisterRequestDto request)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(request.Email))
            return null;

        // Hash password using BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            DisplayName = request.DisplayName,
            Phone = request.Phone,
            IsAdmin = false,
            IsOrganizer = false,
            CreatedUtc = DateTime.UtcNow,
            Status = 1
        };

        var userId = await _userRepository.CreateAsync(user, "REGISTRATION");
        user.UserId = userId;

        return user;
    }

    public string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName),
            new("IsAdmin", user.IsAdmin.ToString()),
            new("IsOrganizer", user.IsOrganizer.ToString())
        };

        if (user.IsAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        if (user.IsOrganizer)
            claims.Add(new Claim(ClaimTypes.Role, "Organizer"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"] ?? "1440")),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
