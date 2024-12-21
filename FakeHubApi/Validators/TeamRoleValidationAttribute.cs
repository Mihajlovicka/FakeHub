using System.ComponentModel.DataAnnotations;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Validators;

public class TeamRoleValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || !Enum.IsDefined(typeof(TeamRole), value))
        {
            return new ValidationResult("Invalid team role.");
        }

        return ValidationResult.Success!;
    }
}
