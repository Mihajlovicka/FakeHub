import { Component, inject } from '@angular/core';
import {FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors, ValidatorFn} from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatButton } from "@angular/material/button";
import { MatCard, MatCardActions, MatCardContent, MatCardHeader, MatCardTitle } from "@angular/material/card";
import { MatError, MatFormField, MatLabel } from "@angular/material/form-field";
import { MatInput } from "@angular/material/input";
import { ReactiveFormsModule } from "@angular/forms";
import {Router, RouterLink} from "@angular/router";
import { ChangePasswordRequest } from "../../../core/model/change-password-request";
import { UserService } from "../../../core/services/user.service";
import { take } from "rxjs";

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [
    CommonModule,
    MatButton,
    MatCard,
    MatCardActions,
    MatCardContent,
    MatCardHeader,
    MatCardTitle,
    MatError,
    MatFormField,
    MatInput,
    MatLabel,
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.css']
})
export class ChangePasswordComponent{
  public changePasswordForm: FormGroup;

  private readonly service: UserService = inject(UserService);
  private readonly fb: FormBuilder = inject(FormBuilder);
  private readonly router: Router = inject(Router);

  public constructor() {
    this.changePasswordForm = this.fb.group({
      oldPassword: ['', { validators: Validators.required, updateOn: 'change' }],
      newPassword: ['', { validators: Validators.required, updateOn: 'change' }],
      newPasswordConfirm: ['', { validators: Validators.required, updateOn: 'change' }]
    },
        {
          validators: this.passwordsMatchValidator
        });
  }

  public onSubmit(): void {
    if (this.changePasswordForm.valid) {
      this.service.changePassword(this.changePasswordForm.value).subscribe({
        next: () => {
          this.router.navigate(["/"]);
        },
      });
    }
  }

  private passwordsMatchValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
    const group = control as FormGroup;
    const newPassControl = group.controls['newPassword'];
    const newPassConf = group.controls['newPasswordConfirm'];

    if (newPassControl.value !== newPassConf.value) {
      newPassConf.setErrors({ passwordMismatch: true });
    } else {
      newPassConf.setErrors(null);
    }
    return null;
  };
}
