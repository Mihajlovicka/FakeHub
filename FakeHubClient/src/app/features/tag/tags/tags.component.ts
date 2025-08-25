import { Component, Input, OnInit } from '@angular/core';
import { Artifact } from '../../../core/model/tag';
import { CommonModule } from '@angular/common';
import { MatIcon } from "@angular/material/icon";

@Component({
  selector: 'app-tags',
  standalone: true,
  imports: [CommonModule, MatIcon],
  templateUrl: './tags.component.html',
  styleUrl: './tags.component.css'
})
export class TagsComponent {
  @Input() artifacts: Artifact[] = [];

}
