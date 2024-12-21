import {
  Component,
  Input,
  OnInit,
  OnChanges,
  SimpleChanges,
} from "@angular/core";
import { CommonModule } from "@angular/common";
import { UserProfileResponseDto } from "../../../core/model/user";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";

@Component({
  selector: "app-view-team-members",
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: "./view-team-members.component.html",
  styleUrls: ["./view-team-members.component.css"],
})
export class ViewTeamMembersComponent implements OnInit, OnChanges {
  public filteredUsers: UserProfileResponseDto[] = [];
  public searchQuery: string = "";

  @Input() public users: UserProfileResponseDto[] = [];

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
}
