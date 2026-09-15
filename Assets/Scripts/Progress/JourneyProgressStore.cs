using System;
using System.Collections.Generic;

/// <summary>
/// Pure, idempotent progress rules. The service owns persistence and event delivery;
/// this class owns only stable-ID state transitions so they can be tested in EditMode.
/// </summary>
public sealed class JourneyProgressStore
{
    private readonly JourneyProgressData _data;

    public JourneyProgressData Data => _data;

    public JourneyProgressStore(JourneyProgressData data = null)
    {
        _data = data ?? new JourneyProgressData();
        _data.Normalize();
    }

    public bool HasCompletedAction(string actionId) => Contains(_data.completedActionIds, actionId);
    public bool HasDiscoveredNote(string noteId) => Contains(_data.discoveredNoteIds, noteId);
    public bool HasInventoryItem(string itemId) => Contains(_data.inventoryItemIds, itemId);

    public bool CompleteAction(string actionId) => AddUnique(_data.completedActionIds, actionId);
    public bool DiscoverNote(string noteId) => AddUnique(_data.discoveredNoteIds, noteId);
    public bool AddInventoryItem(string itemId) => AddUnique(_data.inventoryItemIds, itemId);

    public bool SetCheckpoint(string checkpointId, float timeOfDay)
    {
        string normalizedId = RequireId(checkpointId, nameof(checkpointId));
        if (string.Equals(_data.checkpointId, normalizedId, StringComparison.Ordinal) &&
            Math.Abs(_data.timeOfDay - timeOfDay) < 0.0001f)
        {
            return false;
        }

        _data.checkpointId = normalizedId;
        _data.timeOfDay = timeOfDay;
        return true;
    }

    public void Reset()
    {
        _data.version = JourneyProgressData.CurrentVersion;
        _data.checkpointId = string.Empty;
        _data.timeOfDay = 0f;
        _data.discoveredNoteIds.Clear();
        _data.inventoryItemIds.Clear();
        _data.completedActionIds.Clear();
    }

    private static bool AddUnique(List<string> ids, string id)
    {
        string normalizedId = RequireId(id, nameof(id));
        if (Contains(ids, normalizedId)) return false;

        ids.Add(normalizedId);
        return true;
    }

    private static bool Contains(List<string> ids, string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;
        string normalizedId = id.Trim();
        foreach (string existingId in ids)
        {
            if (string.Equals(existingId, normalizedId, StringComparison.Ordinal)) return true;
        }

        return false;
    }

    private static string RequireId(string id, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("A stable progress ID is required.", parameterName);
        }

        return id.Trim();
    }
}
