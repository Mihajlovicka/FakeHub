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
  private userService: UserService = inject(UserService);

  public isLoggedIn: boolean = false;
  public isDropdownVisible = false;
  public username: string = '';

  constructor(private router: Router) { }

  public ngOnInit(): void {
    this.isLoggedIn = this.userService.isLoggedIn();
    this.username = this.userService.getUserName() ?? '';
  }

  public toggleDropdown(event: MouseEvent) {
    this.isDropdownVisible = !this.isDropdownVisible;

    event.stopPropagation();

    const button = event.target as HTMLElement;

    if (this.isDropdownVisible) {
      button.classList.add('dropdown-open');
    } else {
      button.classList.remove('dropdown-open');
    }
  }

  public goToProfile() {
    this.isDropdownVisible = false;
  }

  public goToRegistration(): void {
    this.router.navigate(['/register']);
  }

  public goToLogin(): void {
    this.router.navigate(['/login']);
  }

  public signOut(): void {
    this.userService.logout();
    window.location.reload();
  }

  public capitalizeFirstLetter(input: string): string {
    if (!input) {
      return '';
    }

    return input.charAt(0).toUpperCase();
  }

  @HostListener('document:click', ['$event'])
  public clickOutside(event: MouseEvent) {
    const dropdown = event.target as HTMLElement;

    if (dropdown && !dropdown.closest('.dropdown') && !dropdown.closest('.btn-hover')) {
      this.isDropdownVisible = false;
    }
  }
}
