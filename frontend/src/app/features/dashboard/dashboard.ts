import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { RunReport, RunSummary, SolicitorResult } from '../../core/models';

type SortColumn = 'name' | 'location' | 'rating' | 'reviewCount';

@Component({
  selector: 'app-dashboard',
  imports: [DatePipe, DecimalPipe, FormsModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  private api = inject(ApiService);

  runs = signal<RunSummary[]>([]);
  selectedRunId = signal<number | null>(null);
  report = signal<RunReport | null>(null);
  results = signal<SolicitorResult[]>([]);
  scraping = signal(false);
  loading = signal(false);
  error = signal<string | null>(null);

  locationFilter = 'all';
  searchTerm = '';
  private filterVersion = signal(0);
  private sortColumn = signal<SortColumn>('rating');
  private sortAsc = signal(false);

  resultLocations = computed(() =>
    [...new Set(this.results().map(r => r.location))].sort());

  private newKeys = computed(() =>
    new Set((this.report()?.newSolicitors ?? []).map(n => `${n.location}|${n.name}`)));

  hasPreviousRun = computed(() => {
    const id = this.selectedRunId();
    return id !== null && this.runs().some(r => r.id < id);
  });

  maxLocationCount = computed(() =>
    Math.max(1, ...(this.report()?.locations.map(l => l.count) ?? [])));

  filteredResults = computed(() => {
    this.filterVersion();
    const term = this.searchTerm.trim().toLowerCase();
    const location = this.locationFilter;
    const column = this.sortColumn();
    const direction = this.sortAsc() ? 1 : -1;

    return this.results()
      .filter(r => location === 'all' || r.location === location)
      .filter(r => !term
        || r.name.toLowerCase().includes(term)
        || (r.address ?? '').toLowerCase().includes(term))
      .sort((a, b) => direction * this.compare(a, b, column));
  });

  ngOnInit(): void {
    this.loadRuns(true);
  }

  runSearch(): void {
    this.scraping.set(true);
    this.error.set(null);
    this.api.runSearch().subscribe({
      next: summary => {
        this.scraping.set(false);
        this.loadRuns(false);
        this.selectRun(summary.id);
      },
      error: err => {
        this.scraping.set(false);
        this.error.set(err.error?.error ?? 'The search failed. Is the API running?');
      }
    });
  }

  selectRun(id: number): void {
    this.selectedRunId.set(id);
    this.loading.set(true);
    forkJoin({
      run: this.api.getRun(id),
      report: this.api.getReport(id)
    }).subscribe({
      next: ({ run, report }) => {
        this.results.set(run.results);
        this.report.set(report);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Could not load the selected run.');
      }
    });
  }

  onRunPicked(value: string): void {
    this.selectRun(Number(value));
  }

  refreshFilters(): void {
    this.filterVersion.update(v => v + 1);
  }

  setSort(column: SortColumn): void {
    if (this.sortColumn() === column) {
      this.sortAsc.update(asc => !asc);
    } else {
      this.sortColumn.set(column);
      this.sortAsc.set(column === 'name' || column === 'location');
    }
  }

  sortIndicator(column: SortColumn): string {
    if (this.sortColumn() !== column) return '';
    return this.sortAsc() ? '▲' : '▼';
  }

  isNew(result: SolicitorResult): boolean {
    return this.hasPreviousRun() && this.newKeys().has(`${result.location}|${result.name}`);
  }

  barWidth(count: number): string {
    return `${Math.round((count / this.maxLocationCount()) * 100)}%`;
  }

  stars(rating: number): string {
    const full = Math.floor(rating);
    return '★'.repeat(full) + (rating - full >= 0.5 ? '½' : '');
  }

  private loadRuns(selectLatest: boolean): void {
    this.api.getRuns().subscribe(runs => {
      this.runs.set(runs);
      if (selectLatest && runs.length > 0) {
        this.selectRun(runs[0].id);
      }
    });
  }

  private compare(a: SolicitorResult, b: SolicitorResult, column: SortColumn): number {
    switch (column) {
      case 'name': return a.name.localeCompare(b.name);
      case 'location': return a.location.localeCompare(b.location) || a.name.localeCompare(b.name);
      case 'rating': return (a.rating ?? -1) - (b.rating ?? -1) || (a.reviewCount ?? 0) - (b.reviewCount ?? 0);
      case 'reviewCount': return (a.reviewCount ?? -1) - (b.reviewCount ?? -1);
    }
  }
}
