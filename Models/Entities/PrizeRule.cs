namespace FantaScommesse.Models.Entities;

/// <summary>
/// Represents prize rules configuration for a season
/// Types: WEEKLY, FINAL, MIDSEASON
/// </summary>
public class PrizeRule : BaseAuditEntity
{
    public int PrizeRuleId { get; set; }
    public int SeasonId { get; set; }
    public string Type { get; set; } = string.Empty;
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public int? Placement { get; set; }
    public decimal AmountEur { get; set; }
}
