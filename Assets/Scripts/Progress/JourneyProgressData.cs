using System;
using System.Collections.Generic;

/// <summary>
/// Serialized, versioned progress for one local journey. IDs are authored content
/// identifiers, never references to scene objects or static gameplay fields.
/// </summary>
[Serializable]
public sealed class JourneyProgressData
{
    public const int CurrentVersion = 1;

    public int version = CurrentVersion;
    public string checkpointId = string.Empty;
    public float timeOfDay;
    public List<string> discoveredNoteIds = new();
    public List<string> inventoryItemIds = new();
    public List<string> completedActionIds = new();

    public void Normalize()
    {
        version = version <= 0 ? CurrentVersion : version;
        checkpointId ??= string.Empty;
        discoveredNoteIds ??= new List<string>();
        inventoryItemIds ??= new List<string>();
        completedActionIds ??= new List<string>();
    }
}

public static class JourneyActionIds
{
    public const string ArrivalComplete = "arrival_complete";
}
