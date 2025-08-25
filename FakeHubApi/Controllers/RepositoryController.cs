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

    [Authorize(Roles = "USER, ADMIN, SUPERADMIN")]
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
}