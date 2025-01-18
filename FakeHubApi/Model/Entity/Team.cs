using System.ComponentModel.DataAnnotations.Schema;

namespace FakeHubApi.Model.Entity;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public TeamRole TeamRole { get; set; } = TeamRole.ReadOnly;

    [ForeignKey("Organization")]
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = new();

    [ForeignKey("Repository")]
    public int RepositoryId { get; set; }
    public Repository Repository { get; set; } = new();
    public bool Active { get; set; } = true;
    public List<User> Users { get; set; } = new();
}
