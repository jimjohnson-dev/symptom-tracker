# Symptom Tracker API

> [!NOTE]
> Made for patients diagnosed with Ideopathic Intracranial Hypertension (IIH)/PseudoTumor Ceribrii 

A C# .NET 10 WebAPI for tracking and correlating symptoms related to Pseudotumor Cerebri (IIH) with barometric pressure changes over time.

## Background

Pseudotumor Cerebri causes increased intracranial pressure, causing chronic headaches, visual deficits, impaired or foggy cognition, and increased optic nerve pressure. Many patients report sensitivity to atmospheric pressure changes based on anecdotal evidence. Without tracked data, proving increases in symptoms are related to weather pattern changes is difficult.

In response, this project lets IIH patients and their caregivers record and analyze related data trends.

## Features

- Logs symptom entries: head pressure, eye pressure, vision clarity, fatigue, nausea, aphasia, confusion
- Captures environmental snapshots with barometric pressure readings
- Computes pressure differences for given time windows at write time
- Runs Pearson correlation analysis between pressure and symptom severity
- Stores all results in a local SQLite database

## Project Status

> Work in progress, built as a learning project

---

## Planned but Not Built

- **Real weather API integration** — connect to Open-Meteo (free, no key) or OpenWeatherMap
- **Authentication** — JWT or API key gating for a hosted deployment
- **Caregiver sync mode** — Phase 2: a caregiver logs observations separately; entries are merged with role attribution for display but filtered from severity calculations
- **Push notifications** — alert patient when a significant pressure drop is detected
- **Time-series charts** — visualize pressure vs. severity overlay over time
- **Export** — CSV or PDF report for sharing with a neurologist


---

## Running Locally

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Start the API

```bash
cd src/SymptomTracker.Api
dotnet run
```

The API starts on `http://localhost:5014` by default. The SQLite database file `symptomtracker.db` is created automatically in the working directory on first run.

Open Swagger UI: [http://localhost:5014/swagger](http://localhost:5014/swagger)

### Run Tests

```bash
dotnet test
```

Or for verbose output:

```bash
dotnet test -v normal
```

---
