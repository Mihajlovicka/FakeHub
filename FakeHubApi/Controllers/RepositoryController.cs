using FakeHubApi.Model.Dto;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FakeHubApi.Controllers;

[Route("api/repositories")]
[ApiController]
public class RepositoryController(IRepositoryService repositoryService) : ControllerBase
{
    [Authorize(Roles = "USER, ADMIN, SUPERADMIN")]
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RepositoryDto model)
    {
        var response = await repositoryService.Save(model);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [Authorize(Roles = "USER, ADMIN, SUPERADMIN")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllRepositoriesForCurrentUser()
    {
        var response = await repositoryService.GetAllRepositoriesForCurrentUser();
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [Authorize(Roles = "USER, ADMIN, SUPERADMIN")]
    [HttpGet("all/{username}")]
    public async Task<IActionResult> GetAllVisibleRepositoriesForUser([FromRoute] string username)
    {
        var response = await repositoryService.GetAllVisibleRepositoriesForUser(username);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [Authorize(Roles = "USER")]
    [HttpGet("organization/{orgName}")]
    public async Task<IActionResult> GetAllRepositoriesForOrganization([FromRoute] string orgName)
    {
        var response = await repositoryService.GetAllRepositoriesForOrganization(orgName);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpGet("{repositoryId:int}")]
    public async Task<IActionResult> GetRepository([FromRoute] int repositoryId)
    {
        var response = await repositoryService.GetRepository(repositoryId);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [Authorize(Roles = "ADMIN, SUPERADMIN, USER")]
    [HttpDelete("{repositoryId:int}")]
    public async Task<IActionResult> DeleteRepository([FromRoute] int repositoryId)
    {
        var response = await repositoryService.DeleteRepository(repositoryId);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [Authorize(Roles = "USER, ADMIN, SUPERADMIN")]
    [HttpGet("canEdit/{id}")]
    public async Task<IActionResult> CanEditRepository([FromRoute] int id)
    {
        var response = await repositoryService.CanEditRepository(id);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }
    
    [Authorize(Roles = "ADMIN, SUPERADMIN, USER")]
    [HttpPut]
    public async Task<IActionResult> EditRepository([FromBody] EditRepositoryDto data)
    {
        var response = await repositoryService.EditRepository(data);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }
    
    [Authorize(Roles = "USER, ADMIN, SUPERADMIN")]
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? query)
    {
        var response = await repositoryService.Search(query);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpGet("public-repositories")]
    public async Task<IActionResult> GetAllPublicRepositories([FromQuery] string? query)
    {
        var response = await repositoryService.GetAllPublicRepositories(query);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [Authorize(Roles = "USER, ADMIN, SUPERADMIN")]
    [HttpPost("{repositoryId}/add-collaborator")]
    public async Task<IActionResult> AddCollaborator([FromRoute] int repositoryId, [FromBody] AddCollaboratorDto model)
    {
        var response = await repositoryService.AddCollaborator(repositoryId, model.Username);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [Authorize(Roles = "USER, ADMIN, SUPERADMIN")]
    [HttpGet("{id}/collaborators")]
    public async Task<ActionResult<List<UserDto>>> GetCollaborators([FromRoute] int id)
    {
        var response = await repositoryService.GetCollaborators(id);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [Authorize(Roles = "USER, ADMIN, SUPERADMIN")]
    [HttpGet("contributed/{username}")]
    public async Task<IActionResult> GetRepositoriesUserContributed([FromRoute] string username)
{
        var response = await repositoryService.GetRepositoriesUserContributed(username);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }
}