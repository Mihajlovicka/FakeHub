import { Component, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { UserService } from '../../../core/services/user.service';
import { HelperService } from '../../../core/services/helper.service';
import { OrganizationService } from '../../../core/services/organization.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent {
  public helperService: HelperService = inject(HelperService);
  private userService: UserService = inject(UserService);
  private organizationService: OrganizationService = inject(OrganizationService);
  private router: Router = inject(Router);

  public isLoggedIn: boolean = false;
  public isDropdownVisible = false;
  public username: string = '';

  public searchQuery: string = ""

  ngOnInit(): void {
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

  @HostListener('document:click', ['$event'])
  public clickOutside(event: MouseEvent) {
    const dropdown = event.target as HTMLElement;

    if (dropdown && !dropdown.closest('.dropdown') && !dropdown.closest('.btn-hover')) {
      this.isDropdownVisible = false;
    }
  }

  public goToOrganizations(): void {
    this.organizationService.searchResultsSignal.set(null);
    this.searchQuery = '';
    this.router.navigate(['/organizations']);
  }

  public search(): void {
    const service: any = this.getService();
    if(this.searchQuery.trim() === '') {
      service.searchResultsSignal.set(null);
      return;
    }
    service.search(this.searchQuery);
  }

  private getService(): any {
    const currentRoute = this.router.url;

    if (currentRoute.includes('/organizations')) {
      return this.organizationService;
    } 
    return null;
  }
  
}