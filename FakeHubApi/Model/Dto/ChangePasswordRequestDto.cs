namespace FakeHubApi.Model.Dto;

public class ChangePasswordRequestDto
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string NewPasswordConfirm  { get; set; } = string.Empty;
}