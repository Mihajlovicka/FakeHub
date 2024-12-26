using System.ComponentModel.DataAnnotations;
using FakeHubApi.Validators;

namespace FakeHubApi.Model.Dto;

public class UpdateOrganizationDto
{
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Base64ImageValidation]
    public string ImageBase64 { get; set; } = string.Empty;
}
