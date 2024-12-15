import { Component, inject } from "@angular/core";
import {
  ReactiveFormsModule,
  FormsModule,
  FormGroup,
  FormControl,
  Validators,
} from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCardModule } from "@angular/material/card";
import { MatInputModule } from "@angular/material/input";
import { ActivatedRoute, Router } from "@angular/router";
import { OrganizationService } from "../../../core/services/organization.service";
import { Organization } from "../../../core/model/organization";
import { HelperService } from "../../../core/services/helper.service";

@Component({
  selector: "app-edit-organization",
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatInputModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    FormsModule,
  ],
  templateUrl: "./edit-organization.component.html",
  styleUrl: "./edit-organization.component.css",
})
export class EditOrganizationComponent {
  private readonly service: OrganizationService = inject(OrganizationService);
  private readonly router: Router = inject(Router);
  private readonly activatedRoute: ActivatedRoute = inject(ActivatedRoute);
  private readonly helperService: HelperService = inject(HelperService);

  public organizationForm: FormGroup = new FormGroup({
    name: new FormControl(""),
    description: new FormControl("", [Validators.maxLength(500)]),
    imageBase64: new FormControl(""),
  });

  public onFileChange(event: Event): void {
    const fileInput = event.target as HTMLInputElement;
    const file = fileInput.files?.[0];
    if (file) {
      this.helperService.readImageBase64(file).then((base64: string) => {
        this.organizationForm.patchValue({
          imageBase64: base64,
        });
      });
    }
  }

  public onSubmit(): void {
    if (this.organizationForm.invalid) return;

    this.service.editOrganization(this.organizationForm.value).subscribe({
      next: () => {
        this.router.navigate([
          "/organization/view",
          this.organizationForm.value.name,
        ]);
      },
    });
  }

  public cancel(): void {
    this.router.navigate([
      "/organization/view",
      this.organizationForm.value.name,
    ]);
  }

  public ngOnInit(): void {
    const name = this.activatedRoute.snapshot.paramMap.get("name");
    if (name) {
      this.service.getOrganization(name).subscribe({
        next: (organization: Organization) => {
          this.organizationForm.patchValue(organization);
        },
      });
    }
  }
}
