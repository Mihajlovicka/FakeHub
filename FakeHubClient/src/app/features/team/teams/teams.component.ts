import {Component, EventEmitter, inject, Input, Output} from "@angular/core";
import { Team } from "../../../core/model/team";
import { CommonModule, DatePipe } from "@angular/common";
import { ActivatedRoute, Router } from "@angular/router";
import {
  ConfirmationDialogComponent
} from "../../../shared/components/confirmation-dialog/confirmation-dialog.component";
import {MatDialog} from "@angular/material/dialog";
import {OrganizationService} from "../../../core/services/organization.service";
import {UserProfileResponseDto} from "../../../core/model/user";
import {MatIcon} from "@angular/material/icon";

@Component({
  selector: "app-teams",
  standalone: true,
  imports: [CommonModule, MatIcon],
  providers: [DatePipe],
  templateUrl: "./teams.component.html",
  styleUrl: "./teams.component.css",
})
export class TeamsComponent {
  @Input() public teams: Team[] = [];
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
}
