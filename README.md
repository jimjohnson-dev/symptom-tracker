# SymptomTracker

A C# .NET 10 WebAPI for tracking and correlating symptoms related to Pseudotumor Cerebri (IIH) with barometric pressure changes over time.

## Background

Pseudotumor Cerebri causes increased intracranial pressure, causing chronic headaches, visual deficits, impaired or foggy cognition, and increased optic nerve pressure. Many patients report sensitivity to atmospheric pressure changes based on anecdotal evidence. Without tracked data, proving increases in symptoms are related to weather pattern changes is difficult.

In response, this project provides a mechanism for IIH patients and their caregivers to record and analyze data trends.

## Features

- Logs symptom entries: head pressure, eye pressure, vision clarity, fatigue, nausea, aphasia, confusion
- Captures environmental snapshots with barometric pressure readings
- Computes pressure differences for given time windows at write time
- Runs Pearson correlation analysis between pressure and symptom severity
- Stores all results in a local SQLite database

## Project Status

> Work in progress, built as a learning project

## Future Milestones
1. Stubbed static weather data -> Real weather API integration
2. Caregiver input capture
3. Auth
4. Notifications on significant pressure changes, scheduled logging reminders, etc