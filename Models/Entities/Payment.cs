namespace FantaScommesse.Models.Entities;

/// <summary>
/// Represents payment tracking for a participation
/// </summary>
public class Payment : BaseAuditEntity
{
    public long PaymentId { get; set; }
    public long ParticipationId { get; set; }
    public decimal ExpectedEur { get; set; }
    public decimal PaidEur { get; set; }
    public DateTime DueOnUtc { get; set; }
    public DateTime? PaidOnUtc { get; set; }
    public string? Method { get; set; }
    public string? Note { get; set; }
}
