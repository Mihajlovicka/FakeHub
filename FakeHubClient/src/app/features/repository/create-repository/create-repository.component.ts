import { Component, inject, OnInit } from "@angular/core";
import {
  AbstractControl,
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCardModule } from "@angular/material/card";
import { MatInputModule } from "@angular/material/input";
import { MatRadioModule } from "@angular/material/radio";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatSelectModule } from "@angular/material/select";
import { MatOptionModule } from "@angular/material/core";
import { CommonModule } from "@angular/common";
import { OrganizationService } from "../../../core/services/organization.service";
import { IdNamePair } from "../../../core/model/id-name-pair";
import { RepositoryService } from "../../../core/services/repository.service";
import { RepositoryOwnedBy } from "../../../core/model/repository";
import { Router } from "@angular/router";
import { UserService } from "../../../core/services/user.service";
import { RepositoryValidationHelper } from "../../../core/helpers/repository-validation.helper";

@Component({
  selector: "app-create-repository",
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatInputModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    FormsModule,
    MatRadioModule,
    MatFormFieldModule,
    MatSelectModule,
    MatOptionModule,
    CommonModule,
  ],
  templateUrl: "./create-repository.component.html",
  styleUrl: "./create-repository.component.css",
})
export class CreateRepositoryComponent implements OnInit {
  private readonly organizationService: OrganizationService = inject(OrganizationService);
  private readonly repositoryService: RepositoryService = inject(RepositoryService);
  private userService: UserService = inject(UserService);
  private readonly router: Router = inject(Router);

  public isAdminLoggedIn: boolean = this.userService.isAdminLoggedIn();
  public isSuperAdminLoggedIn: boolean = this.userService.isSuperAdminLoggedIn();

  public repositoryForm: FormGroup = new FormGroup({
    name: new FormControl("", [
      Validators.required,
      Validators.maxLength(100),
      RepositoryValidationHelper.noWhitespaceValidator,
      RepositoryValidationHelper.harborNameValidator
    ]),
    description: new FormControl("", [Validators.maxLength(500)]),
    isPrivate: new FormControl({ value: false, disabled: this.isAdminLoggedIn || this.isSuperAdminLoggedIn }, Validators.required),
    ownerId: new FormControl({ value: null, disabled: this.isAdminLoggedIn || this.isSuperAdminLoggedIn }, Validators.required),
  });

  public ownerSelection: IdNamePair[] = [{ id: -1, name: "me" }];

  public ngOnInit(): void {
    if (this.isAdminLoggedIn || this.isSuperAdminLoggedIn) {
      this.ownerSelection = [{ id: -2, name: "me" }];
      this.repositoryForm.patchValue({ ownerId: -2 });
    } else {
      this.organizationService.getByUserIdName().subscribe((data) => {
        this.ownerSelection = [...this.ownerSelection, ...data];
      });
    }
  }

  public onSubmit(): void {
    if (this.repositoryForm.invalid) return;

    const data = this.repositoryForm.value;

    if (!(this.isAdminLoggedIn || this.isSuperAdminLoggedIn))
      data.ownedBy = data.ownerId === -1 ? RepositoryOwnedBy.USER : RepositoryOwnedBy.ORGANIZATION;

    this.repositoryService.save(data).subscribe((res) => {
      this.router.navigate(["/repositories"]);
    });
  }
}
