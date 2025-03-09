import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTabsModule } from '@angular/material/tabs';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Repository } from '../../../core/model/repository';
import { RepositoryService } from '../../../core/services/repository.service';
import { Subscription, take } from 'rxjs';
import { HelperService } from '../../../core/services/helper.service';
import { UserService } from '../../../core/services/user.service';

@Component({
  selector: 'app-view-repository',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatMenuModule,
    MatButtonModule,
    RouterModule,
    MatTabsModule
  ],
  templateUrl: './view-repository.component.html',
  styleUrl: './view-repository.component.css'
})
export class ViewRepositoryComponent implements OnInit, OnDestroy{
  public repository!: Repository;
  public capitalizedLetterAvatar: string = "";

  private readonly repositoryService: RepositoryService = inject(RepositoryService);
  private readonly activatedRoute: ActivatedRoute = inject(ActivatedRoute);
  private readonly helperService: HelperService = inject(HelperService);
  private readonly userService: UserService = inject(UserService);
  private repositorySubscription: Subscription | null = null;
  private routeSubscription: Subscription | null = null;

  public ngOnInit(){
    const repoId = this.getRepoId();
    if(repoId){
      this.repositorySubscription = this.repositoryService.getRepository(repoId).subscribe(
        data => {
          if(data){
          this.repository = data;
          this.avatarProfile();
          }
        }
      );
    }
  }

  public ngOnDestroy(): void {
    if (this.repositorySubscription) {
      this.repositorySubscription.unsubscribe();
    }
    if(this.routeSubscription){
      this.routeSubscription.unsubscribe();
    }
  }

  public isOwner(): boolean {
    return (
      this.repository.ownerUsername !== null &&
      this.repository.ownerUsername == this.userService.getUserName()
    );
  }

  public avatarProfile(): void {
    this.capitalizedLetterAvatar =  this.helperService.capitalizeFirstLetter(this.repository?.name ?? "");
  }
  
  private getRepoId(): number | undefined{
    let id = undefined;

    this.activatedRoute.paramMap.pipe(take(1)).subscribe(
      route => {
        id = route.get("repositoryId");
      }
    );

    return id;
  }
}
