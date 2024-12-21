using System.ComponentModel.DataAnnotations;
using FakeHubApi.Validators;

namespace FakeHubApi.Model.Dto;

public class OrganizationDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Base64ImageValidation]
    public string ImageBase64 { get; set; } = string.Empty;

    public string? Owner { get; set; } = null;

    public List<TeamDto> Teams { get; set; } = new();

    public List<UserDto> Users { get; set; } = new();
}
