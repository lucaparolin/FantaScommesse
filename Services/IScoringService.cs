namespace FantaScommesse.Services;

/// <summary>
/// Service interface for scoring calculations
/// Implements the complex logic for points, bonuses, and penalties
/// </summary>
public interface IScoringService
{
    /// <summary>
    /// Calculates scores for all predictions in a round
    /// </summary>
    Task CalculateRoundScoresAsync(int roundId, string username);

    /// <summary>
    /// Recalculates a specific prediction score
    /// </summary>
    Task RecalculatePredictionScoreAsync(long predictionId, string username);
}
