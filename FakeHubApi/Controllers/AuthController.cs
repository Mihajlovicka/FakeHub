using FakeHubApi.Model.Dto;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FakeHubApi.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [Authorize(Policy = "NoRolePolicy")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
    {
        var response = await authService.Register(model);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [Authorize(Policy = "NoRolePolicy")]
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
        var response = await authService.Register(model, "ADMIN");
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpGet("profile/{username}")]
    public async Task<IActionResult> GetUserProfileByUsername(string username)
    {
        if (string.IsNullOrEmpty(username)) return BadRequest(ResponseBase.ErrorResponse("Username is empty"));

        var response = await authService.GetUserProfileByUsernameAsync(username);

        if (response.Success)
        {
            return Ok(response);
        }

        return NotFound(response);
    }
}
