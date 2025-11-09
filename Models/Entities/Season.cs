namespace FantaScommesse.Models.Entities;

/// <summary>
/// Represents a football season (e.g., Serie A 2025/26)
/// </summary>
public class Season : BaseAuditEntity
{
    public int SeasonId { get; set; }
    public string Name { get; set; } = string.Empty;
    public short YearStart { get; set; }
    public short YearEnd { get; set; }
    public DateTime SignupDeadline { get; set; }
    public decimal BaseFeeEur { get; set; } = 100.00m;
    public decimal ReferralDiscount { get; set; } = 5.00m;
    public long CreatedBy { get; set; }
    public bool IsClosed { get; set; }
}
