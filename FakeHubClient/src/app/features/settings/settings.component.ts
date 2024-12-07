import { Component, inject } from '@angular/core';
import { UserService } from '../../core/services/user.service';
import { UserProfileResponseDto } from '../../core/model/user';
import { Router } from '@angular/router';
import { HelperService } from '../../core/services/helper.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css'
})
export class SettingsComponent {
  public helperService: HelperService = inject(HelperService);
  private userService: UserService = inject(UserService);
  private router: Router = inject(Router);
  
  public user: UserProfileResponseDto = new UserProfileResponseDto();
  public username: string = '';

  ngOnInit(): void {
    this.username = this.userService.getUserNameFromToken() ?? '';

    this.userService.getUserProfileByUsername(this.username).subscribe(user => {
      this.user = user ?? new UserProfileResponseDto();
    });
  }

  public goToEmailEdit() {
    this.router.navigate(['/settings/email-edit']);
  }

  public goToPasswordEdit() {
    this.router.navigate(['/change-password']);
  }
}
