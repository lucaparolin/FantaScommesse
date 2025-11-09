using FantaScommesse.Models.Entities;

namespace FantaScommesse.Services;

public interface IParticipationService
{
    Task<Participation> EnrollUserInSeasonAsync(long userId, int seasonId, long? referrerParticipationId = null);
    Task<bool> GeneratePaymentPlanAsync(long participationId, string paymentPlan, string username);
    Task<string> GenerateReferralCodeAsync(long participationId);
    Task<Participation?> GetByReferralCodeAsync(string referralCode);
}
