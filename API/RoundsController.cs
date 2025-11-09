using FantaScommesse.Models.DTOs;
using FantaScommesse.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace FantaScommesse.API;

/// <summary>
/// API Controller for round operations
/// Base path: /api/v1/rounds
/// </summary>
[ApiController]
[Route("api/v1/rounds")]
[Authorize]
public class RoundsController : ControllerBase
{
    private readonly IRoundRepository _roundRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public RoundsController(
        IRoundRepository roundRepository,
        IMatchRepository matchRepository,
        IConfiguration configuration)
    {
        _roundRepository = roundRepository;
        _matchRepository = matchRepository;
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not configured");
    }

    /// <summary>
    /// Get current active round
    /// GET /api/v1/rounds/current
    /// </summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentRound()
    {
        var round = await _roundRepository.GetCurrentRoundAsync();

        if (round == null)
            return NotFound(new { message = "Nessuna giornata attiva" });

        var matches = await GetMatchesWithTeamsAsync(round.RoundId);

        return Ok(new RoundDto
        {
            RoundId = round.RoundId,
            RoundNo = round.RoundNo,
            DeadlineUtc = round.DeadlineUtc,
            IsPublished = round.IsPublished,
            IsClosed = round.IsClosed,
            Matches = matches
        });
    }

    /// <summary>
    /// Get specific round details
    /// GET /api/v1/rounds/{roundId}
    /// </summary>
    [HttpGet("{roundId}")]
    public async Task<IActionResult> GetRound(int roundId)
    {
        var round = await _roundRepository.GetByIdAsync(roundId);

        if (round == null)
            return NotFound();

        var matches = await GetMatchesWithTeamsAsync(roundId);

        return Ok(new RoundDto
        {
            RoundId = round.RoundId,
            RoundNo = round.RoundNo,
            DeadlineUtc = round.DeadlineUtc,
            IsPublished = round.IsPublished,
            IsClosed = round.IsClosed,
            Matches = matches
        });
    }

    private async Task<List<MatchDto>> GetMatchesWithTeamsAsync(int roundId)
    {
        const string sql = @"
            SELECT m.match_id, m.order_no, ht.name as home_team, at.name as away_team,
                   m.kickoff_utc, m.result_code, m.goals_home, m.goals_away
            FROM fs_match m
            INNER JOIN fs_team ht ON ht.team_id = m.home_team_id
            INNER JOIN fs_team at ON at.team_id = m.away_team_id
            WHERE m.round_id = @RoundId
            ORDER BY m.order_no";

        var matches = new List<MatchDto>();

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@RoundId", roundId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            matches.Add(new MatchDto
            {
                MatchId = reader.GetInt64(0),
                OrderNo = reader.GetByte(1),
                HomeTeam = reader.GetString(2),
                AwayTeam = reader.GetString(3),
                KickoffUtc = reader.GetDateTime(4),
                ResultCode = reader.IsDBNull(5) ? null : reader.GetString(5),
                GoalsHome = reader.IsDBNull(6) ? null : reader.GetByte(6),
                GoalsAway = reader.IsDBNull(7) ? null : reader.GetByte(7)
            });
        }

        return matches;
    }
}
