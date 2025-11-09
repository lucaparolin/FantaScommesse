namespace FantaScommesse.Models.DTOs;

/// <summary>
/// DTO for scoreboard display
/// </summary>
public class ScoreboardDto
{
    public int RoundId { get; set; }
    public int? RoundNo { get; set; }
    public List<ScoreboardEntryDto> Entries { get; set; } = new();
}

public class ScoreboardEntryDto
{
    public int Rank { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public byte PointsBase { get; set; }
    public short Bonus { get; set; }
    public short Penalty { get; set; }
    public short Total { get; set; }
}
