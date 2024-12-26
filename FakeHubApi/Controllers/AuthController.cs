using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FakeHubApi.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
    {
        var response = await authService.Register(model, Role.USER.ToString());
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
    {
        var response = await authService.Login(model);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }
    
    [Authorize(Roles = "SUPERADMIN")]
    [HttpPost("register/admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegistrationRequestDto model)
    {
        var response = await authService.Register(model, Role.ADMIN.ToString());
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }
}
