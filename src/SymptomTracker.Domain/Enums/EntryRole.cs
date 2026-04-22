namespace SymptomTracker.Domain.Enums;

/// <summary>
/// Identifier for who created an entry. Patient is the only active role for this stage of the project
/// as the Patient is the primary use case to support.
/// </summary>
public enum EntryRole
{
    Patient,
    Caregiver
}