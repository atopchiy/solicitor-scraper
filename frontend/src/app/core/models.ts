export interface Location {
  id: number;
  name: string;
  isEnabled: boolean;
}

export interface RunSummary {
  id: number;
  startedAt: string;
  completedAt: string | null;
  resultCount: number;
  failedLocations: string[];
}

export interface SolicitorResult {
  id: number;
  location: string;
  name: string;
  phone: string | null;
  address: string | null;
  website: string | null;
  profileUrl: string | null;
  description: string | null;
  qualityMarks: string | null;
  rating: number | null;
  reviewCount: number | null;
  isFeatured: boolean;
}

export interface SearchRun {
  id: number;
  startedAt: string;
  completedAt: string | null;
  results: SolicitorResult[];
  failedLocations: string[];
}

export interface LocationSummary {
  location: string;
  count: number;
  averageRating: number | null;
  featuredCount: number;
  withQualityMarks: number;
}

export interface RankedFirm {
  name: string;
  locationCount: number;
  rating: number;
  reviewCount: number;
  score: number;
}

export interface NewSolicitor {
  name: string;
  location: string;
}

export interface RunReport {
  runId: number;
  startedAt: string;
  totalSolicitors: number;
  locationsCovered: number;
  averageRating: number | null;
  withQualityMarks: number;
  featuredCount: number;
  locations: LocationSummary[];
  topRated: RankedFirm[];
  newSinceLastRun: NewSolicitor[];
  failedLocations: string[];
}
