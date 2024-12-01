﻿namespace FakeHubApi.Model.Dto;

public class UserProfileResponseDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; } = null;
}
