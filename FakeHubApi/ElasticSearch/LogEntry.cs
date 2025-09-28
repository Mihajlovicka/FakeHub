namespace FakeHubApi.ElasticSearch;

public class LogEntry(string timestamp, string level, string message, string exception, string application)
{
    public string Timestamp { get; set; } = timestamp;
    public string Level { get; set; } = level;
    public string Message { get; set; } = message;
    public string Exception { get; set; } = exception;
    public string Application { get; set; } = application;
}