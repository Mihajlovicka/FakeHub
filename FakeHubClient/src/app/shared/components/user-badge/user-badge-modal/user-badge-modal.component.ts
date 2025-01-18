import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef, MatDialogTitle } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { UserBadge } from '../../../../core/model/user';

export interface DialogData {
  currentBadge: string
}

@Component({
  selector: 'app-user-badge-modal',
  standalone: true,
  imports: [
    CommonModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    MatButtonModule,
    MatDialogTitle,
    MatDialogContent,
    MatDialogActions,
    MatDialogClose,
    MatRadioModule
  ],
  templateUrl: './user-badge-modal.component.html',
  styleUrl: './user-badge-modal.component.css'
})
export class UserBadgeModalComponent {
  readonly dialogRef = inject(MatDialogRef<UserBadgeModalComponent>);
  readonly data = inject(MAT_DIALOG_DATA);
  readonly selectedBadge = signal<string>(this.data.currentBadge ?? UserBadge.None);

  public UserBadge = UserBadge;

  public onNoClick(): void {
    this.dialogRef.close();
  }

  public onSaveClick() {
    this.dialogRef.close(this.selectedBadge());
  }
}
