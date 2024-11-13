import { Component, inject } from "@angular/core";
import { MatInputModule } from "@angular/material/input";
import { MatButtonModule } from "@angular/material/button";
import { MatCardModule } from "@angular/material/card";
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { RouterLink } from "@angular/router";
import { UserService } from "../../../core/services/user.service";
import { Router } from "@angular/router";

@Component({
  selector: "app-login",
  standalone: true,
  imports: [
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    ReactiveFormsModule,
    FormsModule,
    RouterLink,
  ],
  templateUrl: "./login.component.html",
  styleUrl: "./login.component.css",
})
export class LoginComponent {
  service: UserService = inject(UserService);
  router: Router = inject(Router);

  loginForm: FormGroup = new FormGroup({
    email: new FormControl("", [Validators.required, Validators.email]),
    password: new FormControl("", Validators.required),
  });

  onSubmit(): void {
    if (this.loginForm.invalid) return;

    this.service.login(this.loginForm.value).subscribe({
      next: () => {
        this.router.navigate(["/"]);
      },
    });
  }
}
