using FakeHubApi.Model.Dto;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FakeHubApi.Controllers;

[ApiController]
[Route("api/organization/[controller]")]
[Authorize(Roles = "USER")]
public class TeamController(ITeamService teamService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] TeamDto model)
    {
        var response = await teamService.Add(model);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }
}
