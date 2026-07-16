import { Component } from '@angular/core';
import { Dashboard } from './features/dashboard/dashboard';
import { LocationsPanel } from './features/locations/locations-panel';

@Component({
  selector: 'app-root',
  imports: [Dashboard, LocationsPanel],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {}
