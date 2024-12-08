import {ChangeDetectorRef, Component, inject, OnDestroy, OnInit} from "@angular/core";
import { Router } from "@angular/router";
import { UserService } from "../../../core/services/user.service";
import { UserProfileResponseDto } from "../../../core/model/user";
import { CommonModule } from "@angular/common";
import { firstValueFrom, Subscription, take } from "rxjs";
import {HelperService} from "../../../core/services/helper.service";

@Component({
  selector: "app-users-list-view",
  templateUrl: "./users-list-view.component.html",
  styleUrl: "./users-list-view.component.css",
  standalone: true,
  imports: [CommonModule]
})
export class UsersListViewComponent implements OnInit, OnDestroy {
  private readonly router: Router = inject(Router);
  private readonly usersService: UserService = inject(UserService);

  public users: UserProfileResponseDto[] = [];
  public readonly title: string = this.fetchAdmins() ? "Admins" : "Users";
  public isSuperAdmin: boolean = false;

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

    this.isSuperAdmin = this.usersService.isSuperAdminLoggedIn();
  }

  public ngOnDestroy(): void {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }

  public goToAdminRegistration(): void {
    this.router.navigate(["/register/admin"]);
  }

  protected fetchAdmins(): boolean {
    const currentRoute = this.router.url;
    return currentRoute.includes('/admins')
  }

  private async fetchData(query: string): Promise<UserProfileResponseDto[]> {
      return await firstValueFrom(this.usersService.getUsers(query, this.fetchAdmins()).pipe(take(1)));
  }
}
