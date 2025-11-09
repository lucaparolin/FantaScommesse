namespace FantaScommesse.Models.Entities;

/// <summary>
/// Represents a penalty event applied to a prediction
/// Kinds: LATE, FORMAT_ERROR
/// </summary>
public class PenaltyEvent : BaseAuditEntity
{
    public long PenaltyId { get; set; }
    public long PredictionId { get; set; }
    public string Kind { get; set; } = string.Empty;
    public short Value { get; set; }
    public string? Details { get; set; }
}
