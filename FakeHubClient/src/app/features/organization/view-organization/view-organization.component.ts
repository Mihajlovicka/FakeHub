import {
  Router,
  ActivatedRoute,
  RouterModule
} from "@angular/router";
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
import { BehaviorSubject, firstValueFrom, lastValueFrom, Subscription, take } from "rxjs";
import { UserProfileResponseDto } from "../../../core/model/user";
import { AddMemberToOrganizationModalComponent } from "../add-member-to-organization-modal/add-member-to-organization-modal.component";

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
  ],
  templateUrl: "./view-organization.component.html",
  styleUrl: "./view-organization.component.css",
})
export class ViewOrganizationComponent implements OnInit, OnDestroy {
  private readonly service: OrganizationService = inject(OrganizationService);
  private readonly activatedRoute: ActivatedRoute = inject(ActivatedRoute);
  private readonly userService: UserService = inject(UserService);
  private readonly router: Router = inject(Router);
  private readonly dialog = inject(MatDialog);

  public activeLink: number = 1;
  private searchSubscription: Subscription | null = null;

  public users$ = new BehaviorSubject<UserProfileResponseDto[]>([]);
  public organization: Organization = {
    name: "",
    description: "",
    imageBase64: "",
    teams: [],
    users: [],
  };

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

  public addMemberToOrganizationDialog(): void {
    var usersSnapshot = this.users$.getValue();
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
    const filteredUsers = newUsers?.filter(
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

  private addMember(usernames: string[]): void {
    this.service
      .addMember(this.organization.name, { usernames: usernames })
      .subscribe((response) => {
        if (response) {
          this.organization.users.push(...response);
          this.filterUsers([...this.users$.getValue()]);
        }
      });
  }

  public ngOnInit(): void {
    const name = this.activatedRoute.snapshot.paramMap.get("name");
    if (name) {
      this.loadAsync(name);
    }
  }

  public ngOnDestroy() {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }
}
