using FantaScommesse.Models.Entities;

namespace FantaScommesse.Repositories;

public interface ISeasonRepository : IRepository<Season, int>
{
    Task<Season?> GetCurrentSeasonAsync();
    Task<IEnumerable<Season>> GetActiveSeasons();
}
