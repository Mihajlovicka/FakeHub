using Microsoft.AspNetCore.Identity;

namespace FakeHubApi.Model.Entity;

public class User : IdentityUser<int>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Organization> Organizations { get; set; } = new List<Organization>();
    public Badge Badge { get; set; } = Badge.None;
}
