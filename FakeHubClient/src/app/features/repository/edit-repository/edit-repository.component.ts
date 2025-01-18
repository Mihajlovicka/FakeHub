import { CommonModule } from '@angular/common';
import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatOptionModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { RepositoryService } from '../../../core/services/repository.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription, take } from 'rxjs';
import { EditRepositoryDto, Repository } from '../../../core/model/repository';

@Component({
  selector: 'app-edit-repository',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    FormsModule,
    MatRadioModule,
    MatFormFieldModule,
    MatSelectModule,
    MatOptionModule,
    CommonModule,
  ],
  templateUrl: './edit-repository.component.html',
  styleUrl: './edit-repository.component.css'
})
export class EditRepositoryComponent implements OnInit, OnDestroy {
  public repository!: Repository;
  public repositoryForm: FormGroup = new FormGroup({
    name: new FormControl({ value: "", disabled: true }),
    description: new FormControl("", [Validators.maxLength(500)]),
    isPrivate: new FormControl(false, [Validators.required]),
    ownerId: new FormControl(null, [Validators.required]),
  });

  private readonly repositoryService: RepositoryService = inject(RepositoryService);
  private readonly activatedRoute: ActivatedRoute = inject(ActivatedRoute);
  private readonly router: Router = inject(Router);

  private getRepositorySubscription: Subscription | null = null;
  private editRepositorySubscription: Subscription | null = null;

  private initialFormValue!: Repository;

  public ngOnInit() {
    const repoId = Number(this.activatedRoute.snapshot.paramMap.get("repositoryId"));
    if (repoId) {
      this.getRepositorySubscription = this.repositoryService.getRepository(repoId)
        .pipe(take(1))
        .subscribe(
          data => {
            if (data) {
              this.repository = data;
              this.initFormFromModel();
            }
          }
        );
    }
  }

  public ngOnDestroy(): void {
    if (this.getRepositorySubscription) {
      this.getRepositorySubscription.unsubscribe();
    }
    if (this.editRepositorySubscription) {
      this.editRepositorySubscription.unsubscribe();
    }
  }

  public onSubmit(): void {
    if (this.repositoryForm.invalid) return;

    const updatedRepo: EditRepositoryDto = {
      id: this.repository.id!,
      description: this.repositoryForm.value.description,
      isPrivate: this.repositoryForm.value.isPrivate
    };

    this.editRepositorySubscription = this.repositoryService.editRepository(updatedRepo)
      .pipe(take(1))
      .subscribe({
        next: () => {
          this.router.navigate(["/repositories"]);
        },
        error: (err) => {
          throw err;
        }
      });
  }

  public isFormUnchanged(): boolean {
    return JSON.stringify(this.repositoryForm.getRawValue()) === JSON.stringify(this.initialFormValue);
  }

  private initFormFromModel(): void {
    if (!this.repository) return;

    this.repositoryForm.patchValue({
      name: this.repository.name,
      description: this.repository.description,
      isPrivate: this.repository.isPrivate,
      ownerId: this.repository.ownerId
    });

    this.initialFormValue = this.repositoryForm.getRawValue();
  }
}
