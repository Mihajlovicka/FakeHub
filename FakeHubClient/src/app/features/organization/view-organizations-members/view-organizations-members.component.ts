import {
  Component,
  Input,
  OnInit,
  OnChanges,
  SimpleChanges,
  inject,
  Output,
  EventEmitter,
} from "@angular/core";
import { CommonModule } from "@angular/common";
import { UserProfileResponseDto } from "../../../core/model/user";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { MatIconModule } from "@angular/material/icon";
import { MatDialog } from "@angular/material/dialog";
import { ConfirmationDialogComponent } from "../../../shared/components/confirmation-dialog/confirmation-dialog.component";
import { OrganizationService } from "../../../core/services/organization.service";
import { UserBadgeComponent } from "../../../shared/components/user-badge/user-badge.component";
import { Router } from "@angular/router";

@Component({
  selector: "app-view-organizations-members",
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, UserBadgeComponent],
  templateUrl: "./view-organizations-members.component.html",
  styleUrls: ["./view-organizations-members.component.css"],
})
export class ViewOrganizationsMembersComponent implements OnInit, OnChanges {
  public filteredUsers: UserProfileResponseDto[] = [];
  public searchQuery: string = "";
  private readonly dialog = inject(MatDialog);
  private readonly service = inject(OrganizationService);
  private readonly router: Router = inject(Router);

  @Input() public users: UserProfileResponseDto[] = [];
  @Input() public organizationName: string = "";
  @Input() public isOwner: boolean = false;
  @Output() deleteUserEvent = new EventEmitter<UserProfileResponseDto>();

  public ngOnInit(): void {
    this.initializeFilteredUsers();
  }

  public ngOnChanges(changes: SimpleChanges): void {
    if (changes["users"] && changes["users"].currentValue) {
      this.initializeFilteredUsers();
    }
  }

  private initializeFilteredUsers(): void {
    this.filteredUsers = [...this.users];
  }

  public search(): void {
    const query = this.searchQuery.trim().toLowerCase();

    if (!query) {
      this.filteredUsers = [...this.users];
      return;
    }

    const queriesUsername = [query];
    const queriesEmail = [query];

    let result: UserProfileResponseDto[] = [];

    for (const q of queriesUsername) {
      const userNameExact = this.users.filter(
        (u) => u.username.toLowerCase() === q
      );
      const userNameContains = this.users.filter((u) =>
        u.username.toLowerCase().includes(q)
      );
      result = Array.from(
        new Set([...result, ...userNameExact, ...userNameContains])
      );
    }

    for (const q of queriesEmail) {
      const emailExact = this.users.filter((u) => u.email.toLowerCase() === q);
      const emailContains = this.users.filter((u) =>
        u.email.toLowerCase().includes(q)
      );
      result = Array.from(
        new Set([...result, ...emailExact, ...emailContains])
      );
    }

    this.filteredUsers = result.sort((a, b) => {
      const isExactUsernameA = queriesUsername.includes(
        a.username.toLowerCase()
      );
      const isExactUsernameB = queriesUsername.includes(
        b.username.toLowerCase()
      );

      const isExactEmailA = queriesEmail.includes(a.email.toLowerCase());
      const isExactEmailB = queriesEmail.includes(b.email.toLowerCase());

      const isPartialUsernameA = queriesUsername.some((q) =>
        a.username.toLowerCase().includes(q)
      );
      const isPartialUsernameB = queriesUsername.some((q) =>
        b.username.toLowerCase().includes(q)
      );

      const isPartialEmailA = queriesEmail.some((q) =>
        a.email.toLowerCase().includes(q)
      );
      const isPartialEmailB = queriesEmail.some((q) =>
        b.email.toLowerCase().includes(q)
      );

      if (isExactUsernameA !== isExactUsernameB)
        return isExactUsernameA ? -1 : 1;
      if (isExactEmailA !== isExactEmailB) return isExactEmailA ? -1 : 1;
      if (isPartialUsernameA !== isPartialUsernameB)
        return isPartialUsernameA ? -1 : 1;
      if (isPartialEmailA !== isPartialEmailB) return isPartialEmailA ? -1 : 1;

      return 0;
    });
  }

  public openDeleteMemberModal(user: UserProfileResponseDto): void {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      data: {
        title: "Delete member",
        description:
          'Would you like to delete "' + user.username + '" from organization?',
      },
    });
    dialogRef.afterClosed().subscribe((isConfirmed) => {
      if (isConfirmed) {
        this.service
          .deleteMember(this.organizationName, user.username)
          .subscribe((data) => {
            if (data?.username != null) {
              const userIndex = this.filteredUsers.findIndex(
                (u) => u.username == data.username
              );
              this.filteredUsers.splice(userIndex, 1);
              this.deleteUserEvent.emit(data);
            }
          });
      }
    });
  }

  public onUserClick(user: UserProfileResponseDto): void {
    this.router.navigate([`/profile/${user.username}`]);
  }
}
