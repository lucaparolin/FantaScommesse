namespace FantaScommesse.Models.DTOs;

/// <summary>
/// DTO for submitting a prediction (colonna)
/// </summary>
public class PredictionSubmitDto
{
    public int RoundId { get; set; }
    public List<PredictionItemDto> Items { get; set; } = new();
    public bool IsDraft { get; set; } // false = submit final
}

public class PredictionItemDto
{
    public long MatchId { get; set; }
    public string Selection { get; set; } = string.Empty;
}
