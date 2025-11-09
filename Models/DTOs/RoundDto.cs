namespace FantaScommesse.Models.DTOs;

/// <summary>
/// DTO for round information
/// </summary>
public class RoundDto
{
    public int RoundId { get; set; }
    public int RoundNo { get; set; }
    public DateTime DeadlineUtc { get; set; }
    public bool IsPublished { get; set; }
    public bool IsClosed { get; set; }
    public List<MatchDto> Matches { get; set; } = new();
}
