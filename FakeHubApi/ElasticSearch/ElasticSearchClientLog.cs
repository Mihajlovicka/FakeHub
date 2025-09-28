using Nest;

namespace FakeHubApi.ElasticSearch;

public class ElasticSearchClientLog(DateTime timestamp, string level, string message, LogFields fields)
{
    [PropertyName("@timestamp")]
    public DateTime Timestamp { get; set; } = timestamp;

    [PropertyName("level")]
    public string Level { get; set; } = level;

    [PropertyName("message")]
    public string Message { get; set; } = message;

    [PropertyName("fields")]
    public LogFields Fields { get; set; } = fields; 
}

public class LogFields(string application)
{
    [PropertyName("Application")]
    public string Application { get; set; } = application;
}