using Microsoft.AspNetCore.Identity;

namespace FakeHubApi.Model.Entity;

public class User : IdentityUser<int>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Badge Badge { get; set; } = Badge.None;
    public ICollection<Organization> OwnedOrganizations { get; set; } = new List<Organization>();
    public List<Organization> Organizations =>
        UserOrganizations.Where(uo => uo.Active).Select(uo => uo.Organization).ToList();
    public List<UserOrganization> UserOrganizations { get; set; } = new();
}
