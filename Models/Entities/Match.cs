namespace FantaScommesse.Models.Entities;

/// <summary>
/// Represents an individual football match within a round
/// </summary>
public class Match : BaseAuditEntity
{
    public long MatchId { get; set; }
    public int RoundId { get; set; }
    public byte OrderNo { get; set; } // 1-10
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public DateTime KickoffUtc { get; set; }
    public string? ResultCode { get; set; } // '1', 'X', '2'
    public byte? GoalsHome { get; set; }
    public byte? GoalsAway { get; set; }
    public string? GgNg { get; set; } // 'GG', 'NG'
    public string? Ou { get; set; } // 'OVER', 'UNDER'
    public string? ApiMatchId { get; set; }
}
