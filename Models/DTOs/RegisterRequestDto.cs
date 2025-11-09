namespace FantaScommesse.Models.DTOs;

/// <summary>
/// DTO for user registration request
/// </summary>
public class RegisterRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Phone { get; set; }
}
