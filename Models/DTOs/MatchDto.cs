namespace FantaScommesse.Models.DTOs;

/// <summary>
/// DTO for match information
/// </summary>
public class MatchDto
{
    public long MatchId { get; set; }
    public byte OrderNo { get; set; }
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public DateTime KickoffUtc { get; set; }
    public string? ResultCode { get; set; }
    public byte? GoalsHome { get; set; }
    public byte? GoalsAway { get; set; }
}
