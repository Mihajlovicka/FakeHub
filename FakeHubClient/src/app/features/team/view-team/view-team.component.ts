import { Component, inject } from "@angular/core";
import { Team } from "../../../core/model/team";
import { ActivatedRoute, Router, RouterModule } from "@angular/router";
import { TeamService } from "../../../core/services/team.service";
import { CommonModule, DatePipe } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatMenuModule } from "@angular/material/menu";
import { MatTabsModule } from "@angular/material/tabs";
import { TeamsComponent } from "../teams/teams.component";
import { UserService } from "../../../core/services/user.service";

@Component({
  selector: "app-view-team",
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatMenuModule,
    MatButtonModule,
    RouterModule,
    MatTabsModule,
  ],
  providers: [DatePipe],
  templateUrl: "./view-team.component.html",
  styleUrl: "./view-team.component.css",
})
export class ViewTeamComponent {
  private readonly activatedRoute: ActivatedRoute = inject(ActivatedRoute);
  private readonly router: Router = inject(Router);
  private readonly teamService: TeamService = inject(TeamService);
  private readonly userService: UserService = inject(UserService);
  private organizationName: string = "";

  public team: Team = {
    name: "",
    description: "",
    createdAt: new Date(),
    owner: "",
  };

  public edit(): void {
    this.router.navigate([
      "/organization/team/edit",
      this.organizationName,
      this.team.name,
    ]);
  }

  public isOwner(): boolean {
    return this.team.owner === this.userService.getUserName();
  }

  ngOnInit(): void {
    const name = this.activatedRoute.snapshot.paramMap.get("organizationName");
    const teamName = this.activatedRoute.snapshot.paramMap.get("teamName");
    if (name && teamName) {
      this.organizationName = name;
      this.teamService.getTeam(name, teamName).subscribe({
        next: (team: Team) => {
          this.team = team;
        },
      });
    }
  }
}
