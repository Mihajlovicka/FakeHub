import { Component, Input  } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DockerImage } from '../../../core/model/docker-image';

@Component({
  selector: 'app-docker-image',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './docker-image.component.html',
  styleUrl: './docker-image.component.css'
})
export class DockerImageComponent {
  @Input() dockerImage?: DockerImage;  
  
  formatNumber(value: number): string {
    if (value >= 1000000) {
      return (value / 1000000).toFixed(1) + 'M';  
    } else if (value >= 1000) {
      return (value / 1000).toFixed(1) + 'K';    
    }
    return value.toString();  
  }
  
  onLinkClick(dockerImage: any) {
    console.log('Link clicked!', dockerImage);
  }

}
