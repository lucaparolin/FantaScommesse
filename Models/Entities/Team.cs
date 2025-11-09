namespace FantaScommesse.Models.Entities;

/// <summary>
/// Represents a football team (Serie A club)
/// </summary>
public class Team : BaseAuditEntity
{
    public int TeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? ApiTeamId { get; set; }
}
