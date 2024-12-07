import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UserBadge, UserProfileResponseDto } from '../../../core/model/user';
import { UserService } from '../../../core/services/user.service';
import { HelperService } from '../../../core/services/helper.service';
import { UserBadgeComponent } from '../../../shared/components/user-badge/user-badge.component';
import { UserBadgeModalComponent } from '../../../shared/components/user-badge/user-badge-modal/user-badge-modal.component';
import { MatDialog } from '@angular/material/dialog';
import { ChangeUserBadgeRequest } from '../../../core/model/change-badge-to-user-request';

export interface DialogData {
  animal: string;
  name: string;
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, UserBadgeComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit{

  public helperService: HelperService = inject(HelperService);
  private userService: UserService = inject(UserService);
  private route: ActivatedRoute = inject(ActivatedRoute);
  private router: Router = inject(Router);
  readonly dialog = inject(MatDialog);

  public user: UserProfileResponseDto = new UserProfileResponseDto();
  public activeLink: number = 1;  
  public isLoggedInUserProfile: boolean = false;
  public isAdmin: boolean = false;

  ngOnInit(): void {
    var usernameParam = this.route.snapshot.paramMap.get('username') ?? "";
    this.isAdmin = this.userService.getRole() == "ADMIN";
    this.isLoggedInUserProfile = this.userService.getUserNameFromToken() == usernameParam;

    this.userService.getUserProfileByUsername(usernameParam).subscribe(user => {
      this.user = user ?? new UserProfileResponseDto();
    });
  }

  public setActiveLink(linkNumber: number) {
    this.activeLink = linkNumber;  
  }

  public goToSettings() {
    this.router.navigate(['/settings']);
  }

  public openDialog(): void {
    if(this.isAdmin) {
      const dialogRef = this.dialog.open(UserBadgeModalComponent, {
        data: {currentBadge: this.user?.badge ?? UserBadge.None},
      });
  
      dialogRef.afterClosed().subscribe(selectedBadge => {
        if (selectedBadge !== undefined) {
           this.changeUserBadge(selectedBadge);
        }
      });
    }
  }

  public changeUserBadge(selectedBadge: string): void {
    const changeUserBadge = new ChangeUserBadgeRequest(Number(selectedBadge), this.user.username);
    this.userService.changeUserBadge(changeUserBadge).subscribe((result) => {
      if(result?.username) {
        this.user = result;
      }
    });
  }
}