import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DockerImageComponent } from "../../shared/components/docker-image/docker-image.component";
import { Repository } from '../../core/model/repository';
import { RepositoryService } from '../../core/services/repository.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, DockerImageComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  private repositoryService: RepositoryService = inject(RepositoryService);
  private readonly router: Router = inject(Router);

  public isTrustedContentVisible: boolean = true;
  public publicRepositories: Repository[] = [];

  ngOnInit(): void {
    this.loadPublicRepositories();
  }
  
  public toggleTrustedContentVisibility() {
    this.isTrustedContentVisible = !this.isTrustedContentVisible;
  }

  public navigateToRepository(id: number | undefined){
    if(id) this.router.navigate(["/repository/", id]);
  }

  private loadPublicRepositories(): void {
    this.repositoryService.getAllPublicRepositories()
    .subscribe({
      next: (repos) => {
        this.publicRepositories = repos;
      },
      });
  }
}
