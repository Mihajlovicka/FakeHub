using FakeHubApi.Model.Dto;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Model.Entity;
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

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto changePasswordRequestDto)
    {
        var response = await authService.ChangePassword(changePasswordRequestDto);
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

    [Authorize(Roles = "USER,ADMIN")]
    [HttpPost("change-email")]
    public async Task<IActionResult> ChangeUserEmail(ChangeEmailRequestDto changeEmailRequestDto)
    {
        var response = await authService.ChangeEmailAsync(changeEmailRequestDto);

        if (response.Success)
        {
            return Ok(response);
        }

        return BadRequest(response);
    }
}
