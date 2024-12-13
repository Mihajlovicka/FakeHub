import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogActions,
  MatDialogClose,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle,
} from '@angular/material/dialog';

@Component({
  selector: 'app-confirmation-dialog',
  standalone: true,
  imports: [MatButtonModule, MatDialogActions, MatDialogClose, MatDialogTitle, MatDialogContent],
  templateUrl: './confirmation-dialog.component.html',
  styleUrl: './confirmation-dialog.component.css'
})
export class ConfirmationDialogComponent {
   private readonly dialogRef = inject(
      MatDialogRef<ConfirmationDialogComponent>
    );
    public readonly title = inject(MAT_DIALOG_DATA).title;
    public readonly description = inject(MAT_DIALOG_DATA).description;

  public onConfirmClick() {
    this.dialogRef.close(true);
  }

  public onCloseClick(): void {
    this.dialogRef.close(false);
  }
}
