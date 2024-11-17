import { Component, inject, OnInit } from '@angular/core';
import { HeaderComponent } from "../../shared/components/header/header.component";
import { CommonModule } from '@angular/common';
import { DockerImageComponent } from "../../shared/components/docker-image/docker-image.component";
import { DockerImage } from '../../core/model/docker-image';
import { DockerImageService } from '../../core/services/docker-image.service';
import { UserService } from '../../core/services/user.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [HeaderComponent, CommonModule, DockerImageComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  userService: UserService = inject(UserService);

  isTrustedContentVisible: boolean = true;
  isCategoriesVisible: boolean = true;
  buttonCount: number[] = new Array(10).fill(0);
  isLoggedIn: boolean = false;
  dockerImages: DockerImage[] = [];
  error: string | null = null;

  constructor(private dockerImageService: DockerImageService) {}

  ngOnInit(): void {
    this.isLoggedIn = this.userService.isLoggedIn();
    this.loadDockerImages();
  }

  loadDockerImages(): void {
    this.dockerImageService.getDockerImages()
      .subscribe({
        next: (images) => {
          this.dockerImages = images;
        },
        error: (err) => {
          this.error = 'Failed to load Docker images: ' + err.message;
        }
      });
  }
  
  toggleTrustedContentVisibility() {
    this.isTrustedContentVisible = !this.isTrustedContentVisible;
  }

  toggleCategoriesVisibility() {
    this.isCategoriesVisible = !this.isCategoriesVisible;
  }
}
