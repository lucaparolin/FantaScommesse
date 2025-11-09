namespace FantaScommesse.Models.Entities;

/// <summary>
/// Represents a user's prediction for a round (colonna)
/// </summary>
public class Prediction : BaseAuditEntity
{
    public long PredictionId { get; set; }
    public long ParticipationId { get; set; }
    public int RoundId { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedUtc { get; set; } // NULL = draft
    public bool IsValid { get; set; }
    public bool IsLate { get; set; }
    public byte ErrorsCnt { get; set; }
}
