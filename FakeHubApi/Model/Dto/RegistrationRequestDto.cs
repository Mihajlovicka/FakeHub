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
        public string Email { get; set; }

        public string Password { get; set; }

        [EnumDataType(typeof(Role), ErrorMessage = "Invalid role.")]
        public string Role { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string Username { get; set; }
    }
}