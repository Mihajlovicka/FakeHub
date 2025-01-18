import { CommonModule } from "@angular/common";
import { Component, OnDestroy, OnInit, ViewChild, inject } from "@angular/core";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatMenuModule } from "@angular/material/menu";
import { MatTabsModule, MatTabGroup } from "@angular/material/tabs";
import { ActivatedRoute, Router, RouterModule } from "@angular/router";
import { Repository, RepositoryOwnedBy } from "../../../core/model/repository";
import { RepositoryService } from "../../../core/services/repository.service";
import { filter, Subscription, switchMap, take } from "rxjs";
import { HelperService } from "../../../core/services/helper.service";
import { TagsComponent } from "../../tag/tags/tags.component";
import { RepositoryBadgeComponent } from "../../../shared/components/repository-badge/repository-badge.component";
import { ConfirmationDialogComponent } from "../../../shared/components/confirmation-dialog/confirmation-dialog.component";
import { MatDialog } from "@angular/material/dialog";
import { MatInputModule } from "@angular/material/input";
import { UserService } from "../../../core/services/user.service";
import { AddCollaboratorComponent } from "../add-collaborator/add-collaborator.component";
import { UserProfileResponseDto } from "../../../core/model/user";
import { UserBadgeComponent } from "../../../shared/components/user-badge/user-badge.component";
import { Team } from "../../../core/model/team";
import { MatTooltipModule } from "@angular/material/tooltip";
import { OrganizationService } from "../../../core/services/organization.service";
import { TeamService } from "../../../core/services/team.service";

@Component({
  selector: "app-view-repository",
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatMenuModule,
    MatButtonModule,
    RouterModule,
    MatTabsModule,
    TagsComponent,
    RepositoryBadgeComponent,
    MatInputModule,
    UserBadgeComponent,
    MatTooltipModule,
  ],
  templateUrl: "./view-repository.component.html",
  styleUrl: "./view-repository.component.css",
})
export class ViewRepositoryComponent implements OnInit, OnDestroy {
  @ViewChild('tabGroup') tabGroup!: MatTabGroup;
  
  public repository!: Repository;
  public capitalizedLetterAvatar: string = "";
  public canUserDelete: boolean = false;
  public isCurrentUserCollaborator: boolean = false;
  public collaborators?: (UserProfileResponseDto | Team)[] = [];
  public currentUserUsername: string = "";

  private readonly repositoryService: RepositoryService =
    inject(RepositoryService);
  private readonly organizationService: OrganizationService =
    inject(OrganizationService);
  private readonly userService: UserService = inject(UserService);
  private readonly teamService: TeamService = inject(TeamService);
  private readonly activatedRoute: ActivatedRoute = inject(ActivatedRoute);
  private readonly helperService: HelperService = inject(HelperService);
  private getRepositorySubscription: Subscription | null = null;
  private canEditRepositorySubscription: Subscription | null = null;
  private routeSubscription: Subscription | null = null;
  private dialogSubscription: Subscription | null = null;
  private readonly router: Router = inject(Router);
  private readonly dialog = inject(MatDialog);

  public ngOnInit() {
    this.currentUserUsername = this.userService.getUserNameFromToken() ?? "";

    const repoId = this.getRepoId();
    if (repoId) {
      this.canEditRepositorySubscription = this.getRepositorySubscription =
        this.repositoryService.getRepository(repoId).subscribe((data) => {
          if (data) {
            this.repository = data;
            this.avatarProfile();
            if (this.userService.isLoggedIn()) {
              this.isOwner();
              this.loadCollaborators();
            }
          }
        });
    }
  }

  public ngOnDestroy(): void {
    if (this.getRepositorySubscription) {
      this.getRepositorySubscription.unsubscribe();
    }
    if (this.routeSubscription) {
      this.routeSubscription.unsubscribe();
    }
    if (this.canEditRepositorySubscription) {
      this.canEditRepositorySubscription.unsubscribe();
    }
    if (this.dialogSubscription) {
      this.dialogSubscription.unsubscribe();
    }
  }

  public isOwner(): void {
    if (this.repository.id) {
      this.canEditRepositorySubscription = this.repositoryService
        .canEditRepository(this.repository.id)
        .pipe(take(1))
        .subscribe((data: boolean) => {
          this.canUserDelete = data;
        });
    }
  }

  public avatarProfile(): void {
    this.capitalizedLetterAvatar = this.helperService.capitalizeFirstLetter(
      this.repository?.name ?? ""
    );
  }

  public isOrganizationRepository(): boolean {
    return this.repository?.ownedBy === RepositoryOwnedBy.ORGANIZATION;
  }

