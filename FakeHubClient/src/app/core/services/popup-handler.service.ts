import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PopupComponent } from '../../shared/components/popup/popup.component';

@Injectable({
  providedIn: 'root',
})
export class PopupHandlerService {
  constructor(private snackBar: MatSnackBar) {}

  public openSnackbar(message: string, type: 'success' | 'error' = 'error'): void {
    this.snackBar.openFromComponent(PopupComponent, {
      data: { message, type },
      panelClass: type,
      duration: 5000,
    });
  }
}
