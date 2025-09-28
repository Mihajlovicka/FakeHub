import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnalyticsService } from '../../core/services/analytics.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.css'] // ispravljeno
})
export class AnalyticsComponent implements OnInit {
  public logs: any[] = [];
  public filtered: any[] = [];
  public levels: string[] = ['Information', 'Warning', 'Error'];
  public levelFilter = '';
  public query = '';
  public expandedIndex: number | null = null;
  public fromDate: string = '';
  public toDate: string = '';

  public constructor(private readonly analyticsService: AnalyticsService) { }

  public ngOnInit(): void {
    this.analyticsService.getElasticLogs(150).subscribe((logs: any[]) => {
      this.logs = logs;
      this.filtered = [...this.logs];
    });
  }

  public applyFilters() {
    const from = this.fromDate || '';
    const to = this.toDate || '';

    this.analyticsService.searchElasticLogs(
      this.query,
      this.levelFilter,
      from,
      to,
      100
    ).subscribe((logs: any[]) => {
      this.filtered = logs;
      this.expandedIndex = null;
    });
  }

  public resetFilters() {
    this.levelFilter = '';
    this.query = '';
    this.fromDate = '';
    this.toDate = '';
    this.analyticsService.getElasticLogs(50).subscribe((logs: any[]) => {
      this.filtered = logs;
      this.expandedIndex = null;
    });
  }

  public toggleExpand(i: number) {
    this.expandedIndex = this.expandedIndex === i ? null : i;
  }

  public downloadJson() {
    const dataStr = JSON.stringify(this.filtered, null, 2);
    const blob = new Blob([dataStr], { type: 'application/json' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'logs.json';
    a.click();
    window.URL.revokeObjectURL(url);
  }
}