using FantaScommesse.Models.Entities;

namespace FantaScommesse.Repositories;

public interface IReferralRepository : IRepository<Referral, long>
{
    Task<IEnumerable<Referral>> GetByReferrerIdAsync(long referrerParticipationId);
    Task<int> GetReferralCountAsync(long referrerParticipationId);
    Task<decimal> GetTotalDiscountsEarnedAsync(long referrerParticipationId);
}
