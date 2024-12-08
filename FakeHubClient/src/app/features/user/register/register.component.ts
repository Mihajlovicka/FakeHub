import {Component, inject} from "@angular/core";
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
import {UserRole} from "../../../core/model/user-role";

@Component({
  selector: "app-register",
  standalone: true,
  imports: [
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    ReactiveFormsModule,
    FormsModule,
    RouterLink,
  ],
  templateUrl: "./register.component.html",
  styleUrl: "./register.component.css",
})
export class RegisterComponent {
  private readonly service: UserService = inject(UserService);
  private readonly router: Router = inject(Router);

  public isSuperAdmin: boolean = this.service.isSuperAdminLoggedIn();

  public registerForm: FormGroup = new FormGroup({
    username: new FormControl("", Validators.required),
    email: new FormControl("", [Validators.required, Validators.email]),
    password: new FormControl("", Validators.required),
  });

  public onSubmit(): void {
    if (this.registerForm.invalid) return;

    this.service.register(this.registerForm.value).subscribe({
      next: () => {
        if(this.service.isSuperAdminLoggedIn()) this.router.navigate(["/"]);
        else this.router.navigate(["/login"]);
      },
    });
  }
}