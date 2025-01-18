import { AbstractControl, ValidationErrors } from "@angular/forms";

export class RepositoryValidationHelper {
    public static noWhitespaceValidator(control: AbstractControl): ValidationErrors | null {
        if (control.value && control.value.indexOf(' ') >= 0) {
            return { whitespace: true };
        }
        return null;
    }

    public static harborNameValidator(control: AbstractControl): ValidationErrors | null {
        const value = control.value as string;
        if (!value) return null;

        const regex = /^(?![.-])(?!.*[.-]{2})[a-z0-9._-]{2,255}(?<![.-])$/;

        if (!regex.test(value)) {
            return { invalidHarborName: true };
        }
        return null;
    }
}