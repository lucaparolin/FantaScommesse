using FantaScommesse.Models.Entities;
using FantaScommesse.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FantaScommesse.API.Admin;

[ApiController]
[Route("api/v1/admin/matches")]
[Authorize(Roles = "Admin,Organizer")]
public class AdminMatchesController : ControllerBase
{
    private readonly IMatchRepository _matchRepository;

    public AdminMatchesController(IMatchRepository matchRepository)
    {
        _matchRepository = matchRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMatch([FromBody] Match match)
    {
        var id = await _matchRepository.CreateAsync(match, User.Identity?.Name ?? "ADMIN");
        return CreatedAtAction("GetById", "Rounds", new { id }, match);
    }

    [HttpPut("{id}/result")]
    public async Task<IActionResult> UpdateResult(long id, [FromBody] MatchResultDto result)
    {
        var success = await _matchRepository.UpdateResultsAsync(
            id, result.ResultCode!, result.GoalsHome, result.GoalsAway,
            result.GgNg, result.Ou, User.Identity?.Name ?? "ADMIN");

        return success ? Ok() : NotFound();
    }
}

public class MatchResultDto
{
    public string? ResultCode { get; set; }
    public byte? GoalsHome { get; set; }
    public byte? GoalsAway { get; set; }
    public string? GgNg { get; set; }
    public string? Ou { get; set; }
}
