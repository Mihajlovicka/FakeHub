import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import * as moment from 'moment';
import { UserProfileResponseDto } from '../../../core/model/user';
import { UserService } from '../../../core/services/user.service';
import { HeaderComponent } from '../../../shared/components/header/header.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, HeaderComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit{
  private userService: UserService = inject(UserService);
  private route: ActivatedRoute = inject(ActivatedRoute);

  public user: UserProfileResponseDto = new UserProfileResponseDto();
  public activeLink: number = 1;  
  private usernameParam: string = "";
  public isLoggedInUserProfile: boolean = false;

  ngOnInit(): void {
    this.usernameParam   = this.route.snapshot.paramMap.get('username') ?? "";

    this.userService.getUserProfileByUsername(this.usernameParam).subscribe(user => {
      this.user = user ?? new UserProfileResponseDto();
      this.isLoggedInUserProfile = this.userService.getUserNameFromToken() == user?.username;
    });
  }

  public setActiveLink(linkNumber: number) {
    this.activeLink = linkNumber;  
  }

  public formatDate(date: Date) {
    return this.isDateValid(date) ? moment.default(date).local().format('MMMM D, YYYY'): "";
  }

  public capitalizeFirstLetter(input: string): string {
    if (!input) {
      return '';
    }
    return input.charAt(0).toUpperCase();
  }

  public isDateValid(date:Date) {
    return new Date(date).getTime() > 0;
  }
}