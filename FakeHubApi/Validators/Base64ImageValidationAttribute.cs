using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FakeHubApi.Validators;

public class Base64ImageValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrEmpty(value as string))
        {
            return ValidationResult.Success!;
        }

        var base64Pattern = @"^data:image\/(jpeg|png|gif|bmp|webp);base64,[a-zA-Z0-9+/]+={0,2}$";
        var stringValue = value as string;
        if (stringValue != null && Regex.IsMatch(stringValue, base64Pattern))
        {
            return ValidationResult.Success!;
        }

        return new ValidationResult("The image must be a valid Base64 encoded image.");
    }
}
