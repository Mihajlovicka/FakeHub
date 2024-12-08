import { Component, Input } from '@angular/core';
import {CommonModule, DatePipe} from "@angular/common";
import {UserProfileResponseDto} from "../../../core/model/user";

@Component({
  selector: 'app-view-organizations-members',
  standalone: true,
  imports: [CommonModule],
  providers: [DatePipe],
  templateUrl: './view-organizations-members.component.html',
  styleUrl: './view-organizations-members.component.css'
})
export class ViewOrganizationsMembersComponent {
  @Input() public users: UserProfileResponseDto[] = [];

  constructor() {
    console.log(this.users.length)
  }
}
