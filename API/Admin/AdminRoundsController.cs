using FantaScommesse.Models.Entities;
using FantaScommesse.Repositories;
using FantaScommesse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FantaScommesse.API.Admin;

[ApiController]
[Route("api/v1/admin/rounds")]
[Authorize(Roles = "Admin,Organizer")]
public class AdminRoundsController : ControllerBase
{
    private readonly IRoundRepository _roundRepository;
    private readonly IScoringService _scoringService;

    public AdminRoundsController(IRoundRepository roundRepository, IScoringService scoringService)
    {
        _roundRepository = roundRepository;
        _scoringService = scoringService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRound([FromBody] Round round)
    {
        var id = await _roundRepository.CreateAsync(round, User.Identity?.Name ?? "ADMIN");
        return CreatedAtAction("GetById", "Rounds", new { id }, round);
    }

    [HttpPost("{id}/close")]
    public async Task<IActionResult> CloseRound(int id)
    {
        var round = await _roundRepository.GetByIdAsync(id);
        if (round == null) return NotFound();

        round.IsClosed = true;
        await _roundRepository.UpdateAsync(round, User.Identity?.Name ?? "ADMIN");

        // Calculate scores
        await _scoringService.CalculateRoundScoresAsync(id, User.Identity?.Name ?? "ADMIN");

        return Ok(new { message = "Round closed and scores calculated" });
    }

    [HttpPost("{id}/publish")]
    public async Task<IActionResult> PublishRound(int id)
    {
        var round = await _roundRepository.GetByIdAsync(id);
        if (round == null) return NotFound();

        round.IsPublished = true;
        await _roundRepository.UpdateAsync(round, User.Identity?.Name ?? "ADMIN");

        return Ok(new { message = "Round published" });
    }
}
