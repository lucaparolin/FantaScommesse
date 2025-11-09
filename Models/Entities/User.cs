namespace FantaScommesse.Models.Entities;

/// <summary>
/// Represents a user in the system (participant, organizer, or admin)
/// </summary>
public class User : BaseAuditEntity
{
    public long UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsOrganizer { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public byte Status { get; set; } = 1; // 1 = active, 0 = disabled
}
