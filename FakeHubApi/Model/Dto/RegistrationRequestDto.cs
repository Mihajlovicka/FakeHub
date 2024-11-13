using System.ComponentModel.DataAnnotations;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Model.Dto
{
    public class RegistrationRequestDto
    {
        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        [EnumDataType(typeof(Role), ErrorMessage = "Invalid role.")]
        public string Role { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;
    }
}
