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

  public formatNumber(value: number): string {
    if (value >= 1000000) {
        return this.formatLargeNumber(value, 1000000, 'M');
    } else if (value >= 1000) {
        return this.formatLargeNumber(value, 1000, 'K');
    }
    return value.toString();
  }

  private formatLargeNumber (value: number, divisor: number, suffix: string): string {
    let decNumber = (value / divisor).toFixed(1);
    decNumber = decNumber.endsWith('.0') ? decNumber.slice(0, decNumber.length - 2) : decNumber;
    return decNumber + suffix;
  };
}
