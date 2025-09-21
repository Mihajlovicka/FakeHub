import { Component, EventEmitter, inject, Input, Output } from "@angular/core";
import { Team } from "../../../core/model/team";
import { CommonModule, DatePipe } from "@angular/common";
import { ActivatedRoute, Router } from "@angular/router";
import {
  ConfirmationDialogComponent
} from "../../../shared/components/confirmation-dialog/confirmation-dialog.component";
import { MatDialog } from "@angular/material/dialog";
import { OrganizationService } from "../../../core/services/organization.service";
import { UserProfileResponseDto } from "../../../core/model/user";
import { MatIcon } from "@angular/material/icon";
import { FormsModule } from "@angular/forms";

@Component({
  selector: "app-teams",
  standalone: true,
  imports: [CommonModule, MatIcon, FormsModule],
  providers: [DatePipe],
  templateUrl: "./teams.component.html",
  styleUrl: "./teams.component.css",
})
export class TeamsComponent {
  public searchQuery: string = "";
  public filteredTeams: Team[] = [];


  @Input() public set teams(value: Team[]) {
    this._teams = value;
    this.filteredTeams = [...value];
  }

  public get teams(): Team[] {
    return this._teams;
  }

  private _teams: Team[] = [];

  private readonly dialog = inject(MatDialog);
  private readonly service = inject(OrganizationService);

  @Input() public organizationName: string = "";
  @Input() public isOwner: boolean = false;
  @Output() deleteUserEvent = new EventEmitter<UserProfileResponseDto>();
  @Output() deleteTeamEvent = new EventEmitter<Team>();


  private readonly activatedRoute: ActivatedRoute = inject(ActivatedRoute);
  private readonly router: Router = inject(Router);

  public openDeleteTeamModal(team: Team): void {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      data: {
        title: "Delete member",
        description: "Would you like to delete team \"" + team.name + "\" from organization?"
      },
    });
    dialogRef.afterClosed().subscribe((isConfirmed) => {
      if (isConfirmed) {
        this.service.deleteTeam(this.organizationName, team.name).subscribe(
          _ => {
            const userIndex = this.teams.findIndex(u => u.name == team.name);
            this.teams.splice(userIndex, 1);
            this.deleteTeamEvent.emit(team);
            this.router.navigate(['organization/view/', this.organizationName]);
          }
        )
      }
    });
  }

  public goToTeam(teamName: string): void {
    const name = this.activatedRoute.snapshot.paramMap.get("name");
    this.router.navigate(["/organization/team/view", name, teamName]);
  }

  public search(): void {
    const query = this.searchQuery.trim().toLowerCase();

    if (!query) {
      this.filteredTeams = [...this.teams];
      return;
    }

    let result: Team[] = [];

    const getRepoName = (t: Team) => t.repository?.name?.toLowerCase() ?? "";
    const getOwner = (t: Team) => t.owner?.toLowerCase() ?? "";
    const getRole = (t: Team) => t.teamRole?.toString().toLowerCase() ?? "";

    const exactMatches = this.teams.filter(t =>
      t.name.toLowerCase() === query ||
      (t.description ?? "").toLowerCase() === query ||
      getRepoName(t) === query ||
      getOwner(t) === query ||
      getRole(t) === query
    );

    const partialMatches = this.teams.filter(t =>
      t.name.toLowerCase().includes(query) ||
      (t.description ?? "").toLowerCase().includes(query) ||
      getRepoName(t).includes(query) ||
      getOwner(t).includes(query) ||
      getRole(t).includes(query)
    );

    result = Array.from(new Set([...exactMatches, ...partialMatches]));

    result = result.sort((a, b) => {
      const aExact =
        a.name.toLowerCase() === query ||
        (a.description ?? "").toLowerCase() === query ||
        getRepoName(a) === query ||
        getOwner(a) === query ||
        getRole(a) === query;

      const bExact =
        b.name.toLowerCase() === query ||
        (b.description ?? "").toLowerCase() === query ||
        getRepoName(b) === query ||
        getOwner(b) === query ||
        getRole(b) === query;

      const aPartial =
        a.name.toLowerCase().includes(query) ||
        (a.description ?? "").toLowerCase().includes(query) ||
        getRepoName(a).includes(query) ||
        getOwner(a).includes(query) ||
        getRole(a).includes(query);

      const bPartial =
        b.name.toLowerCase().includes(query) ||
        (b.description ?? "").toLowerCase().includes(query) ||
        getRepoName(b).includes(query) ||
        getOwner(b).includes(query) ||
        getRole(b).includes(query);

      if (aExact !== bExact) return aExact ? -1 : 1;
      if (aPartial !== bPartial) return aPartial ? -1 : 1;

      return 0;
    });

    this.filteredTeams = result;
  }

}
