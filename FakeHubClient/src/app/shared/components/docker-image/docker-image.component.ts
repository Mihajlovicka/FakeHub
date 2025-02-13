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
  @Input() title?: string;
  @Input() description?: string;
  @Input() updatedAt?: Date | string;
  @Input() badge?: string;
  @Input() isPrivate?: boolean;
  @Input() footerText?: string = "";

  public formatNumber(value: number): string {
    if (value >= 1000000) {
        return this.formatLargeNumber(value, 1000000, 'M');
    } else if (value >= 1000) {
        return this.formatLargeNumber(value, 1000, 'K');
    }
    return value.toString();
  }

  public isDateValid(dateString: string | Date | undefined): boolean {
    if(!dateString || dateString == undefined) return false;

    const date = new Date(dateString);
    const minValidDate = new Date(1, 0, 1);
    return date > minValidDate;
  }

  private formatLargeNumber (value: number, divisor: number, suffix: string): string {
    let decNumber = (value / divisor).toFixed(1);
    decNumber = decNumber.endsWith('.0') ? decNumber.slice(0, decNumber.length - 2) : decNumber;
    return decNumber + suffix;
  };
}
