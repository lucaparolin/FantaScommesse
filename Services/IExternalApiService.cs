namespace FantaScommesse.Services;

public interface IExternalApiService
{
    Task<List<ExternalMatch>> FetchUpcomingMatchesAsync(string leagueId, int season);
    Task<List<ExternalResult>> FetchMatchResultsAsync(string leagueId, int round);
    Task SyncCalendarAsync(int seasonId, int roundNo);
    Task SyncResultsAsync(int roundId);
}

public class ExternalMatch
{
    public string ApiMatchId { get; set; } = string.Empty;
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public DateTime KickoffUtc { get; set; }
    public int RoundNo { get; set; }
}

public class ExternalResult
{
    public string ApiMatchId { get; set; } = string.Empty;
    public string ResultCode { get; set; } = string.Empty;
    public byte GoalsHome { get; set; }
    public byte GoalsAway { get; set; }
}
