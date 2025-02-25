import { Router, ActivatedRoute, RouterModule } from "@angular/router";
import { Component, inject, OnDestroy, OnInit } from "@angular/core";
import { Organization } from "../../../core/model/organization";
import { OrganizationService } from "../../../core/services/organization.service";
import { UserService } from "../../../core/services/user.service";
import { CommonModule } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { MatDialog } from "@angular/material/dialog";
import { MatIconModule } from "@angular/material/icon";
import { MatMenuModule } from "@angular/material/menu";
import { MatTabsModule } from "@angular/material/tabs";
import { TeamsComponent } from "../../team/teams/teams.component";
import {
  BehaviorSubject,
  firstValueFrom,
  lastValueFrom,
  Subscription,
  take,
} from "rxjs";
import { UserProfileResponseDto } from "../../../core/model/user";
import { AddMemberToOrganizationModalComponent } from "../add-member-to-organization-modal/add-member-to-organization-modal.component";
import { ViewOrganizationsMembersComponent } from "../view-organizations-members/view-organizations-members.component";
import { ConfirmationDialogComponent } from "../../../shared/components/confirmation-dialog/confirmation-dialog.component";
import { Team } from "../../../core/model/team";
import { Repository } from "../../../core/model/repository";
import { RepositoryService } from "../../../core/services/repository.service";
import { DockerImageComponent } from "../../../shared/components/docker-image/docker-image.component";

@Component({
  selector: "app-view-organization",
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatMenuModule,
    MatButtonModule,
    RouterModule,
    MatTabsModule,
    TeamsComponent,
    ViewOrganizationsMembersComponent,
    DockerImageComponent
  ],
  templateUrl: "./view-organization.component.html",
  styleUrl: "./view-organization.component.css",
})
export class ViewOrganizationComponent implements OnInit, OnDestroy {
  private readonly service: OrganizationService = inject(OrganizationService);
  private readonly activatedRoute: ActivatedRoute = inject(ActivatedRoute);
  private readonly repositoryService: RepositoryService = inject(RepositoryService);
  private readonly userService: UserService = inject(UserService);
  private readonly router: Router = inject(Router);
  private readonly dialog = inject(MatDialog);

  private searchSubscription: Subscription | null = null;

  public users$ = new BehaviorSubject<UserProfileResponseDto[]>([]);
  public organization: Organization = {
    name: "",
    description: "",
    imageBase64: "",
  };
  public repositories: Repository[] = [];

  public isOwner(): boolean {
    return (
      this.organization.owner !== null &&
      this.organization.owner === this.userService.getUserName()
    );
  }

  public edit(): void {
    this.router.navigate(["/organization/edit", this.organization.name]);
  }

  public addTeam(): void {
    this.router.navigate(["/organization/team/add", this.organization.name]);
  }

  public openDeactivateOrganizationModal(): void {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      data: {
        title: "Deactivate organization",
        description:
          'Would you like to deactivate "' +
          this.organization.name +
          '" organization?',
      },
    });
    dialogRef.afterClosed().subscribe((isConfirmed) => {
      if (isConfirmed) {
        this.service
          .deactivateOrganization(this.organization.name)
          .subscribe((_) => {
            this.router.navigate(["/organizations"]);
          });
      }
    });
  }

  public addMemberToOrganizationDialog(): void {
    const usersSnapshot = this.users$.getValue();
    const dialogRef = this.dialog.open(AddMemberToOrganizationModalComponent, {
      data: {
        users: this.users$,
      },
    });
    dialogRef.afterClosed().subscribe((selectedUsers) => {
      if (selectedUsers !== undefined) {
        this.addMember(selectedUsers);
      }
      this.users$.next(usersSnapshot);
    });
  }

  private filterUsers(newUsers: UserProfileResponseDto[]): void {
    const filteredUsers =
      newUsers?.filter(
        (user) =>
          this.organization.owner !== user.username &&
          !this.organization.users?.some((u) => u.username === user.username)
      ) || [];

    this.users$.next(filteredUsers);
  }

  private async fetchUsers(query: string): Promise<UserProfileResponseDto[]> {
    return await firstValueFrom(
      this.userService.getUsers(query, false).pipe(take(1))
    );
  }

  private async loadAsync(name: string) {
    this.organization = await lastValueFrom(this.service.getOrganization(name));

    this.searchSubscription = this.userService.searchQuery$.subscribe(
      (query) => {
        this.fetchUsers(query).then((data) => {
          this.filterUsers(data);
        });
      }
    );
  }

  public OnUserDeleted(deletedUser: UserProfileResponseDto): void {
    if (deletedUser == null) return;

    const deletedUserIndex =
      this.organization.users?.findIndex(
        (u) => u.username == deletedUser.username
      ) ?? -1;
    if (deletedUserIndex >= 0) {
      this.organization.users?.splice(deletedUserIndex, 1);
      this.filterUsers([...this.users$.getValue(), deletedUser]);
    }
  }

  public OnTeamDeleted(deletedTeam: Team): void {
    if (deletedTeam == null) return;

    const deletedTeamIndex =
      this.organization.teams?.findIndex((u) => u.name == deletedTeam.name) ??
      -1;
    if (deletedTeamIndex >= 0) {
      this.organization.teams?.splice(deletedTeamIndex, 1);
    }
  }

  private addMember(usernames: string[]): void {
    this.service
      .addMember(this.organization.name, { usernames: usernames })
      .subscribe((response) => {
        if (response) {
          this.organization.users = [...this.organization.users!, ...response];
          this.filterUsers([...this.users$.getValue()]);
        }
      });
  }

  public ngOnInit(): void {
    const name = this.activatedRoute.snapshot.paramMap.get("name");
    if (name) {
      this.loadAsync(name).then(() => {
        this.repositoryService.GetAllRepositoriesForOrganization(this.organization.name).subscribe({
          next: repos => {
            this.repositories = repos;
          }
        });
      });
    }
  }

  public ngOnDestroy() {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }
}
