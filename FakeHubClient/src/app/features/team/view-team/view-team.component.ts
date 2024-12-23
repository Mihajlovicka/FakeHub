import { Component, inject, OnDestroy, OnInit } from "@angular/core";
import { Team, TeamRole } from "../../../core/model/team";
import { ActivatedRoute, Router, RouterModule } from "@angular/router";
import { TeamService } from "../../../core/services/team.service";
import { CommonModule, DatePipe } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatMenuModule } from "@angular/material/menu";
import { MatTabsModule } from "@angular/material/tabs";
import { UserService } from "../../../core/services/user.service";
import { AddMemberToTeamModalComponent } from "../add-member-to-team-modal/add-member-to-team-modal.component";
import { MatDialog } from "@angular/material/dialog";
import { BehaviorSubject } from "rxjs/internal/BehaviorSubject";
import { UserProfileResponseDto } from "../../../core/model/user";
import { firstValueFrom, Subscription } from "rxjs";
import { OrganizationService } from "../../../core/services/organization.service";
import { ViewTeamMembersComponent } from "../view-team-members/view-team-members.component";

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
    ViewTeamMembersComponent,
  ],
  providers: [DatePipe],
  templateUrl: "./view-team.component.html",
  styleUrl: "./view-team.component.css",
})
export class ViewTeamComponent implements OnInit, OnDestroy {
  private readonly activatedRoute: ActivatedRoute = inject(ActivatedRoute);
  private readonly router: Router = inject(Router);
  private readonly teamService: TeamService = inject(TeamService);
  private readonly organizationService: OrganizationService =
    inject(OrganizationService);
  private readonly userService: UserService = inject(UserService);
  private readonly dialog = inject(MatDialog);
  public organizationName: string = "";

  private searchSubscription: Subscription | null = null;
  public users$ = new BehaviorSubject<UserProfileResponseDto[]>([]);
  public team: Team = {
    name: "",
    description: "",
    createdAt: new Date(),
    owner: "",
    users: [],
    teamRole: TeamRole.ReadOnly,
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

  public addTeamMember(): void {
    this.searchSubscription = this.userService.searchQuery$.subscribe(
      (query) => {
        this.fetchOrganizationMembers(query).then((data) => {
          this.filterUsers(data);
        });
      }
    );
    const usersSnapshot = this.users$.getValue();
    const dialogRef = this.dialog.open(AddMemberToTeamModalComponent, {
      data: {
        users: this.users$,
      },
    });
    dialogRef.afterClosed().subscribe((selectedUsers) => {
      if (selectedUsers !== undefined) {
        this.addMembers(selectedUsers);
      }
      this.users$.next(usersSnapshot);
    });
  }

  public OnUserDeleted(deletedUser: UserProfileResponseDto): void {
    if (deletedUser == null) return;

    const deletedUserIndex =
      this.team.users?.findIndex(
        (u) => u.username == deletedUser.username
      ) ?? -1;
    if (deletedUserIndex >= 0) {
      this.team.users?.splice(deletedUserIndex, 1);
      this.filterUsers([...this.users$.getValue(), deletedUser]);
    }
  }

  private filterUsers(newUsers: UserProfileResponseDto[]): void {
    const filteredUsers =
      newUsers?.filter(
        (user) =>
          this.team.owner !== user.username &&
          !this.team.users?.some((u) => u.username === user.username)
      ) || [];

    this.users$.next(filteredUsers);
  }

  private addMembers(usernames: string[]): void {
    this.teamService
      .addMember(this.organizationName, this.team.name, {
        usernames: usernames,
      })
      .subscribe((response) => {
        if (response) {
          this.team.users = [...this.team.users!, ...response];
          this.filterUsers([...this.users$.getValue()]);
        }
      });
  }

  private async fetchOrganizationMembers(
    query: string
  ): Promise<UserProfileResponseDto[]> {
    return await firstValueFrom(
      this.organizationService.filterMembers(this.organizationName, query)
    );
  }

  public ngOnInit(): void {
    const name = this.activatedRoute.snapshot.paramMap.get("organizationName");
    const teamName = this.activatedRoute.snapshot.paramMap.get("teamName");
    if (name && teamName) {
      this.organizationName = name;
      this.teamService.getTeam(name, teamName).subscribe({
        next: (team: Team) => {
          this.team = team;
        },
      });
    } else {
      this.router.navigate(["/organization/view", name]);
    }
  }

  public ngOnDestroy() {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }
}
