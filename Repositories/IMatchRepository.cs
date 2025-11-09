using FantaScommesse.Models.Entities;

namespace FantaScommesse.Repositories;

/// <summary>
/// Repository interface for Match entity operations
/// </summary>
public interface IMatchRepository : IRepository<Match, long>
{
    Task<IEnumerable<Match>> GetByRoundIdAsync(int roundId);
    Task<bool> UpdateResultsAsync(long matchId, string resultCode, byte? goalsHome, byte? goalsAway, string? ggng, string? ou, string username);
}
