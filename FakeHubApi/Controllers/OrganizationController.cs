using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeHubApi.Model.Dto;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FakeHubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "USER")]
public class OrganizationController(IOrganizationService organizationService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] OrganizationDto model)
    {
        var response = await organizationService.Add(model);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> Get([FromRoute] string name)
    {
        var response = await organizationService.GetByName(name);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpPut("{name}")]
    public async Task<IActionResult> Update(
        [FromRoute] string name,
        [FromBody] UpdateOrganizationDto model
    )
    {
        var response = await organizationService.Update(name, model);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetByUser()
    {
        var response = await organizationService.GetByUser();
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpGet("")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        var response = await organizationService.Search(query);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }
}
