using Microsoft.AspNetCore.Mvc;

namespace FakeHubApi.ElasticSearch;

[ApiController]
[Route("api/elasticsearch")]
public class ElasticSearchController(ElasticSearchService elasticService) : ControllerBase
{
    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs([FromQuery] int size = 100)
    {
        try
        {
            var logs = await elasticService.GetAllLogsAsync(size);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
    
    [HttpGet("search")]
    public async Task<IActionResult> SearchLogs(
        [FromQuery] string? query = null,
        [FromQuery] string? level = null,
        [FromQuery] string? from = null,
        [FromQuery] string? to = null,
        [FromQuery] int size = 100)
    {
        try
        {
            DateTime? fromDate = string.IsNullOrWhiteSpace(from) ? null : DateTime.Parse(from, null, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal);
            DateTime? toDate = string.IsNullOrWhiteSpace(to) ? null : DateTime.Parse(to, null, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal);

            var logs = await elasticService.SearchLogsAsync(query, level, fromDate, toDate, size);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}