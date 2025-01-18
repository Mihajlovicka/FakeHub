import { Component, inject, OnDestroy, OnInit} from "@angular/core";
import { Router } from "@angular/router";
import { UserService } from "../../../core/services/user.service";
import { UserBadge, UserProfileResponseDto } from "../../../core/model/user";
import { CommonModule } from "@angular/common";
import { firstValueFrom, Subscription, take } from "rxjs";
import { UserBadgeComponent } from "../../../shared/components/user-badge/user-badge.component";
import { MatDialog } from "@angular/material/dialog";
import { UserBadgeModalComponent } from "../../../shared/components/user-badge/user-badge-modal/user-badge-modal.component";
import { ChangeUserBadgeRequest } from "../../../core/model/change-badge-to-user-request";

@Component({
  selector: "app-users-list-view",
  templateUrl: "./users-list-view.component.html",
  styleUrl: "./users-list-view.component.css",
  standalone: true,
  imports: [CommonModule, UserBadgeComponent]
})
export class UsersListViewComponent implements OnInit, OnDestroy {
  private readonly router: Router = inject(Router);
  private readonly usersService: UserService = inject(UserService);
  private readonly dialog = inject(MatDialog);

  public users: UserProfileResponseDto[] = [];
  public user: UserProfileResponseDto = new UserProfileResponseDto();
  public readonly title: string = this.fetchAdmins() ? "Admins" : "Users";
  public isSuperAdminLoggedIn: boolean = false;
  public isAdminLoggedIn: boolean = false;

  private searchSubscription: Subscription | null = null;

  public ngOnInit(): void {
    this.searchSubscription = this.usersService.searchQuery$.subscribe(
        query => {
          this.fetchData(query).then(
              data => {
                this.users = data;
              }
          )
        }
    );

    this.isSuperAdminLoggedIn = this.usersService.isSuperAdminLoggedIn();
    this.isAdminLoggedIn = this.usersService.isAdminLoggedIn();
  }

  public ngOnDestroy(): void {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }

  public goToAdminRegistration(): void {
    this.router.navigate(["/register/admin"]);
  }

  public onUserClick(user: UserProfileResponseDto): void {
    this.router.navigate([`/profile/${user.username}`]);
  }

  public openUserBadgeModal(user: UserProfileResponseDto): void {
      if(this.isAdminLoggedIn || this.isSuperAdminLoggedIn) {
        const dialogRef = this.dialog.open(UserBadgeModalComponent, {
          data: {currentBadge: user?.badge ?? UserBadge.None},
        });
        
        dialogRef.afterClosed().subscribe(selectedBadge => {
          if (selectedBadge !== undefined) {
             this.changeUserBadge(user, selectedBadge);
          }
        });
      }
    }

    public changeUserBadge(user: UserProfileResponseDto, selectedBadge: string): void {
        const changeUserBadge = new ChangeUserBadgeRequest(Number(selectedBadge), user.username);
        this.usersService.changeUserBadge(changeUserBadge).subscribe((result) => {
          if(result?.username) {
            this.users = this.users.map(user => {
              if(user.username == result.username) return result;
              return user;
            })
          }
        });
      }

  protected fetchAdmins(): boolean {
    const currentRoute = this.router.url;
    return currentRoute.includes('/admins')
  }

  private async fetchData(query: string): Promise<UserProfileResponseDto[]> {
      return await firstValueFrom(this.usersService.getUsers(query, this.fetchAdmins()).pipe(take(1)));
  }
}
