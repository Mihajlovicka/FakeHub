import { Component, Inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import {
  MAT_SNACK_BAR_DATA,
  MatSnackBarRef,
} from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
@Component({
  selector: 'app-popup',
  standalone: true,
  imports: [MatIconModule, MatButtonModule],
  templateUrl: './popup.component.html',
  styleUrls: ['./popup.component.css'],
})
export class PopupComponent {
  constructor(
    @Inject(MAT_SNACK_BAR_DATA) public data: { message: string; type: string },
    public snackBarRef: MatSnackBarRef<PopupComponent>
  ) {}
}
