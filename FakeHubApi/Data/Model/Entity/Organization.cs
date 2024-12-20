using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FakeHubApi.Model.Entity;

public class Organization
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageBase64 { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    [ForeignKey("Owner")]
    public int OwnerId { get; set; }
    public User Owner { get; set; } = new();
    public List<Team> Teams { get; set; } = new();
    public List<User> Users
    {
        get => UserOrganizations.Where(uo => uo.Active).Select(uo => uo.User).ToList();
        set
        {
            // Set UserOrganizations based on the value assigned to Users
            UserOrganizations = value.Select(u => new UserOrganization
            {
                User = u,
                Active = true // Assuming you want all users to be active when adding them
            }).ToList();
        }
    }

    public bool Active { get; set; } = true;
    public List<UserOrganization> UserOrganizations { get; set; } = new();
}
