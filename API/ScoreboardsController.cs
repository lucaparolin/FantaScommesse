using FantaScommesse.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace FantaScommesse.API;

/// <summary>
/// API Controller for scoreboard operations
/// Base path: /api/v1/scoreboards
/// </summary>
[ApiController]
[Route("api/v1/scoreboards")]
[Authorize]
public class ScoreboardsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public ScoreboardsController(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not configured");
    }

    /// <summary>
    /// Get scoreboard for a specific round
    /// GET /api/v1/scoreboards/{roundId}
    /// </summary>
    [HttpGet("{roundId}")]
    public async Task<IActionResult> GetRoundScoreboard(int roundId)
    {
        var scoreboard = new ScoreboardDto
        {
            RoundId = roundId,
            Entries = new List<ScoreboardEntryDto>()
        };

        const string sql = @"
            SELECT rank_position, display_name, points_base, bonus, penalty, total
            FROM vw_round_scoreboard
            WHERE round_id = @RoundId
            ORDER BY rank_position";

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@RoundId", roundId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            scoreboard.Entries.Add(new ScoreboardEntryDto
            {
                Rank = reader.GetInt32(0),
                DisplayName = reader.GetString(1),
                PointsBase = reader.GetByte(2),
                Bonus = reader.GetInt16(3),
                Penalty = reader.GetInt16(4),
                Total = reader.GetInt16(5)
            });
        }

        return Ok(scoreboard);
    }

    /// <summary>
    /// Get overall season scoreboard
    /// GET /api/v1/scoreboards/season/{seasonId}
    /// </summary>
    [HttpGet("season/{seasonId}")]
    public async Task<IActionResult> GetSeasonScoreboard(int seasonId)
    {
        var entries = new List<object>();

        const string sql = @"
            SELECT rank_position, display_name, rounds_played, total_base_points,
                   total_bonus, total_penalty, total_points
            FROM vw_season_scoreboard
            WHERE season_id = @SeasonId
            ORDER BY rank_position";

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SeasonId", seasonId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            entries.Add(new
            {
                rank = reader.GetInt32(0),
                displayName = reader.GetString(1),
                roundsPlayed = reader.GetInt32(2),
                totalBasePoints = reader.GetInt32(3),
                totalBonus = reader.GetInt32(4),
                totalPenalty = reader.GetInt32(5),
                totalPoints = reader.GetInt32(6)
            });
        }

        return Ok(new { seasonId, entries });
    }
}
