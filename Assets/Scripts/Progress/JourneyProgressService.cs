using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Local persistence and event delivery for <see cref="JourneyProgressStore"/>.
/// Attach this to the existing GameManager; it is deliberately not another singleton.
/// </summary>
public sealed class JourneyProgressService : MonoBehaviour
{
    [Header("Persistence")]
    [SerializeField] private string _saveFileName = "journey_progress.json";

    [Header("Events")]
    [SerializeField] private GameEventSo _onJourneyProgressChanged;

    private JourneyProgressStore _store;

    public JourneyProgressData Current => _store.Data;
    public bool HasCheckpoint => !string.IsNullOrEmpty(_store.Data.checkpointId);

    private void Awake()
    {
        _store = new JourneyProgressStore(LoadData());
    }

    public bool HasCompletedAction(string actionId) => _store.HasCompletedAction(actionId);

    public bool CompleteAction(string actionId)
    {
        if (!_store.CompleteAction(actionId)) return false;
        SaveAndNotify();
        return true;
    }

    public bool DiscoverNote(string noteId)
    {
        if (!_store.DiscoverNote(noteId)) return false;
        SaveAndNotify();
        return true;
    }

    public bool AddInventoryItem(string itemId)
    {
        if (!_store.AddInventoryItem(itemId)) return false;
        SaveAndNotify();
        return true;
    }

    public bool SetCheckpoint(string checkpointId, float timeOfDay)
    {
        if (!_store.SetCheckpoint(checkpointId, timeOfDay)) return false;
        SaveAndNotify();
        return true;
    }

    public void StartNewJourney()
    {
        _store.Reset();
        SaveAndNotify();
    }

    private JourneyProgressData LoadData()
    {
        string path = SavePath;
        if (!File.Exists(path)) return new JourneyProgressData();

        try
        {
            JourneyProgressData data = JsonUtility.FromJson<JourneyProgressData>(File.ReadAllText(path));
            if (data == null || data.version > JourneyProgressData.CurrentVersion)
            {
                Debug.LogWarning("[JourneyProgressService] Save is missing or from a newer version; starting a new journey.");
                return new JourneyProgressData();
            }

            data.Normalize();
            return data;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[JourneyProgressService] Could not read save; starting a new journey. {exception.Message}");
            return new JourneyProgressData();
        }
    }

    private void SaveAndNotify()
    {
        try
        {
            string directory = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            string temporaryPath = SavePath + ".tmp";
            File.WriteAllText(temporaryPath, JsonUtility.ToJson(_store.Data, true));
            File.Copy(temporaryPath, SavePath, true);
            File.Delete(temporaryPath);
            _onJourneyProgressChanged?.RaiseEvent();
        }
        catch (Exception exception)
        {
            Debug.LogError($"[JourneyProgressService] Could not save journey progress. {exception.Message}");
        }
    }

    private string SavePath => Path.Combine(Application.persistentDataPath, _saveFileName);
}
