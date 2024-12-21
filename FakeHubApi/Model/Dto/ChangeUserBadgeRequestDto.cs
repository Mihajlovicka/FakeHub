using FakeHubApi.Model.Entity;

namespace FakeHubApi.Model.Dto;

public class ChangeUserBadgeRequestDto
{
    public Badge Badge { get; set; }
    public string Username { get; set; } = string.Empty;
}
