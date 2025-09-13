using FakeHubApi.Model.Dto;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FakeHubApi.Controllers;

[Route("api/repositories/[controller]")]
[Authorize(Roles = "ADMIN, SUPERADMIN, USER")]
[ApiController]
public class TagController(ITagService tagService) : ControllerBase
{
    [HttpGet("{repositoryId}/canUserDelete")]
    public async Task<IActionResult> canDelete([FromRoute] int repositoryId)
    {
        var response = await tagService.CanDelete(repositoryId);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpDelete("{repositoryId}")]
    public async Task<IActionResult> deleteTag([FromRoute] int repositoryId, [FromBody] ArtifactDto artifact)
    {
        var response = await tagService.DeleteTag(artifact, repositoryId);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

}