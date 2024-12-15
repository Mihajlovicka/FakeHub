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

    [HttpGet("{organizationName}/{teamName}")]
    public async Task<IActionResult> Get(
        [FromRoute] string organizationName,
        [FromRoute] string teamName
    )
    {
        var response = await teamService.Get(organizationName, teamName);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpPut("{organizationName}/{teamName}")]
    public async Task<IActionResult> Update(
        [FromRoute] string organizationName,
        [FromRoute] string teamName,
        [FromBody] UpdateTeamDto model
    )
    {
        var response = await teamService.Update(organizationName, teamName, model);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }
}
