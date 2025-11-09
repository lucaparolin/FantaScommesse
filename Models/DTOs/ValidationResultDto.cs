namespace FantaScommesse.Models.DTOs;

/// <summary>
/// DTO for validation results
/// </summary>
public class ValidationResultDto
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ValidationResultDto Success() => new() { IsValid = true };
    public static ValidationResultDto Failure(params string[] errors) => new() { IsValid = false, Errors = errors.ToList() };
}
