using System.Text.Json;
using FantaScommesse.Repositories;

namespace FantaScommesse.Services;

public class ExternalApiService : IExternalApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMatchRepository _matchRepository;
    private readonly IRoundRepository _roundRepository;
    private readonly ILogger<ExternalApiService> _logger;
    private readonly IConfiguration _configuration;

    public ExternalApiService(
        IHttpClientFactory httpClientFactory,
        IMatchRepository matchRepository,
        IRoundRepository roundRepository,
        ILogger<ExternalApiService> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _matchRepository = matchRepository;
        _roundRepository = roundRepository;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<List<ExternalMatch>> FetchUpcomingMatchesAsync(string leagueId, int season)
    {
        var apiKey = _configuration["ExternalApi:TheSportsDB:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("TheSportsDB API key not configured");
            return new List<ExternalMatch>();
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://www.thesportsdb.com/api/v1/json/{apiKey}/eventsnextleague.php?id={leagueId}";
            var response = await client.GetStringAsync(url);
            var data = JsonSerializer.Deserialize<JsonElement>(response);

            // Parse and return matches (simplified)
            _logger.LogInformation("Fetched matches from TheSportsDB");
            return new List<ExternalMatch>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch matches from TheSportsDB");
            return new List<ExternalMatch>();
        }
    }

    public async Task<List<ExternalResult>> FetchMatchResultsAsync(string leagueId, int round)
    {
        // Similar to FetchUpcomingMatchesAsync but for past events
        _logger.LogInformation("Fetching results for league {LeagueId} round {Round}", leagueId, round);
        return new List<ExternalResult>();
    }

    public async Task SyncCalendarAsync(int seasonId, int roundNo)
    {
        _logger.LogInformation("Syncing calendar for season {SeasonId} round {RoundNo}", seasonId, roundNo);
        // TODO: Implement full sync logic
        await Task.CompletedTask;
    }

    public async Task SyncResultsAsync(int roundId)
    {
        _logger.LogInformation("Syncing results for round {RoundId}", roundId);
        var round = await _roundRepository.GetByIdAsync(roundId);
        if (round == null) return;

        // Fetch results from external API
        var results = await FetchMatchResultsAsync("4332", round.RoundNo); // 4332 = Serie A

        // Update matches
        foreach (var result in results)
        {
            // Find match and update
            _logger.LogInformation("Processing result for match {ApiMatchId}", result.ApiMatchId);
        }

        await Task.CompletedTask;
    }
}
