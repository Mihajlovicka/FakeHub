import { Component } from '@angular/core';
import { HeaderComponent } from "../../shared/components/header/header.component";
import { CommonModule } from '@angular/common';
import { DockerImageComponent } from "../../shared/components/docker-image/docker-image.component";
import { DockerImage } from '../../core/model/docker-image.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [HeaderComponent, CommonModule, DockerImageComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  isTrustedContentVisible: boolean = true;
  isCategoriesVisible: boolean = true;
  buttonCount: number[] = new Array(10).fill(0);
  isLoggedIn: boolean = true;

  dockerImages: DockerImage[] = [
    new DockerImage(
      'icons8-docker-64.png',
      'Nginx',
      'https://example.com/nginx-badge.png',
      'Official Nginx image for web servers.',
      14000,
      7000000
    ),
    new DockerImage(
      'icons8-docker-64.png',
      'MySQL',
      'https://example.com/mysql-badge.png',
      'Official MySQL image for databases.',
      12000,
      5000000
    ),
    new DockerImage(
      'icons8-docker-64.png',
      'Redis',
      'https://example.com/redis-badge.png',
      'Official Redis image for caching.',
      18000,
      3000000
    ),
    new DockerImage(
      'icons8-docker-64.png',
      'PostgreSQL',
      'https://example.com/postgresql-badge.png',
      'Official PostgreSQL image for relational databases.',
      9000,
      6000000
    ),
    new DockerImage(
      'icons8-docker-64.png',
      'Alpine',
      'https://example.com/alpine-badge.png',
      'Official Alpine Linux image.',
      20000,
      8000000
    ),
    new DockerImage(
      'icons8-docker-64.png',
      'Ubuntu',
      'https://example.com/ubuntu-badge.png',
      'Official Ubuntu image for Linux distributions.',
      25000,
      10000000
    ),
    new DockerImage(
      'icons8-docker-64.png',
      'MongoDB',
      'https://example.com/mongodb-badge.png',
      'Official MongoDB image for NoSQL databases.',
      11000,
      4500000
    ),
    new DockerImage(
      'icons8-docker-64.png',
      'Node.js',
      'https://example.com/nodejs-badge.png',
      'Official Node.js image for JavaScript runtime.',
      30000,
      9000000
    ),
    new DockerImage(
      'icons8-docker-64.png',
      'Java',
      'https://example.com/java-badge.png',
      'Official Java image for development environment.',
      17000,
      6500000
    ),
    new DockerImage(
      'icons8-docker-64.png',
      'Python',
      'https://example.com/python-badge.png',
      'Official Python image for programming language.',
      22000,
      8500000
    ),
    new DockerImage(
      'icons8-docker-64.png',
      'WordPress',
      'https://example.com/wordpress-badge.png',
      'Official WordPress image for content management system.',
      16000,
      7500000
    )
  ];
  
  toggleTrustedContentVisibility() {
    this.isTrustedContentVisible = !this.isTrustedContentVisible;
  }

  toggleCategoriesVisibility() {
    this.isCategoriesVisible = !this.isCategoriesVisible;
  }
}