  public openDeleteRepositoryModal(): void {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      data: {
        title: "Delete Repository",
        description: `Are you sure you want to delete "${this.repository.name}"?`,
      },
    });

    this.dialogSubscription = dialogRef
      .afterClosed()
      .pipe(
        filter((isConfirmed) => isConfirmed && this.repository?.id != null),
        switchMap(() => this.repositoryService.delete(this.repository.id!)),
        take(1)
      )
      .subscribe({
        next: () => {
          this.router.navigate(["/repositories"]);
        },
        error: (err) => {
          throw err;
        },
      });
  }

  public openAddCollaboratorModal(): void {
    const dialogRef = this.dialog.open(AddCollaboratorComponent, {
      width: "30rem",
    });

    dialogRef.afterClosed().subscribe((username) => {
      const repoId = this.repository?.id;
      if (username && repoId !== undefined) {
        this.repositoryService.addCollaborator(repoId, username).subscribe({
          next: () => {
            this.loadCollaborators();
            if (this.tabGroup) {
              this.tabGroup.selectedIndex = 1;
            }
          },
        });
      }
    });
  }

  public loadCollaborators(): void {
    const repoId = this.repository?.id;
    if (!repoId) return;

    this.repositoryService.getCollaborators(repoId).subscribe({
      next: (data) => {
        if (data != null) {
          this.isCurrentUserCollaborator = true;
          this.collaborators = data;
        }
      },
    });
  }

  public onUserClick(user: UserProfileResponseDto): void {
    this.router.navigate([`/profile/${user.username}`]);
  }

  public onTeamClick(team: Team): void {
    const organizationName = this.repository.ownerUsername;
    const teamName = team.name;
    if (organizationName) {
      this.router.navigate([
        "/organization/team/view",
        organizationName,
        teamName,
      ]);
    }
  }

  public isUser(item: UserProfileResponseDto | Team): item is UserProfileResponseDto {
    return (item as UserProfileResponseDto).username !== undefined;
  }

  public isTeam(item: UserProfileResponseDto | Team): item is Team {
    return ((item as Team).name !== undefined && (item as Team).users !== undefined);
  }

  public trackById(index: number, item: any) {
    return item.id;
  }

  public openDeleteCollaboratorModal(
    item: UserProfileResponseDto | Team
  ): void {
    let name = this.isUser(item) ? item.username : item.name;

    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      data: {
        title: "Delete collaborator",
        description: `Would you like to delete "${name}" from repository?`,
      },
    });

    dialogRef.afterClosed().subscribe((isConfirmed) => {
      if (isConfirmed) {
        const repoId = this.repository?.id;
        if (!repoId) return;

        if (this.isUser(item)) {
          this.repositoryService
            .removeUserCollaborator(repoId, item.username)
            .subscribe(() => this.loadCollaborators());
        } else if (this.isTeam(item)) {
          this.organizationService
            .deleteTeam(this.repository.ownerUsername!, item.name)
            .subscribe(() => this.loadCollaborators());
        }
      }
    });
  }

  public leaveRepository(): void {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      data: {
        title: "Leave repository",
        description: `Would you like to leave "${this.repository.name}" repository?`,
      },
    });

    this.dialogSubscription = dialogRef
      .afterClosed()
      .subscribe((isConfirmed) => {
        if (isConfirmed) {
          const repoId = this.repository?.id;
          if (!repoId) return;

          this.repositoryService
            .removeUserCollaborator(repoId, this.currentUserUsername)
            .subscribe({
              next: () => {
                if (this.repository.isPrivate) {
                  this.router.navigate(["/repositories"]);
                } else {
                  this.loadCollaborators();
                  this.isCurrentUserCollaborator = false;
                }
              },
            });
        }
      });
  }

  public isUserInTeam(team: Team): boolean {
    return (
      team.users?.some(u => u.username === this.currentUserUsername) ?? false
    );
  }

  public leaveTeam(team: Team): void {
    if (this.isTeam(team)) {
      const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
        data: {
          title: "Leave team",
          description: `Would you like to leave "${team.name}" team?`,
        },
      });
      this.dialogSubscription = dialogRef
        .afterClosed()
        .subscribe((isConfirmed) => {
          if (isConfirmed) {
            this.teamService
              .deleteMember(
                this.repository.ownerUsername!,
                team.name!,
                this.currentUserUsername
              )
              .subscribe({
                next: () => {
                  this.loadCollaborators();
                  this.isCurrentUserCollaborator = false;
                },
              });
          }
        });
    }
  }

  private getRepoId(): number | undefined {
    let id = undefined;

    this.routeSubscription = this.activatedRoute.paramMap
      .pipe(take(1))
      .subscribe((route) => {
        id = route.get("repositoryId");
      });

    return id;
  }

  public navigateToRepoEdit() {
    this.router.navigate(["/repository/edit/", this.repository.id]);
  }
}