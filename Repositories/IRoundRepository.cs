using FantaScommesse.Models.Entities;

namespace FantaScommesse.Repositories;

/// <summary>
/// Repository interface for Round entity operations
/// </summary>
public interface IRoundRepository : IRepository<Round, int>
{
    Task<Round?> GetBySeasonAndRoundNoAsync(int seasonId, int roundNo);
    Task<IEnumerable<Round>> GetBySeasonIdAsync(int seasonId);
    Task<Round?> GetCurrentRoundAsync();
}
