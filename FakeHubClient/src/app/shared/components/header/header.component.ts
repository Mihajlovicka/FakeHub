import { Component, HostListener, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { UserService } from '../../../core/services/user.service';
import { HelperService } from '../../../core/services/helper.service';
import { OrganizationService } from '../../../core/services/organization.service';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { RepositoryService } from '../../../core/services/repository.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent implements OnInit, OnDestroy {
  private helperService: HelperService = inject(HelperService);
  private userService: UserService = inject(UserService);
  private organizationService: OrganizationService = inject(OrganizationService);
  private repositoryService: RepositoryService = inject<any>(RepositoryService);
  private router: Router = inject(Router);

  public isLoggedIn: boolean = false;
  public isDropdownVisible = false;
  public username: string = '';
  public profileLabel: string = '';
  public searchQuery: string = '';
  public searchTitlePreview: string = '';
  public isUsersDropdownVisible = false;
  public isUser: boolean = false;
  public isSuperAdmin: boolean = false;
  public isAdmin: boolean = false;

  private routeSubscription: Subscription | null = null;

  @HostListener('document:click', ['$event'])
  public clickOutside(event: MouseEvent) {
    const dropdown = event.target as HTMLElement;
    if (dropdown && !dropdown.closest('.dropdown') && !dropdown.closest('.btn-hover')) {
      this.isDropdownVisible = false;
      this.isUsersDropdownVisible = false;
    }
  }

  public ngOnInit(): void {
    this.userService.isAuth$.subscribe(_ => {
      this.isLoggedIn = this.userService.isLoggedIn();
      this.username = this.userService.getUserName() ?? '';
      this.profileLabel = this.helperService.capitalizeFirstLetter(this.username);
      this.isUser = this.userService.isUserLoggedIn();
      this.isSuperAdmin = this.userService.isSuperAdminLoggedIn();
      this.isAdmin = this.userService.isAdminLoggedIn();
    });

    this.routeSubscription = this.router.events.subscribe((event: any) => {
      if (event.constructor.name === 'NavigationEnd') {
        this.searchTitlePreview = this.getSearchTitlePreview(event.urlAfterRedirects);
      }
    });
  }

  public ngOnDestroy(): void {
    if (this.routeSubscription) {
      this.routeSubscription.unsubscribe();
    }
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
    this.isDropdownVisible = false;
    this.userService.logout();
    this.router.navigate(['/'], { queryParams: { reload: new Date().getTime() } });
  }

  public goToOrganizations(): void {
    this.searchQuery = '';
    this.router.navigate(['/organizations']);
  }

  public goToRepositories(): void {
    this.searchQuery = "";
    this.router.navigate(['/repositories']);
  }

 public goToUsers(): void {
   this.searchQuery = '';
   this.router.navigate(['/users']);
 }

  public goToAdmins(): void {
    this.searchQuery = '';
    this.router.navigate(['/users/admins']);
  }

  public goToHomePage(): void {
    this.searchQuery = '';
    this.repositoryService.updateQuery('');
    this.router.navigate(['/']);
  }

  public search(): void {
    const service: any = this.getService();
    service.updateQuery(this.searchQuery);
  }

  public toggleUsersDropdown(event: MouseEvent): void {
    this.isUsersDropdownVisible = !this.isUsersDropdownVisible;
    event.stopPropagation(); // Prevent triggering the document click handler
  }

  private getService(): any {
    const currentRoute = this.router.url;
    if (currentRoute.includes('/organizations')) {
      return this.organizationService;
    }
    if(currentRoute.includes('/users')){
      return this.userService;
    }
    if(currentRoute.includes('/repositories') || currentRoute === '/'){
      return this.repositoryService;
    }
    return null;
  }

  private getSearchTitlePreview(urlAfterRedirects: string): string {
    if(urlAfterRedirects.includes('/organizations') || urlAfterRedirects.includes('/organization/')) return "organizations";
    if(urlAfterRedirects.includes('/users')) return "users";
    if(urlAfterRedirects.includes('/repositories') || urlAfterRedirects.includes('/repository/')) return "repositories";
    return "FakeHub";
  }
}