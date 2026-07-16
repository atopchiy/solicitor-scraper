# Solicitor Scraper

Scrapes conveyancing solicitors' contact details by location from
[solicitors.com](https://www.solicitors.com/conveyancing.html) and turns the results into a
small report. Built as a .NET 8 Web API + Angular SPA.

## What it does

- Keeps an adjustable list of locations (seeded with London, Birmingham, Leeds, Manchester,
  Sheffield, Bradford, Liverpool, Bristol). Locations can be added, removed or temporarily
  disabled from the UI.
- On "Run search" it fetches the results page for every enabled location in parallel and
  parses each listing by hand (no HTML parsing libraries) — name, phone, address, website,
  profile link, star rating, review count, quality marks and description.
- Every run is stored in Postgres, so there is a run history. The report flags solicitors
  that haven't been seen in any previous run. (The diff is against the union of all earlier
  runs rather than just the last one, because the site rotates its non-featured listings
  between requests — a last-run-only comparison would flag rotated-back-in firms as new.)
- The report view shows totals per location, average ratings, quality-mark coverage and a
  national top-10 ranked by star rating weighted by review volume.

## Requirements

- .NET 8 SDK
- Node 20+ (Angular 20)
- Docker (for Postgres), or a local Postgres instance

## Running it

1. Start Postgres:

   ```bash
   docker compose up -d
   ```

   This starts Postgres 16 on port **5433** (not 5432, to avoid clashing with a local
   install) with database/user/password all set to match the connection string below.

2. Start the API:

   ```bash
   cd backend
   dotnet run --project src/SolicitorScraper.Api
   ```

   The API listens on `http://localhost:5055`. It applies the EF Core migration on startup,
   so the schema and the seeded locations are created automatically — no manual DB setup
   needed.

3. Start the frontend:

   ```bash
   cd frontend
   npm install
   npm start
   ```

   Open `http://localhost:4200`. The dev server proxies `/api` to the backend
   (`proxy.conf.json`), so there is nothing to configure.

4. Hit **Run search**. A full scrape of the 8 default locations takes a few seconds.
   Run it a second time to see the run history and the "new since last run" detection.

## Configuration

The connection string lives in
[`backend/src/SolicitorScraper.Api/appsettings.json`](backend/src/SolicitorScraper.Api/appsettings.json):

```json
"ConnectionStrings": {
  "Default": "Host=localhost;Port=5433;Database=solicitor_scraper;Username=solicitor;Password=solicitor"
}
```

If you use your own Postgres instead of the docker-compose one, point this at it. The schema
is also available as a plain SQL script in [`db/schema.sql`](db/schema.sql) (generated from
the EF migration, includes the seeded locations) in case you want to create the database by
hand.

## Tests

```bash
cd backend
dotnet test
```

The parser is tested against a saved copy of a real results page
(`tests/SolicitorScraper.Tests/Fixtures/conveyancing-london.html`) plus synthetic edge cases,
and the scrape service is tested with a fake HTTP client (disabled locations, partial
failures).

## How it's put together

```
backend/
  src/SolicitorScraper.Domain          entities, repository/service interfaces, report models
  src/SolicitorScraper.Infrastructure  scraper client + hand-rolled parser, EF Core, services
  src/SolicitorScraper.Api             controllers, DI, CORS
  tests/SolicitorScraper.Tests         xUnit
frontend/                              Angular 20, standalone components, plain SCSS
```

The scraping detail worth mentioning: the site's search form just 302-redirects to a static
page per location (`conveyancing+london.html`), so the client does a plain GET instead of
driving the form. The parser is string-scanning (`IndexOf`-based block extraction) rather
than regex or a DOM library, per the brief.

## API

| Method | Route | |
|---|---|---|
| GET | `/api/locations` | list locations |
| POST | `/api/locations` | add `{ "name": "York" }` |
| PATCH | `/api/locations/{id}` | enable/disable `{ "isEnabled": false }` |
| DELETE | `/api/locations/{id}` | remove |
| POST | `/api/searches` | run a scrape, returns the run summary |
| GET | `/api/searches` | run history |
| GET | `/api/searches/{id}` | full results of a run |
| GET | `/api/searches/{id}/report` | aggregated report for a run |
