namespace FakeHubApi.Model.Entity;

public class UserOrganization
{
    public int UserId { get; set; }
    public User User { get; set; } = new();

    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = new();

    public bool Active { get; set; } = true;
}