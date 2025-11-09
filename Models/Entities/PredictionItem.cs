namespace FantaScommesse.Models.Entities;

/// <summary>
/// Represents an individual match prediction within a colonna
/// </summary>
public class PredictionItem : BaseAuditEntity
{
    public long ItemId { get; set; }
    public long PredictionId { get; set; }
    public long MatchId { get; set; }
    public string Selection { get; set; } = string.Empty; // '1','X','2','1X','X2','12','GG','NG','OVER','UNDER'
}
