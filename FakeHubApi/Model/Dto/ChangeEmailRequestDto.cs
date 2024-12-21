namespace FakeHubApi.Model.Dto;

public class ChangeEmailRequestDto
{
    public string NewEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
