import { Injectable } from '@angular/core';
import moment from 'moment';

@Injectable({
  providedIn: 'root'
})
export class HelperService {
  public formatDate(date: Date): string {
    return this.isDateValid(date) ? moment(date).local().format('MMMM D, YYYY') : '';
  }

  public isDateValid(date: Date): boolean {
    return new Date(date).getTime() > 0;
  }

  public capitalizeFirstLetter(input: string): string {
    if (!input) {
      return '';
    }
    return input.charAt(0).toUpperCase();
  }

   public readImageBase64(file: File): Promise<string> {
    return new Promise((resolve) => {
        const reader = new FileReader();
        reader.onload = (e: any) => {
          resolve(e.target.result);
        };
        reader.readAsDataURL(file);
    });
  }
}
