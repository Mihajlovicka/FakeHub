import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { RepositoryOwnedBy } from '../../../core/model/repository';
import { UserBadge } from '../../../core/model/user';

export const IconSize = {
  Small : "small-icon",
  Medium : "medium-icon",
  Large : "large-icon"
}

@Component({
  selector: 'app-repository-badge',
  standalone: true,
  imports: [ CommonModule, MatIconModule],
  templateUrl: './repository-badge.component.html',
  styleUrl: './repository-badge.component.css'
})
export class RepositoryBadgeComponent implements OnChanges {
  @Input() iconSize: string = IconSize.Large;
  @Input() ownedBy: number | undefined;
  @Input() ownerBadge: UserBadge | undefined;
  public showOfficialBadge: boolean = false;
  public showVerifiedPublisher: boolean = false;
  public showSponsoredOss: boolean = false;

  ngOnChanges(): void {
    this.showOfficialBadge = this.ownedBy === RepositoryOwnedBy.ADMIN || this.ownedBy === RepositoryOwnedBy.SUPERADMIN;
    this.showVerifiedPublisher = this.ownedBy === RepositoryOwnedBy.USER && this.ownerBadge === UserBadge.VerifiedPublisher;
    this.showSponsoredOss = this.ownedBy === RepositoryOwnedBy.USER && this.ownerBadge === UserBadge.SponsoredOSS;
  }
} 