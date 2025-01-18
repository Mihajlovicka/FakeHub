import { Component, inject, Input, OnInit } from '@angular/core';
import { Artifact } from '../../../core/model/tag';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIcon } from "@angular/material/icon";
import { TagService } from '../../../core/services/tag.service';
import { Repository } from '../../../core/model/repository';
import { UserService } from '../../../core/services/user.service';

@Component({
  selector: 'app-tags',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIcon],
  templateUrl: './tags.component.html',
  styleUrl: './tags.component.css'
})
export class TagsComponent implements OnInit{
  @Input() repository!: Repository;

  private readonly userService = inject(UserService);
  private readonly tagsService: TagService = inject(TagService);

  artifacts: Artifact[] = [];
  filteredArtifacts: Artifact[] = [];
  searchQuery: string = '';

  canDeleteTags: boolean = true;

  selectedSortOption: number = 1;
  sortBy: { id: number; name: string }[] = [
    { id: 1, name: 'A-Z' },
    { id: 2, name: 'Z-A' },
    { id: 3, name: 'Newest' },
    { id: 4, name: 'Oldest' },
  ];

  ngOnInit() {
    if(this.repository) {
      this.tagsService.getTags(this.repository.id!).subscribe((artifacts: Artifact[]) => {
        this.artifacts = artifacts;
        this.filteredArtifacts = structuredClone(this.artifacts);
        
        if(this.userService.isLoggedIn())
          this.tagsService.canUserDeleteTags(this.repository.id!).subscribe((canDelete: boolean) => {
            this.canDeleteTags = canDelete;
          });
      }) 
    }
  }

  search() {
    const query = this.searchQuery.trim().toLowerCase();

    if (!query) {
      this.filteredArtifacts = structuredClone(this.artifacts || []);
      return;
    }

    this.filteredArtifacts = this.artifacts.filter(artifact =>
      artifact.tag.name.toLowerCase().includes(query)
    ) || [];
  }

  sortArtifacts() {
    this.filteredArtifacts = structuredClone(this.artifacts || []);
    if(this.selectedSortOption == 1){
      this.filteredArtifacts.sort((a,b)=> a.tag.name.localeCompare(b.tag.name))
    } 
    if(this.selectedSortOption == 2){
      this.filteredArtifacts.sort((a,b)=> b.tag.name.localeCompare(a.tag.name))
    } else if(this.selectedSortOption == 3){
      this.filteredArtifacts.sort((a, b) => new Date(b.tag.pushTime).getTime() - new Date(a.tag.pushTime).getTime());
    } else if(this.selectedSortOption == 4){
      this.filteredArtifacts.sort((a, b) => new Date(a.tag.pushTime).getTime() - new Date(b.tag.pushTime).getTime());
    }
    console.log(this.filteredArtifacts);
  }

  public deleteTag(artifact: Artifact): void{
    if(this.repository) {
      this.tagsService.deleteTag(artifact, this.repository.id!).subscribe((artifacts: Artifact[]) => {
          this.artifacts = artifacts;
          this.filteredArtifacts = structuredClone(this.artifacts || []);
      });
    }
  }

  trackByIndexName(index: number, item: Artifact): string {
    return `${item.id}-${item.tag.name}-${index}`;
  } 
}
