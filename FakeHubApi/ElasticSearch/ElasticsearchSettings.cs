namespace FakeHubApi.ElasticSearch;

public class ElasticsearchSettings(string uri, string username, string password)
{
    public string Uri { get; set; } = uri;
    public string Username { get; set; } = username;
    public string Password { get; set; } = password;
}
