import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-repositories-list-view',
  standalone: true,
  imports: [],
  templateUrl: './repositories-list-view.component.html',
  styleUrl: './repositories-list-view.component.css'
})
export class RepositoriesListViewComponent {
  private readonly router: Router = inject(Router);
  
  public goToCreateRepository(): void {
    this.router.navigate(["/repository/add"]);
  }
}
