import { Component, inject } from "@angular/core";
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { Router } from "@angular/router";
import { OrganizationService } from "../../../core/services/organization.service";
import { MatInputModule } from "@angular/material/input";
import { MatCardModule } from "@angular/material/card";
import { MatButtonModule } from "@angular/material/button";

@Component({
  selector: "app-add-organization",
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatInputModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    FormsModule,
  ],
  templateUrl: "./add-organization.component.html",
  styleUrl: "./add-organization.component.css",
})
export class AddOrganizationComponent {
  private readonly service: OrganizationService = inject(OrganizationService);
  private readonly router: Router = inject(Router);

  public organizationForm: FormGroup = new FormGroup({
    name: new FormControl("", [Validators.required, Validators.maxLength(100)]),
    description: new FormControl("", [Validators.maxLength(500)]),
    imageBase64: new FormControl(""),
  });

  public onFileChange(event: Event): void {
    const fileInput = event.target as HTMLInputElement;
    if (fileInput.files && fileInput.files[0]) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.organizationForm.patchValue({
          imageBase64: e.target.result,
        });
      };
      reader.readAsDataURL(fileInput.files[0]);
    }
  }

  public onSubmit(): void {
    if (this.organizationForm.invalid) return;

    this.service.addOrganization(this.organizationForm.value).subscribe({
      next: () => {
        this.router.navigate(["/organizations"]);
      },
    });
  }
}
