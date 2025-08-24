import { Component, inject, OnInit } from "@angular/core";
import {
  ReactiveFormsModule,
  FormsModule,
  FormControl,
  FormGroup,
  Validators,
} from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCardModule } from "@angular/material/card";
import { MatInputModule } from "@angular/material/input";
import { ActivatedRoute, Router } from "@angular/router";
import { CommonModule } from "@angular/common";
import { MatSelectModule } from "@angular/material/select";
import { TeamService } from "../../../core/services/team.service";
import { TeamRole } from "../../../core/model/team";
import { Repository } from "../../../core/model/repository";
import { RepositoryService } from "../../../core/services/repository.service";

@Component({
  selector: "app-add-team",
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatInputModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    FormsModule,
    CommonModule,
    MatSelectModule,
  ],
  templateUrl: "./add-team.component.html",
  styleUrl: "./add-team.component.css",
})
export class AddTeamComponent implements OnInit {
  private readonly service: TeamService = inject(TeamService);
  private readonly repositoryService: RepositoryService = inject(RepositoryService);
  private readonly router: Router = inject(Router);
  private readonly activatedRoute: ActivatedRoute = inject(ActivatedRoute);

  public organizationName: string = "";
  public teamRoles: string[] = Object.values(TeamRole);
  public repositories: Repository[] = [];

  public teamForm: FormGroup = new FormGroup({
    name: new FormControl("", [Validators.required, Validators.maxLength(100)]),
    description: new FormControl("", [Validators.maxLength(500)]),
    teamRole: new FormControl("ReadOnly", [Validators.required]),
    repository: new FormControl(null, [Validators.required]),
  });

  public onSubmit(): void {
    if (this.teamForm.invalid) return;
    this.teamForm.value.organizationName = this.organizationName;
    this.service.addTeam(this.teamForm.value).subscribe({
      next: () => {
        this.router.navigate(["/organization/view", this.organizationName]);
      },
    });
  }

  public cancel(): void {
    this.router.navigate(["/organization/view", this.organizationName]);
  }

  public ngOnInit(): void {
    const name = this.activatedRoute.snapshot.paramMap.get("organizationName");
    if (name) {
      this.organizationName = name;
    }
    this.repositoryService.GetAllRepositoriesForOrganization(this.organizationName).subscribe({
      next: (repos) => {
        this.repositories = repos;
        if (this.repositories.length > 0) {
          this.teamForm.controls["repository"].setValue(
            this.repositories[0]
          );
        }
      }
    });
  }
}
