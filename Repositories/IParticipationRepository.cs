using FantaScommesse.Models.Entities;

namespace FantaScommesse.Repositories;

public interface IParticipationRepository : IRepository<Participation, long>
{
    Task<Participation?> GetByUserAndSeasonAsync(long userId, int seasonId);
    Task<IEnumerable<Participation>> GetBySeasonIdAsync(int seasonId);
    Task<IEnumerable<Participation>> GetByUserIdAsync(long userId);
    Task<int> GetParticipantCountAsync(int seasonId);
}
