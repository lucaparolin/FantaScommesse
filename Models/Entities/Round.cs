namespace FantaScommesse.Models.Entities;

/// <summary>
/// Represents a match round (giornata) within a season
/// </summary>
public class Round : BaseAuditEntity
{
    public int RoundId { get; set; }
    public int SeasonId { get; set; }
    public int RoundNo { get; set; }
    public DateTime DeadlineUtc { get; set; }
    public bool IsPublished { get; set; }
    public bool IsClosed { get; set; }
}
