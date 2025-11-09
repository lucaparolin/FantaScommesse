using FantaScommesse.Models.DTOs;

namespace FantaScommesse.Validators;

/// <summary>
/// Validator implementation for prediction validation
/// Implements the 4-3-3 rule as per specifications:
/// - Exactly 4 doubles (1X, X2, 12)
/// - Exactly 3 singles (1, X, 2)
/// - Exactly 3 specials (GG, NG, OVER, UNDER)
/// </summary>
public class PredictionValidator : IPredictionValidator
{
    private static readonly string[] ValidSelections = { "1", "X", "2", "1X", "X2", "12", "GG", "NG", "OVER", "UNDER" };
    private static readonly string[] Doubles = { "1X", "X2", "12" };
    private static readonly string[] Singles = { "1", "X", "2" };
    private static readonly string[] Specials = { "GG", "NG", "OVER", "UNDER" };

    public ValidationResultDto Validate(PredictionSubmitDto prediction, int expectedMatchCount = 10)
    {
        var errors = new List<string>();

        // Check if prediction items count matches expected (10 matches)
        if (prediction.Items.Count != expectedMatchCount)
        {
            errors.Add($"Devi pronosticare esattamente {expectedMatchCount} partite. Hai inserito {prediction.Items.Count}.");
            return ValidationResultDto.Failure(errors.ToArray());
        }

        // Check for duplicate matches
        var matchIds = prediction.Items.Select(i => i.MatchId).ToList();
        var duplicates = matchIds.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Any())
        {
            errors.Add($"Hai inserito pronostici duplicati per le seguenti partite: {string.Join(", ", duplicates)}");
        }

        // Validate each selection is valid
        var invalidSelections = prediction.Items
            .Where(i => !ValidSelections.Contains(i.Selection.ToUpperInvariant()))
            .Select(i => $"Partita {i.MatchId}: '{i.Selection}'")
            .ToList();

        if (invalidSelections.Any())
        {
            errors.Add($"Selezioni non valide trovate: {string.Join(", ", invalidSelections)}");
        }

        // Count selection types
        var selections = prediction.Items.Select(i => i.Selection.ToUpperInvariant()).ToList();
        var doublesCount = selections.Count(s => Doubles.Contains(s));
        var singlesCount = selections.Count(s => Singles.Contains(s));
        var specialsCount = selections.Count(s => Specials.Contains(s));

        // Validate 4-3-3 rule
        if (doublesCount != 4)
        {
            errors.Add($"Servono esattamente 4 doppie (1X, X2, 12). Hai inserito {doublesCount} doppie.");
        }

        if (singlesCount != 3)
        {
            errors.Add($"Servono esattamente 3 fisse (1, X, 2). Hai inserito {singlesCount} fisse.");
        }

        if (specialsCount != 3)
        {
            errors.Add($"Servono esattamente 3 selezioni tra GG/NG/OVER/UNDER. Hai inserito {specialsCount} selezioni speciali.");
        }

        // Ensure the sum equals 10
        var totalCount = doublesCount + singlesCount + specialsCount;
        if (totalCount != expectedMatchCount && errors.Count == 0)
        {
            errors.Add($"Il totale delle selezioni deve essere {expectedMatchCount}. Totale attuale: {totalCount}.");
        }

        return errors.Any() ? ValidationResultDto.Failure(errors.ToArray()) : ValidationResultDto.Success();
    }
}
