namespace FantaScommesse.Models.Entities;

/// <summary>
/// Represents a bonus event awarded to a participant
/// Kinds: TENOUTOF10_SOLO, TENOUTOF10_SHARED, TOPSCORE_SOLO, TOPSCORE_SHARED, UNIQUE_RESULT, UNIQUE_FIX
/// </summary>
public class BonusEvent : BaseAuditEntity
{
    public long BonusId { get; set; }
    public int RoundId { get; set; }
    public long ParticipationId { get; set; }
    public string Kind { get; set; } = string.Empty;
    public short Value { get; set; }
    public string? Details { get; set; }
}
