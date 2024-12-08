using FakeHubApi.Model.Entity;

namespace FakeHubApi.Model.Dto;

public class UserDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Role { get; set; }
    public DateTime? CreatedAt { get; set; } = null;
    public Badge Badge { get; set; } = Badge.None;
}
