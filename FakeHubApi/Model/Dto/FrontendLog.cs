namespace FakeHubApi.Model.Dto;

public class FrontendLog
{
    public string Message { get; set; } = string.Empty;
    public string? Stack { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public int? Status { get; set; }
    public DateTime? Timestamp { get; set; }
}