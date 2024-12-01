import { Component, inject } from '@angular/core';
import {FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors, ValidatorFn, FormControl} from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatButton } from "@angular/material/button";
import { MatCard, MatCardActions, MatCardContent, MatCardHeader, MatCardTitle } from "@angular/material/card";
import { MatError, MatFormField, MatLabel } from "@angular/material/form-field";
import { MatInput } from "@angular/material/input";
import { ReactiveFormsModule } from "@angular/forms";
import { UserService } from '../../../core/services/user.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-email-edit',
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
    ReactiveFormsModule,],
  templateUrl: './email-edit.component.html',
  styleUrl: './email-edit.component.css'
})
export class EmailEditComponent {
  private readonly formBuilder: FormBuilder = inject(FormBuilder);
  private readonly userService: UserService = inject(UserService);
  private readonly router: Router = inject(Router);
  public changeEmailForm: FormGroup;

  public constructor() {
    this.changeEmailForm = this.formBuilder.group({
      newEmail: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
  }

  public onSubmit(): void {
    if (this.changeEmailForm.valid) {
      this.userService.changeEmail(this.changeEmailForm.value).subscribe({
        next: () => {
          this.router.navigate(["/settings"]);
        },
      });
    }
  }
}
