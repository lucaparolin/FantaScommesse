using FantaScommesse.Models.Entities;

namespace FantaScommesse.Repositories;

/// <summary>
/// Repository interface for Prediction entity operations
/// </summary>
public interface IPredictionRepository : IRepository<Prediction, long>
{
    Task<Prediction?> GetByParticipationAndRoundAsync(long participationId, int roundId);
    Task<IEnumerable<Prediction>> GetByRoundIdAsync(int roundId);
    Task<IEnumerable<PredictionItem>> GetPredictionItemsAsync(long predictionId);
    Task SavePredictionItemsAsync(long predictionId, IEnumerable<PredictionItem> items, string username);
    Task<bool> SubmitPredictionAsync(long predictionId, DateTime deadline, string username);
}
