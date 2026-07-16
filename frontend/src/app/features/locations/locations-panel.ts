import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { Location } from '../../core/models';

@Component({
  selector: 'app-locations-panel',
  imports: [FormsModule],
  templateUrl: './locations-panel.html',
  styleUrl: './locations-panel.scss'
})
export class LocationsPanel implements OnInit {
  private api = inject(ApiService);

  locations = signal<Location[]>([]);
  newName = '';
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.getLocations().subscribe(locations => this.locations.set(locations));
  }

  add(): void {
    const name = this.newName.trim();
    if (!name) return;

    this.error.set(null);
    this.api.addLocation(name).subscribe({
      next: () => {
        this.newName = '';
        this.load();
      },
      error: err => this.error.set(err.error?.error ?? 'Could not add location.')
    });
  }

  remove(location: Location): void {
    this.api.deleteLocation(location.id).subscribe(() => this.load());
  }

  toggle(location: Location): void {
    this.api.setLocationEnabled(location.id, !location.isEnabled).subscribe(() => this.load());
  }
}
