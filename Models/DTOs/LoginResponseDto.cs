namespace FantaScommesse.Models.DTOs;

/// <summary>
/// DTO for user login response
/// </summary>
public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool IsOrganizer { get; set; }
}
