import { Component, inject, OnDestroy, OnInit } from "@angular/core";
import {
  MAT_DIALOG_DATA,
  MatDialogActions,
  MatDialogClose,
  MatDialogContent,
  MatDialogRef,
} from "@angular/material/dialog";
import { CommonModule } from "@angular/common";
import { MatListModule, MatSelectionListChange } from "@angular/material/list";
import { UserProfileResponseDto } from "../../../core/model/user";
import { UserBadgeComponent } from "../../../shared/components/user-badge/user-badge.component";
import { FormsModule } from "@angular/forms";
import { UserService } from "../../../core/services/user.service";
import { BehaviorSubject, Subscription } from "rxjs";

@Component({
  selector: "app-add-member-to-team-modal",
  standalone: true,
  imports: [
    MatDialogContent,
    MatDialogActions,
    MatDialogClose,
    CommonModule,
    MatListModule,
    UserBadgeComponent,
    FormsModule,
  ],
  templateUrl: "./add-member-to-team-modal.component.html",
  styleUrl: "./add-member-to-team-modal.component.css",
})
export class AddMemberToTeamModalComponent implements OnInit, OnDestroy {
  private readonly dialogRef: MatDialogRef<AddMemberToTeamModalComponent> =
    inject(MatDialogRef<AddMemberToTeamModalComponent>);
  private userService: UserService = inject(UserService);
  public readonly organizations = inject(MAT_DIALOG_DATA).organizations;
  public readonly dialogUsers$: BehaviorSubject<UserProfileResponseDto[]> =
    inject(MAT_DIALOG_DATA).users;
  public usersData: UserProfileResponseDto[] = [];
  public selectedUsers: UserProfileResponseDto[] = [];
  public searchQuery: string = "";
  private usersSubscription: Subscription | null = null;

  public onSelectionChange(event: MatSelectionListChange): void {
    var changedUser = event.options[0].value;
    var userIndex = this.selectedUsers.findIndex(
      (u) => u.username == changedUser.username
    );
    userIndex >= 0
      ? this.selectedUsers.splice(userIndex, 1)
      : this.selectedUsers.push(changedUser);
  }

  public isUserSelected(user: UserProfileResponseDto): boolean {
    return this.selectedUsers.some((u) => u.username == user.username);
  }

  public onSaveClick() {
    var listOfUsernames: string[] = [];
    this.selectedUsers.map((user) => listOfUsernames.push(user.username));
    this.dialogRef.close(listOfUsernames);
  }

  public onCloseClick(): void {
    this.dialogRef.close();
  }

  public search(): void {
    this.userService.updateQuery(this.searchQuery);
  }

  public ngOnInit(): void {
    this.usersSubscription = this.dialogUsers$.subscribe((newData) => {
      this.usersData = newData;
    });
  }

  public ngOnDestroy(): void {
    this.selectedUsers = [];
    if (this.usersSubscription) {
      this.usersSubscription.unsubscribe();
    }
  }
}
