import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { RepositoryService } from '../../../core/services/repository.service';
import { Repository } from '../../../core/model/repository';
import { CommonModule } from '@angular/common';
import { DockerImageComponent } from '../../../shared/components/docker-image/docker-image.component';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-repositories-list-view',
  standalone: true,
  imports: [CommonModule, DockerImageComponent],
  templateUrl: './repositories-list-view.component.html',
  styleUrl: './repositories-list-view.component.css'
})
export class RepositoriesListViewComponent implements OnInit, OnDestroy {
  private readonly router: Router = inject(Router);
  private readonly repositoryService: RepositoryService = inject(RepositoryService);

  public username: string = '';
  public repositories: Repository[] = [];

  private searchSubscription: Subscription | null = null;

  public goToCreateRepository(): void {
    this.router.navigate(["/repository/add"]);
  }

  public navigateToRepository(id: number | undefined){
    if(id) this.router.navigate(["/repository/", id]);
  }

  public ngOnInit(): void {
    this.searchSubscription = this.repositoryService.searchQuery$.subscribe(query => {
      this.repositoryService.getRepositories(query).subscribe(repos => {
        this.repositories = repos;
      });
    });
  }

  public ngOnDestroy(): void {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }
}