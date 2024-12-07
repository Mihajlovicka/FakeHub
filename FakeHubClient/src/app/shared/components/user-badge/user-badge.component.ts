import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { UserBadge } from '../../../core/model/user';

@Component({
  selector: 'app-user-badge',
  standalone: true,
  imports: [ CommonModule, MatIconModule],
  templateUrl: './user-badge.component.html',
  styleUrl: './user-badge.component.css'
})
export class UserBadgeComponent implements OnChanges {
  @Input() userBadge: UserBadge = UserBadge.None;
  @Input() showNone: boolean = false;
  public isVerifiedUserBadge: boolean = false;
  public  isSponsoredOssBadge: boolean = false;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['userBadge']) {
      this.isSponsoredOssBadge = this.userBadge == UserBadge.SponsoredOSS;
      this.isVerifiedUserBadge = this.userBadge == UserBadge.VerifiedPublisher;
    }
  }
}
