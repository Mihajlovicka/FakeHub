using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FakeHubApi.Model.Entity;

public class ApplicationUser : IdentityUser<int>
{
}