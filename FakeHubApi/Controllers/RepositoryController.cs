using FakeHubApi.Model.Dto;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FakeHubApi.Controllers;

[Route("api/repositories")]
[ApiController]
public class RepositoryController(IRepositoryService repositoryService) : ControllerBase
{
    [Authorize(Roles = "USER")]
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
}