using System.ComponentModel.DataAnnotations.Schema;

namespace FakeHubApi.Model.Entity;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [ForeignKey("Organization")]
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = new();
}
