using FakeHubApi.Filters;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Mvc;
using FakeHubApi.Model.Dto;

namespace FakeHubApi.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
    {

        var response = await authService.Register(model);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }
}

