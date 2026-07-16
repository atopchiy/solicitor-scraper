import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Location, RunReport, RunSummary, SearchRun } from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);

  getLocations(): Observable<Location[]> {
    return this.http.get<Location[]>('/api/locations');
  }

  addLocation(name: string): Observable<Location> {
    return this.http.post<Location>('/api/locations', { name });
  }

  deleteLocation(id: number): Observable<void> {
    return this.http.delete<void>(`/api/locations/${id}`);
  }

  setLocationEnabled(id: number, isEnabled: boolean): Observable<Location> {
    return this.http.patch<Location>(`/api/locations/${id}`, { isEnabled });
  }

  runSearch(): Observable<RunSummary> {
    return this.http.post<RunSummary>('/api/searches', {});
  }

  getRuns(): Observable<RunSummary[]> {
    return this.http.get<RunSummary[]>('/api/searches');
  }

  getRun(id: number): Observable<SearchRun> {
    return this.http.get<SearchRun>(`/api/searches/${id}`);
  }

  getReport(id: number): Observable<RunReport> {
    return this.http.get<RunReport>(`/api/searches/${id}/report`);
  }
}
