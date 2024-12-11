using System.ComponentModel.DataAnnotations;

namespace FakeHubApi.Model.Dto;

public class TeamDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    [Required]
    public string OrganizationName { get; set; }

    public DateTime CreatedAt { get; set; }
}
