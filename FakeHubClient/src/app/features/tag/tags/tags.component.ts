import { Component, Input, OnInit } from '@angular/core';
import { Artifact } from '../../../core/model/tag';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-tags',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tags.component.html',
  styleUrl: './tags.component.css'
})
export class TagsComponent implements OnInit{
  @Input() artifacts: Artifact[] = [];

  filteredArtifacts: Artifact[] = [];
  searchQuery: string = '';

  selectedSortOption: number = 1;
  sortBy: { id: number; name: string }[] = [
    { id: 1, name: 'A-Z' },
    { id: 2, name: 'Z-A' },
    { id: 3, name: 'Newest' },
    { id: 4, name: 'Oldest' },
  ];

  ngOnInit() {
    this.filteredArtifacts = structuredClone(this.artifacts);
  }

  search() {
    const query = this.searchQuery.trim().toLowerCase();

    if (!query) {
      this.filteredArtifacts = structuredClone(this.artifacts);
      return;
    }

    this.filteredArtifacts = this.artifacts.filter(artifact =>
      artifact.tags[0].name.toLowerCase().includes(query)
    );
  }

  sortArtifacts() {
    console.log(this.selectedSortOption);
    console.log(this.artifacts);
    this.filteredArtifacts = structuredClone(this.artifacts);
    if(this.selectedSortOption == 1){
      this.filteredArtifacts.sort((a,b)=> a.tags[0].name.localeCompare(b.tags[0].name))
    } 
    if(this.selectedSortOption == 2){
      this.filteredArtifacts.sort((a,b)=> b.tags[0].name.localeCompare(a.tags[0].name))
    } else if(this.selectedSortOption == 3){
      this.filteredArtifacts.sort((a, b) => new Date(b.tags[0].pushTime).getTime() - new Date(a.tags[0].pushTime).getTime());
    } else if(this.selectedSortOption == 4){
      this.filteredArtifacts.sort((a, b) => new Date(a.tags[0].pushTime).getTime() - new Date(b.tags[0].pushTime).getTime());
    }
    console.log(this.filteredArtifacts);
  }
}
