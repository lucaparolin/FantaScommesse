using FantaScommesse.Models.Entities;
using FantaScommesse.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FantaScommesse.API.Admin;

[ApiController]
[Route("api/v1/admin/seasons")]
[Authorize(Roles = "Admin,Organizer")]
public class AdminSeasonsController : ControllerBase
{
    private readonly ISeasonRepository _seasonRepository;

    public AdminSeasonsController(ISeasonRepository seasonRepository)
    {
        _seasonRepository = seasonRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var seasons = await _seasonRepository.GetAllAsync();
        return Ok(seasons);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var season = await _seasonRepository.GetByIdAsync(id);
        return season == null ? NotFound() : Ok(season);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Season season)
    {
        var id = await _seasonRepository.CreateAsync(season, User.Identity?.Name ?? "ADMIN");
        return CreatedAtAction(nameof(GetById), new { id }, season);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Season season)
    {
        season.SeasonId = id;
        var success = await _seasonRepository.UpdateAsync(season, User.Identity?.Name ?? "ADMIN");
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _seasonRepository.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}
