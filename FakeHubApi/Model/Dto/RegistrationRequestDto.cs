using System.ComponentModel.DataAnnotations;

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

        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;
    }
}