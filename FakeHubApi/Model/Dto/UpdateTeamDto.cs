using System.ComponentModel.DataAnnotations;

namespace FakeHubApi.Model.Dto;

public class UpdateTeamDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }
}
