import { Component, inject, Input } from "@angular/core";
import { Team } from "../../../core/model/team";
import { CommonModule, DatePipe } from "@angular/common";
import { ActivatedRoute, Router } from "@angular/router";

@Component({
  selector: "app-teams",
  standalone: true,
  imports: [CommonModule],
  providers: [DatePipe],
  templateUrl: "./teams.component.html",
  styleUrl: "./teams.component.css",
})
export class TeamsComponent {
  @Input() public teams: Team[] = [];

  private readonly activatedRoute: ActivatedRoute = inject(ActivatedRoute);
  private readonly router: Router = inject(Router);

  public goToTeam(teamName: string): void {
    const name = this.activatedRoute.snapshot.paramMap.get("name");
    this.router.navigate(["/organization/team/view", name, teamName]);
  }
}
