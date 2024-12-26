import { Component, inject, OnInit } from '@angular/core';
import { UserService } from '../../core/services/user.service';
import { UserProfileResponseDto } from '../../core/model/user';
import {ActivatedRoute, Router} from '@angular/router';
import { HelperService } from '../../core/services/helper.service';
import {NgIf} from "@angular/common";

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    NgIf
  ],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css'
})
export class SettingsComponent implements OnInit {
  private helperService: HelperService = inject(HelperService);
  private userService: UserService = inject(UserService);
  private router: Router = inject(Router);
  private route: ActivatedRoute = inject(ActivatedRoute);

  public user: UserProfileResponseDto = new UserProfileResponseDto();
  public username: string = '';
  public profileLabel: string = '';
  public formatedDate: string = '';
  public isLoggedInUserProfile: boolean = false;
  public isAdmin: boolean = false;
  public isSuperAdmin: boolean = false;
  public isUser: boolean = false;
  private usernameParam = this.route.snapshot.paramMap.get('username') ?? "";

  public ngOnInit(): void {
    this.username = this.userService.getUserNameFromToken() ?? '';
    this.userService.getUserProfileByUsername(this.username).subscribe(user => {
      this.user = user ?? new UserProfileResponseDto();
      this.profileLabel = this.helperService.capitalizeFirstLetter(this.user.username);
      this.formatedDate = this.helperService.formatDate(this.user.createdAt);
      this.isAdmin = this.userService.isAdminLoggedIn();
      this.isSuperAdmin = this.userService.isSuperAdminLoggedIn();
      this.isUser = this.userService.isUserLoggedIn();
      this.isLoggedInUserProfile = this.userService.getUserNameFromToken() == this.usernameParam;
    });
  }

  public goToEmailEdit() {
    this.router.navigate(['/settings/email-edit']);
  }

  public goToPasswordEdit() {
    this.router.navigate(['/change-password']);
  }
}
