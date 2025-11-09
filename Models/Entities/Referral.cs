namespace FantaScommesse.Models.Entities;

/// <summary>
/// Represents a referral tracking for discount application
/// </summary>
public class Referral : BaseAuditEntity
{
    public long ReferralId { get; set; }
    public long ReferrerParticipationId { get; set; }
    public long ReferredParticipationId { get; set; }
    public decimal DiscountEur { get; set; } = 5.00m;
}
