import { CommonModule } from "@angular/common";
import { Component, OnDestroy, OnInit, inject } from "@angular/core";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatMenuModule } from "@angular/material/menu";
import { MatTabsModule } from "@angular/material/tabs";
import { ActivatedRoute, Router, RouterModule } from "@angular/router";
import { Repository } from "../../../core/model/repository";
import { RepositoryService } from "../../../core/services/repository.service";
import { filter, Subscription, switchMap, take } from "rxjs";
import { HelperService } from "../../../core/services/helper.service";
import { TagsComponent } from "../../tag/tags/tags.component";
import { RepositoryBadgeComponent } from "../../../shared/components/repository-badge/repository-badge.component";
import { ConfirmationDialogComponent } from "../../../shared/components/confirmation-dialog/confirmation-dialog.component";
import { MatDialog } from "@angular/material/dialog";
import { MatInputModule } from "@angular/material/input";

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
    MatInputModule
],
  templateUrl: "./view-repository.component.html",
  styleUrl: "./view-repository.component.css",
})
export class ViewRepositoryComponent implements OnInit, OnDestroy {
  public repository!: Repository;
  public capitalizedLetterAvatar: string = "";
  public canUserDelete: boolean = false;

  private readonly repositoryService: RepositoryService =
    inject(RepositoryService);
  private readonly activatedRoute: ActivatedRoute = inject(ActivatedRoute);
  private readonly helperService: HelperService = inject(HelperService);
  private getRepositorySubscription: Subscription | null = null;
  private canEditRepositorySubscription: Subscription | null = null;
  private routeSubscription: Subscription | null = null;
  private dialogSubscription: Subscription | null = null;
  private readonly router: Router = inject(Router);
  private readonly dialog = inject(MatDialog);

  public ngOnInit() {
    const repoId = this.getRepoId();
    if (repoId) {
      this.canEditRepositorySubscription = this.getRepositorySubscription = this.repositoryService
        .getRepository(repoId)
        .subscribe((data) => {
          if (data) {
            this.repository = data;
            this.avatarProfile();
            this.isOwner();
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
      this.canEditRepositorySubscription = this.repositoryService.canEditRepository(this.repository.id)
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

  public openDeleteRepositoryModal(): void {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      data: {
        title: "Delete Repository",
        description: `Are you sure you want to delete "${this.repository.name}"?`,
      },
    });

    this.dialogSubscription = dialogRef.afterClosed()
      .pipe(
        filter(isConfirmed => isConfirmed && this.repository?.id != null),
        switchMap(() => this.repositoryService.delete(this.repository.id!)),
        take(1)
      )
      .subscribe({
        next: () => {
          this.router.navigate(["/repositories"]);
        },
        error: (err) => {
          throw err;
        }
      });
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