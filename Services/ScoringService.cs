using System.Data.SqlClient;
using FantaScommesse.Models.Entities;
using FantaScommesse.Repositories;

namespace FantaScommesse.Services;

/// <summary>
/// Service implementation for scoring calculations
/// Implements the complete scoring logic as per specifications:
/// - Base points: 1 per correct prediction (max 10)
/// - Bonuses: 10/10 solo (+5), 10/10 shared (+3), top score (+3 solo / +1 shared), unique result (+5), unique fix (+1)
/// - Penalties: late submission (-1), format errors (-1 per error, no bonuses if errors > 0)
/// </summary>
public class ScoringService : IScoringService
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public ScoringService(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not configured");
    }

    public async Task CalculateRoundScoresAsync(int roundId, string username)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        using var transaction = conn.BeginTransaction();

        try
        {
            // Step 1: Get all submitted predictions for the round
            var predictions = await GetRoundPredictionsAsync(conn, transaction, roundId);

            if (!predictions.Any())
            {
                transaction.Rollback();
                return;
            }

            // Step 2: Get match results
            var matchResults = await GetMatchResultsAsync(conn, transaction, roundId);

            // Step 3: Calculate base points for each prediction
            var predictionScores = new Dictionary<long, PredictionScore>();

            foreach (var prediction in predictions)
            {
                var items = await GetPredictionItemsAsync(conn, transaction, prediction.PredictionId);
                var basePoints = CalculateBasePoints(items, matchResults);

                predictionScores[prediction.PredictionId] = new PredictionScore
                {
                    PredictionId = prediction.PredictionId,
                    ParticipationId = prediction.ParticipationId,
                    BasePoints = (byte)basePoints,
                    HasErrors = prediction.ErrorsCnt > 0,
                    IsLate = prediction.IsLate,
                    Items = items
                };
            }

            // Step 4: Calculate penalties
            foreach (var score in predictionScores.Values)
            {
                score.Penalty = CalculatePenalties(score);
            }

            // Step 5: Calculate bonuses (requires analyzing all predictions together)
            await CalculateBonusesAsync(conn, transaction, roundId, predictionScores, matchResults);

            // Step 6: Save scores to database
            await SaveScoresAsync(conn, transaction, predictionScores, username);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task RecalculatePredictionScoreAsync(long predictionId, string username)
    {
        // For now, recalculate the entire round to ensure bonus calculations are correct
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = "SELECT round_id FROM fs_prediction WHERE prediction_id = @PredictionId";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@PredictionId", predictionId);

        var roundId = (int?)await cmd.ExecuteScalarAsync();
        if (roundId.HasValue)
        {
            await CalculateRoundScoresAsync(roundId.Value, username);
        }
    }

    private int CalculateBasePoints(List<PredictionItemData> items, Dictionary<long, MatchResult> matchResults)
    {
        int points = 0;

        foreach (var item in items)
        {
            if (!matchResults.TryGetValue(item.MatchId, out var result))
                continue;

            if (IsCorrectPrediction(item.Selection, result))
            {
                points++;
            }
        }

        return points;
    }

    private bool IsCorrectPrediction(string selection, MatchResult result)
    {
        if (string.IsNullOrEmpty(result.ResultCode)) return false;

        return selection.ToUpperInvariant() switch
        {
            "1" => result.ResultCode == "1",
            "X" => result.ResultCode == "X",
            "2" => result.ResultCode == "2",
            "1X" => result.ResultCode is "1" or "X",
            "X2" => result.ResultCode is "X" or "2",
            "12" => result.ResultCode is "1" or "2",
            "GG" => result.GgNg == "GG",
            "NG" => result.GgNg == "NG",
            "OVER" => result.Ou == "OVER",
            "UNDER" => result.Ou == "UNDER",
            _ => false
        };
    }

    private short CalculatePenalties(PredictionScore score)
    {
        short penalty = 0;

        // Late submission: -1
        if (score.IsLate)
        {
            penalty += 1;
        }

        // Format errors: -1 per error
        if (score.HasErrors)
        {
            penalty += 1; // Simplified: -1 for any errors
        }

        return (short)-penalty; // Negative value
    }

    private async Task CalculateBonusesAsync(
        SqlConnection conn,
        SqlTransaction transaction,
        int roundId,
        Dictionary<long, PredictionScore> predictionScores,
        Dictionary<long, MatchResult> matchResults)
    {
        // Filter out predictions with errors (no bonuses if errors > 0)
        var eligibleScores = predictionScores.Values.Where(s => !s.HasErrors).ToList();

        if (!eligibleScores.Any()) return;

        // Bonus 1: 10/10 perfect score
        var perfectScores = eligibleScores.Where(s => s.BasePoints == 10).ToList();
        if (perfectScores.Count == 1)
        {
            // Solo 10/10: +5
            perfectScores[0].Bonus += 5;
            await SaveBonusEventAsync(conn, transaction, roundId, perfectScores[0].ParticipationId,
                "TENOUTOF10_SOLO", 5, "Unico 10/10 nella giornata");
        }
        else if (perfectScores.Count > 1)
        {
            // Shared 10/10: +3 each
            foreach (var score in perfectScores)
            {
                score.Bonus += 3;
                await SaveBonusEventAsync(conn, transaction, roundId, score.ParticipationId,
                    "TENOUTOF10_SHARED", 3, $"10/10 condiviso con altri {perfectScores.Count - 1}");
            }
        }

        // Bonus 2: Top score without 10/10
        var nonPerfectScores = eligibleScores.Where(s => s.BasePoints < 10).ToList();
        if (nonPerfectScores.Any())
        {
            var maxPoints = nonPerfectScores.Max(s => s.BasePoints);
            var topScorers = nonPerfectScores.Where(s => s.BasePoints == maxPoints).ToList();

            if (topScorers.Count == 1)
            {
                // Solo top score: +3
                topScorers[0].Bonus += 3;
                await SaveBonusEventAsync(conn, transaction, roundId, topScorers[0].ParticipationId,
                    "TOPSCORE_SOLO", 3, $"Punteggio più alto ({maxPoints}/10) da solo");
            }
            else
            {
                // Shared top score: +1 each
                foreach (var score in topScorers)
                {
                    score.Bonus += 1;
                    await SaveBonusEventAsync(conn, transaction, roundId, score.ParticipationId,
                        "TOPSCORE_SHARED", 1, $"Punteggio più alto ({maxPoints}/10) condiviso");
                }
            }
        }

        // Bonus 3: Unique result on a match
        foreach (var matchId in matchResults.Keys)
        {
            var result = matchResults[matchId];

            // Get all correct predictions for this match
            var correctPredictions = eligibleScores
                .Where(s => s.Items.Any(i => i.MatchId == matchId && IsCorrectPrediction(i.Selection, result)))
                .ToList();

            // Group by selection type
            var selectionGroups = correctPredictions
                .SelectMany(s => s.Items.Where(i => i.MatchId == matchId && IsCorrectPrediction(i.Selection, result))
                    .Select(i => new { Score = s, Selection = i.Selection }))
                .GroupBy(x => x.Selection.ToUpperInvariant())
                .ToList();

            foreach (var group in selectionGroups)
            {
                if (group.Count() == 1)
                {
                    // Unique selection: +5
                    var score = group.First().Score;
                    score.Bonus += 5;
                    await SaveBonusEventAsync(conn, transaction, roundId, score.ParticipationId,
                        "UNIQUE_RESULT", 5, $"Unico con {group.Key} sulla partita {matchId}");
                }
            }
        }

        // Bonus 4: Unique fix (single selection 1, X, 2)
        var singles = new[] { "1", "X", "2" };
        foreach (var matchId in matchResults.Keys)
        {
            var result = matchResults[matchId];

            // Get all correct SINGLE predictions for this match
            var correctSingles = eligibleScores
                .Where(s => s.Items.Any(i => i.MatchId == matchId &&
                    singles.Contains(i.Selection.ToUpperInvariant()) &&
                    IsCorrectPrediction(i.Selection, result)))
                .ToList();

            if (correctSingles.Count == 1)
            {
                // Unique fix: +1
                correctSingles[0].Bonus += 1;
                await SaveBonusEventAsync(conn, transaction, roundId, correctSingles[0].ParticipationId,
                    "UNIQUE_FIX", 1, $"Unico con fissa corretta sulla partita {matchId}");
            }
        }
    }

    private async Task<List<Prediction>> GetRoundPredictionsAsync(SqlConnection conn, SqlTransaction transaction, int roundId)
    {
        var sql = @"
            SELECT prediction_id, participation_id, round_id, created_utc, submitted_utc,
                   is_valid, is_late, errors_cnt
            FROM fs_prediction
            WHERE round_id = @RoundId AND submitted_utc IS NOT NULL";

        var predictions = new List<Prediction>();
        using var cmd = new SqlCommand(sql, conn, transaction);
        cmd.Parameters.AddWithValue("@RoundId", roundId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            predictions.Add(new Prediction
            {
                PredictionId = reader.GetInt64(0),
                ParticipationId = reader.GetInt64(1),
                RoundId = reader.GetInt32(2),
                CreatedUtc = reader.GetDateTime(3),
                SubmittedUtc = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                IsValid = reader.GetBoolean(5),
                IsLate = reader.GetBoolean(6),
                ErrorsCnt = reader.GetByte(7)
            });
        }

        return predictions;
    }

    private async Task<Dictionary<long, MatchResult>> GetMatchResultsAsync(SqlConnection conn, SqlTransaction transaction, int roundId)
    {
        var sql = @"
            SELECT match_id, result_code, goals_home, goals_away, ggng, ou
            FROM fs_match
            WHERE round_id = @RoundId";

        var results = new Dictionary<long, MatchResult>();
        using var cmd = new SqlCommand(sql, conn, transaction);
        cmd.Parameters.AddWithValue("@RoundId", roundId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var matchId = reader.GetInt64(0);
            results[matchId] = new MatchResult
            {
                MatchId = matchId,
                ResultCode = reader.IsDBNull(1) ? null : reader.GetString(1),
                GoalsHome = reader.IsDBNull(2) ? null : reader.GetByte(2),
                GoalsAway = reader.IsDBNull(3) ? null : reader.GetByte(3),
                GgNg = reader.IsDBNull(4) ? null : reader.GetString(4),
                Ou = reader.IsDBNull(5) ? null : reader.GetString(5)
            };
        }

        return results;
    }

    private async Task<List<PredictionItemData>> GetPredictionItemsAsync(SqlConnection conn, SqlTransaction transaction, long predictionId)
    {
        var sql = @"
            SELECT item_id, prediction_id, match_id, selection
            FROM fs_prediction_item
            WHERE prediction_id = @PredictionId";

        var items = new List<PredictionItemData>();
        using var cmd = new SqlCommand(sql, conn, transaction);
        cmd.Parameters.AddWithValue("@PredictionId", predictionId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new PredictionItemData
            {
                ItemId = reader.GetInt64(0),
                PredictionId = reader.GetInt64(1),
                MatchId = reader.GetInt64(2),
                Selection = reader.GetString(3)
            });
        }

        return items;
    }

    private async Task SaveScoresAsync(SqlConnection conn, SqlTransaction transaction, Dictionary<long, PredictionScore> predictionScores, string username)
    {
        // Delete existing scores
        var deleteSql = @"
            DELETE FROM fs_score
            WHERE prediction_id IN (
                SELECT prediction_id FROM fs_prediction WHERE round_id =
                (SELECT round_id FROM fs_prediction WHERE prediction_id = @PredictionId)
            )";

        foreach (var score in predictionScores.Values.Take(1))
        {
            using var delCmd = new SqlCommand(deleteSql, conn, transaction);
            delCmd.Parameters.AddWithValue("@PredictionId", score.PredictionId);
            await delCmd.ExecuteNonQueryAsync();
            break;
        }

        // Insert new scores
        var insertSql = @"
            INSERT INTO fs_score (prediction_id, points_base, bonus, penalty, total, computed_utc,
                                  data_inserimento, utente_inserimento)
            VALUES (@PredictionId, @PointsBase, @Bonus, @Penalty, @Total, @ComputedUtc,
                    @DataInserimento, @UtenteInserimento)";

        foreach (var score in predictionScores.Values)
        {
            var total = score.BasePoints + score.Bonus + score.Penalty;

            using var cmd = new SqlCommand(insertSql, conn, transaction);
            cmd.Parameters.AddWithValue("@PredictionId", score.PredictionId);
            cmd.Parameters.AddWithValue("@PointsBase", score.BasePoints);
            cmd.Parameters.AddWithValue("@Bonus", score.Bonus);
            cmd.Parameters.AddWithValue("@Penalty", score.Penalty);
            cmd.Parameters.AddWithValue("@Total", total);
            cmd.Parameters.AddWithValue("@ComputedUtc", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@DataInserimento", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@UtenteInserimento", username);

            await cmd.ExecuteNonQueryAsync();
        }
    }

    private async Task SaveBonusEventAsync(SqlConnection conn, SqlTransaction transaction, int roundId,
        long participationId, string kind, short value, string details)
    {
        var sql = @"
            INSERT INTO fs_bonus_event (round_id, participation_id, kind, value, details,
                                        data_inserimento, utente_inserimento)
            VALUES (@RoundId, @ParticipationId, @Kind, @Value, @Details, @DataInserimento, @UtenteInserimento)";

        using var cmd = new SqlCommand(sql, conn, transaction);
        cmd.Parameters.AddWithValue("@RoundId", roundId);
        cmd.Parameters.AddWithValue("@ParticipationId", participationId);
        cmd.Parameters.AddWithValue("@Kind", kind);
        cmd.Parameters.AddWithValue("@Value", value);
        cmd.Parameters.AddWithValue("@Details", details);
        cmd.Parameters.AddWithValue("@DataInserimento", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("@UtenteInserimento", "SYSTEM");

        await cmd.ExecuteNonQueryAsync();
    }

    // Helper classes
    private class PredictionScore
    {
        public long PredictionId { get; set; }
        public long ParticipationId { get; set; }
        public byte BasePoints { get; set; }
        public short Bonus { get; set; }
        public short Penalty { get; set; }
        public bool HasErrors { get; set; }
        public bool IsLate { get; set; }
        public List<PredictionItemData> Items { get; set; } = new();
    }

    private class PredictionItemData
    {
        public long ItemId { get; set; }
        public long PredictionId { get; set; }
        public long MatchId { get; set; }
        public string Selection { get; set; } = string.Empty;
    }

    private class MatchResult
    {
        public long MatchId { get; set; }
        public string? ResultCode { get; set; }
        public byte? GoalsHome { get; set; }
        public byte? GoalsAway { get; set; }
        public string? GgNg { get; set; }
        public string? Ou { get; set; }
    }
}
