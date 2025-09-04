using FakeHubApi.Model.Dto;
using Microsoft.AspNetCore.Mvc;

namespace FakeHubApi.Controllers;

[Route("api/log")]
[ApiController]
public class LogController(ILogger<LogController> _logger) : ControllerBase
{
    [HttpPost]
    public IActionResult LogError([FromBody] FrontendLog log)
    {
        _logger.LogInformation("Frontend log: {Message}, Stack: {Stack}, URL: {Url}, Status: {Status}", 
            log.Message, log.Stack, log.Url, log.Status);
        return Ok();
    }

}