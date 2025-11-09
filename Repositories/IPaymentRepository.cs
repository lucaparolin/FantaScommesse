using FantaScommesse.Models.Entities;

namespace FantaScommesse.Repositories;

public interface IPaymentRepository : IRepository<Payment, long>
{
    Task<IEnumerable<Payment>> GetByParticipationIdAsync(long participationId);
    Task<IEnumerable<Payment>> GetPendingPaymentsAsync();
    Task<IEnumerable<Payment>> GetOverduePaymentsAsync();
    Task<decimal> GetTotalPaidByParticipationAsync(long participationId);
    Task<decimal> GetTotalDueByParticipationAsync(long participationId);
}
