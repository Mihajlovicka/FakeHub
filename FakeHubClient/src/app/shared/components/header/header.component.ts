import { Component, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent {
  constructor(private router: Router) { }

  isDropdownVisible = false;
  isLoggedIn = false;

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

  signOut() {
    console.log('Signing out...');
    this.isDropdownVisible = false;
  }

  goToRegistration(): void {
    this.router.navigate(['/register']);
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
