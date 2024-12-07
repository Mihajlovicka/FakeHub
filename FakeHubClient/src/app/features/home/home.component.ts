import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DockerImageComponent } from "../../shared/components/docker-image/docker-image.component";
import { DockerImage } from '../../core/model/docker-image';
import { DockerImageService } from '../../core/services/docker-image.service';
import { UserService } from '../../core/services/user.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, DockerImageComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  private userService: UserService = inject(UserService);
  private dockerImageService: DockerImageService = inject(DockerImageService);

  public isTrustedContentVisible: boolean = true;
  public isCategoriesVisible: boolean = true;
  public isLoggedIn: boolean = false;
  public dockerImages: DockerImage[] = [];

  ngOnInit(): void {
    this.isLoggedIn = this.userService.isLoggedIn();
    this.loadDockerImages();
  }
  
  public toggleTrustedContentVisibility() {
    this.isTrustedContentVisible = !this.isTrustedContentVisible;
  }

  public toggleCategoriesVisibility() {
    this.isCategoriesVisible = !this.isCategoriesVisible;
  }

  private loadDockerImages(): void {
    this.dockerImageService.getDockerImages()
      .subscribe({
        next: (images) => {
          this.dockerImages = images;
        }
      });
  }
}
