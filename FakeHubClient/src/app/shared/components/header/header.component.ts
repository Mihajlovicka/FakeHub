import { Component, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { UserService } from '../../../core/services/user.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent {
  constructor(private router: Router) { }
  userService: UserService = inject(UserService);

  isLoggedIn: boolean = false;
  isDropdownVisible = false;
  username: string = '';

  ngOnInit(): void {
    this.isLoggedIn = this.userService.isLoggedIn();
    this.username = this.userService.getUserName() ?? '';
  }

  toggleDropdown(event: MouseEvent) {
    this.isDropdownVisible = !this.isDropdownVisible;

    event.stopPropagation();

    const button = event.target as HTMLElement;

    if (this.isDropdownVisible) {
      button.classList.add('dropdown-open');
    } else {
      button.classList.remove('dropdown-open');
    }
  }

  goToProfile() {
    console.log('Navigating to My Profile...');
    this.isDropdownVisible = false;
  }

  goToRegistration(): void {
    this.router.navigate(['/register']);
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }

  signOut(): void {
    this.userService.logout();
    window.location.reload();
  }

  capitalizeFirstLetter(input: string): string {
    if (!input) {
      return '';
    }

    return input.charAt(0).toUpperCase();
  }

  @HostListener('document:click', ['$event'])
  clickOutside(event: MouseEvent) {
    const dropdown = event.target as HTMLElement;

    if (dropdown && !dropdown.closest('.dropdown') && !dropdown.closest('.btn-hover')) {
      this.isDropdownVisible = false;
    }
  }
}
