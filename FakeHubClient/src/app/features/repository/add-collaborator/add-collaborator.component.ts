import { Component, inject } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import {
  MatDialogActions,
  MatDialogClose,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle,
} from "@angular/material/dialog";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";

@Component({
  selector: "app-add-collaborator",
  standalone: true,
  imports: [
    FormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogActions,
    MatDialogClose,
    MatDialogTitle,
    MatDialogContent,
  ],
  templateUrl: "./add-collaborator.component.html",
  styleUrl: "./add-collaborator.component.css",
})
export class AddCollaboratorComponent {
  public username: string = "";

  private readonly dialogRef = inject(MatDialogRef<AddCollaboratorComponent>);

  public onAddClick(): void {
    this.dialogRef.close(this.username);
  }

  public onCancelClick(): void {
    this.dialogRef.close();
  }
}
