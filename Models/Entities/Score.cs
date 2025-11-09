namespace FantaScommesse.Models.Entities;

/// <summary>
/// Represents the calculated score for a prediction
/// </summary>
public class Score : BaseAuditEntity
{
    public long ScoreId { get; set; }
    public long PredictionId { get; set; }
    public byte PointsBase { get; set; } // 0-10
    public short Bonus { get; set; }
    public short Penalty { get; set; }
    public short Total { get; set; }
    public DateTime ComputedUtc { get; set; } = DateTime.UtcNow;
}
