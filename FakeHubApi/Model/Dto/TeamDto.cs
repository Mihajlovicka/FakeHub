using System.ComponentModel.DataAnnotations;
using FakeHubApi.Validators;

namespace FakeHubApi.Model.Dto;

public class TeamDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string OrganizationName { get; set; } = string.Empty;

    [TeamRoleValidation]
    public string TeamRole { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public string? Owner { get; set; }

    public IEnumerable<UserDto> Users { get; set; } = new List<UserDto>();
}
