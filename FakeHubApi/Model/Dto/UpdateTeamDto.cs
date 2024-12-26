using System.ComponentModel.DataAnnotations;

namespace FakeHubApi.Model.Dto;

public class UpdateTeamDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
}
