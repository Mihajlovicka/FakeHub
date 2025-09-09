namespace FakeHubApi.Model.Settings;

public class HarborSettings
{
    public string Url { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public int Duration { get; set; }
}