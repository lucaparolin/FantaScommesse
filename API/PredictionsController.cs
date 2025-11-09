using System.Security.Claims;
using FantaScommesse.Models.DTOs;
using FantaScommesse.Models.Entities;
using FantaScommesse.Repositories;
using FantaScommesse.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FantaScommesse.API;

/// <summary>
/// API Controller for prediction operations
/// Base path: /api/v1/predictions
/// </summary>
[ApiController]
[Route("api/v1/predictions")]
[Authorize]
public class PredictionsController : ControllerBase
{
    private readonly IPredictionRepository _predictionRepository;
    private readonly IRoundRepository _roundRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IPredictionValidator _validator;

    public PredictionsController(
        IPredictionRepository predictionRepository,
        IRoundRepository roundRepository,
        IMatchRepository matchRepository,
        IPredictionValidator validator)
    {
        _predictionRepository = predictionRepository;
        _roundRepository = roundRepository;
        _matchRepository = matchRepository;
        _validator = validator;
    }

    /// <summary>
    /// Create or update prediction (draft)
    /// POST /api/v1/predictions/{roundId}
    /// </summary>
    [HttpPost("{roundId}")]
    public async Task<IActionResult> CreateOrUpdatePrediction(int roundId, [FromBody] PredictionSubmitDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized();

        // Validate round exists and is not closed
        var round = await _roundRepository.GetByIdAsync(roundId);
        if (round == null || round.IsClosed)
            return BadRequest(new { message = "Giornata non disponibile per i pronostici" });

        // Validate prediction
        var validationResult = _validator.Validate(dto);
        if (!validationResult.IsValid)
            return BadRequest(new { message = "Pronostico non valido", errors = validationResult.Errors });

        // TODO: Get or create participation
        // For now, assume participationId = userId (simplified)
        long participationId = userId;

        // Get or create prediction
        var prediction = await _predictionRepository.GetByParticipationAndRoundAsync(participationId, roundId);

        if (prediction == null)
        {
            prediction = new Prediction
            {
                ParticipationId = participationId,
                RoundId = roundId,
                CreatedUtc = DateTime.UtcNow,
                IsValid = validationResult.IsValid,
                ErrorsCnt = (byte)validationResult.Errors.Count
            };

            var predictionId = await _predictionRepository.CreateAsync(prediction, GetCurrentUsername());
            prediction.PredictionId = predictionId;
        }
        else
        {
            prediction.IsValid = validationResult.IsValid;
            prediction.ErrorsCnt = (byte)validationResult.Errors.Count;
            await _predictionRepository.UpdateAsync(prediction, GetCurrentUsername());
        }

        // Save prediction items
        var items = dto.Items.Select(i => new PredictionItem
        {
            PredictionId = prediction.PredictionId,
            MatchId = i.MatchId,
            Selection = i.Selection.ToUpperInvariant()
        }).ToList();

        await _predictionRepository.SavePredictionItemsAsync(prediction.PredictionId, items, GetCurrentUsername());

        // If not draft, submit
        if (!dto.IsDraft)
        {
            await _predictionRepository.SubmitPredictionAsync(prediction.PredictionId, round.DeadlineUtc, GetCurrentUsername());
        }

        return Ok(new { predictionId = prediction.PredictionId, message = dto.IsDraft ? "Bozza salvata" : "Pronostico inviato" });
    }

    /// <summary>
    /// Submit prediction (final)
    /// POST /api/v1/predictions/{roundId}/submit
    /// </summary>
    [HttpPost("{roundId}/submit")]
    public async Task<IActionResult> SubmitPrediction(int roundId)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized();

        var round = await _roundRepository.GetByIdAsync(roundId);
        if (round == null || round.IsClosed)
            return BadRequest(new { message = "Giornata non disponibile" });

        long participationId = userId; // Simplified
        var prediction = await _predictionRepository.GetByParticipationAndRoundAsync(participationId, roundId);

        if (prediction == null)
            return NotFound(new { message = "Pronostico non trovato" });

        if (prediction.SubmittedUtc != null)
            return BadRequest(new { message = "Pronostico già inviato" });

        var success = await _predictionRepository.SubmitPredictionAsync(prediction.PredictionId, round.DeadlineUtc, GetCurrentUsername());

        if (!success)
            return BadRequest(new { message = "Errore durante l'invio" });

        return Ok(new { message = "Pronostico inviato con successo" });
    }

    /// <summary>
    /// Get prediction for current user and round
    /// GET /api/v1/predictions/{roundId}
    /// </summary>
    [HttpGet("{roundId}")]
    public async Task<IActionResult> GetPrediction(int roundId)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized();

        long participationId = userId; // Simplified
        var prediction = await _predictionRepository.GetByParticipationAndRoundAsync(participationId, roundId);

        if (prediction == null)
            return NotFound();

        var items = await _predictionRepository.GetPredictionItemsAsync(prediction.PredictionId);

        return Ok(new
        {
            predictionId = prediction.PredictionId,
            roundId = prediction.RoundId,
            submittedUtc = prediction.SubmittedUtc,
            isValid = prediction.IsValid,
            isLate = prediction.IsLate,
            items = items.Select(i => new { matchId = i.MatchId, selection = i.Selection })
        });
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private string GetCurrentUsername()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value ?? "UNKNOWN";
    }
}
