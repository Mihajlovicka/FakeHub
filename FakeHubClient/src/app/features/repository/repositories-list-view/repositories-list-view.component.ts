import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { RepositoryService } from '../../../core/services/repository.service';
import { Repository } from '../../../core/model/repository';
import { CommonModule } from '@angular/common';
import { DockerImageComponent } from '../../../shared/components/docker-image/docker-image.component';

@Component({
  selector: 'app-repositories-list-view',
  standalone: true,
  imports: [CommonModule, DockerImageComponent],
  templateUrl: './repositories-list-view.component.html',
  styleUrl: './repositories-list-view.component.css'
})
export class RepositoriesListViewComponent {
  private readonly router: Router = inject(Router);
  private readonly repositoryService: RepositoryService = inject(RepositoryService);

  public username: string = '';
  public repositories: Repository[] = [];
  
  public goToCreateRepository(): void {
    this.router.navigate(["/repository/add"]);
  }

  ngOnInit(): void {
    this.repositoryService.GetAllRepositoriesForCurrentUser().subscribe({
      next: repos => {
        this.repositories = repos;
      }
    });
  }
}
