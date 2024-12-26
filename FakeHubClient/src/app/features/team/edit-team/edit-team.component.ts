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
import { TeamService } from "../../../core/services/team.service";
import { Team } from "../../../core/model/team";

@Component({
  selector: "app-edit-team",
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatInputModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    FormsModule,
  ],
  templateUrl: "./edit-team.component.html",
  styleUrl: "./edit-team.component.css",
})
export class EditTeamComponent {
  private readonly service: TeamService = inject(TeamService);
  private readonly router: Router = inject(Router);
  private readonly activatedRoute: ActivatedRoute = inject(ActivatedRoute);

  private organizationName: string = "";
  private teamName: string = "";

  public teamForm: FormGroup = new FormGroup({
    name: new FormControl("", [Validators.required, Validators.maxLength(100)]),
    description: new FormControl("", [Validators.maxLength(500)]),
  });

  public onSubmit(): void {
    if (this.teamForm.invalid) return;
    this.service
      .editTeam(this.organizationName, this.teamName, this.teamForm.value)
      .subscribe({
        next: () => {
          this.router.navigate([
            "/organization/team/view",
            this.organizationName,
            this.teamForm.value.name,
          ]);
        },
      });
  }

  public cancel(): void {
    this.router.navigate([
      "/organization/team/view",
      this.organizationName,
      this.teamName,
    ]);
  }

  public ngOnInit(): void {
    const name = this.activatedRoute.snapshot.paramMap.get("organizationName");
    const teamName = this.activatedRoute.snapshot.paramMap.get("teamName");
    if (name && teamName) {
      this.organizationName = name;
      this.teamName = teamName;
      this.service.getTeam(name, teamName).subscribe({
        next: (team: Team) => {
          this.teamForm.patchValue(team);
        },
      });
    } else {
      this.router.navigate(["/organization/view", name]);
    }
  }
}
