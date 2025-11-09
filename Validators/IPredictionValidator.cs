using FantaScommesse.Models.DTOs;

namespace FantaScommesse.Validators;

/// <summary>
/// Validator interface for prediction validation
/// Enforces the 4-3-3 rule: 4 doubles, 3 singles, 3 specials
/// </summary>
public interface IPredictionValidator
{
    ValidationResultDto Validate(PredictionSubmitDto prediction, int expectedMatchCount = 10);
}
