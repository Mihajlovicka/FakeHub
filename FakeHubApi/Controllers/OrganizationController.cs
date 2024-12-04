using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeHubApi.Model.Dto;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Mvc;

namespace FakeHubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
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
}
