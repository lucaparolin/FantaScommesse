namespace FantaScommesse.Models.Entities;

/// <summary>
/// Represents a user's participation in a season
/// </summary>
public class Participation : BaseAuditEntity
{
    public long ParticipationId { get; set; }
    public long UserId { get; set; }
    public int SeasonId { get; set; }
}
