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
import { RepositoryService } from '../../../core/services/repository.service';
import { Repository } from '../../../core/model/repository';
import { DockerImageComponent } from '../../../shared/components/docker-image/docker-image.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, UserBadgeComponent, DockerImageComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit{
  private helperService: HelperService = inject(HelperService);
  private userService: UserService = inject(UserService);
  private readonly repositoryService: RepositoryService = inject(RepositoryService);
  private route: ActivatedRoute = inject(ActivatedRoute);
  private router: Router = inject(Router);
  private readonly dialog = inject(MatDialog);

  public user: UserProfileResponseDto = new UserProfileResponseDto();
  public activeLink: number = 1;  
  public isLoggedInUserProfile: boolean = false;
  public isAdminLoggedIn: boolean = false;
  public isSuperAdminLoggedIn: boolean = false;
  public isUserLoggedIn: boolean = false;
  public capitalizedLetterAvatar = "";
  public repositories: Repository[] = [];

  get editable(): boolean {
    return (this.isAdminLoggedIn || this.isSuperAdminLoggedIn) && this.user.role === 'USER';
  }

  ngOnInit(): void {
    const usernameParam = this.route.snapshot.paramMap.get('username') ?? '';
    this.checkUserPermissions();
    this.loadUserProfile(usernameParam);

    this.repositoryService.GetAllVisibleRepositoriesForUser(usernameParam).subscribe({
      next: repos => {
        this.repositories = repos;
      }
    });
  }

  private checkUserPermissions(): void {
    this.isAdminLoggedIn = this.userService.isAdminLoggedIn();
    this.isSuperAdminLoggedIn = this.userService.isSuperAdminLoggedIn();
    this.isUserLoggedIn = this.userService.isUserLoggedIn();
    this.isLoggedInUserProfile = this.userService.getUserNameFromToken() === this.route.snapshot.paramMap.get('username');
  }

  private loadUserProfile(username: string): void {
    this.userService.getUserProfileByUsername(username).subscribe(user => {
      this.user = user ?? new UserProfileResponseDto();
      this.capitalizedLetterAvatar = this.helperService.capitalizeFirstLetter(user?.username ?? "");
    });
  }

  public setActiveLink(linkNumber: number) {
    this.activeLink = linkNumber;  
  }

  public goToSettings() {
    this.router.navigate(['/settings']);
  }

  public openBadgeDialog(): void {
    if(this.isAdminLoggedIn || this.isSuperAdminLoggedIn) {
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
        this.user.badge = result.badge;
      }
    });
  }

  public isDateValid(dateString: string | Date): boolean {
    const date = new Date(dateString);
    const minValidDate = new Date(1, 0, 1);
    return date > minValidDate;
  }
}