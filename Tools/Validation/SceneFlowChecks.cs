// Run in Play Mode from LV_MainMenu using Unity Pipeline run_script.
// The checks use the real configured scenes and leave the game at the main menu.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneFlowChecks
{
    private static readonly List<string> Passed = new List<string>();
    private static int _loads;
    private static GameManager _manager;

    public static string Run()
    {
        if (!Application.isPlaying || SceneManager.GetActiveScene().name != "LV_MainMenu")
            throw new InvalidOperationException("Start in Play Mode in LV_MainMenu.");
        Passed.Clear();
        _loads = 0;
        _manager = GameManager.Instance;
        _manager.StartCoroutine(Execute());
        return "Started; results will be written to Docs/Validation/scene-flow.txt";
    }

    private static IEnumerator Execute()
    {
        SceneManager.sceneLoaded += CountLoad;
        IEnumerator checks = Checks();
        string failure = null;
        while (true)
        {
            object current = null;
            bool next = false;
            try { next = checks.MoveNext(); if (next) current = checks.Current; }
            catch (Exception exception) { failure = exception.ToString(); }
            if (failure != null || !next) break;
            yield return current;
        }
        SceneManager.sceneLoaded -= CountLoad;
        string directory = Path.Combine(Application.dataPath, "../Docs/Validation");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "scene-flow.txt"),
            DateTime.UtcNow.ToString("u") + "\n" +
            string.Join("\n", Passed) + "\n" +
            (failure == null ? "PASS: All scene-flow checks completed." : "FAIL: " + failure));
        Debug.Log("[SceneFlowChecks] " + (failure ?? "All checks passed."));
    }

    private static void CountLoad(Scene scene, LoadSceneMode mode) { _loads++; }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        Passed.Add("PASS: " + message);
    }

    private static IEnumerator Checks()
    {
        Check(_manager != null && _manager.CurrentState == GameState.MainMenu, "Initial menu state");
        var button = UnityEngine.Object.FindFirstObjectByType<LevelSelectButton>(FindObjectsInactive.Include)
            .GetComponent<UnityEngine.UI.Button>();
        Check(button.onClick.GetPersistentEventCount() == 1, "Exactly one level button callback");
        button.onClick.Invoke();
        button.onClick.Invoke();
        float deadline = Time.realtimeSinceStartup + 30f;
        while (_manager.IsLoading && Time.realtimeSinceStartup < deadline) yield return null;
        Check(!_manager.IsLoading && _loads == 1, "Two same-frame clicks produce one completed load");
        Check(SceneManager.GetActiveScene().name == "LV_TestScene", "Configured gameplay scene is active");
        Check(_manager.CurrentState == GameState.Playing && Time.timeScale == 1f, "Gameplay state and time scale");
        Check(UnityEngine.Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length == 1,
            "Exactly one persistent GameManager");
        _manager.TogglePause();
        Check(_manager.IsPaused && Time.timeScale == 0f && Cursor.lockState == CursorLockMode.None, "Pause state/time/cursor");
        _manager.TogglePause();
        Check(_manager.IsPlaying && Time.timeScale == 1f, "Resume state/time");
        _manager.ReturnToMainMenu();
        deadline = Time.realtimeSinceStartup + 30f;
        while (_manager.IsLoading && Time.realtimeSinceStartup < deadline) yield return null;
        Check(!_manager.IsLoading && SceneManager.GetActiveScene().name == "LV_MainMenu", "Return to menu");
        _manager.LoadLevel(0);
        deadline = Time.realtimeSinceStartup + 30f;
        while (_manager.IsLoading && Time.realtimeSinceStartup < deadline) yield return null;
        Check(!_manager.IsLoading && _loads == 3 && _manager.IsPlaying, "Second journey loads once");
        _manager.ReturnToMainMenu();
        deadline = Time.realtimeSinceStartup + 30f;
        while (_manager.IsLoading && Time.realtimeSinceStartup < deadline) yield return null;
        Check(!_manager.IsLoading && _loads == 4, "Second menu return");

        // With gameplay as the persistent scene, the first load replaces the menu;
        // the second loads the menu additively. This exercises two distinct queued
        // loads without creating disposable fixture assets or changing build settings.
        typeof(GameManager).GetField("persistentGameplayScene", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(_manager, "LV_TestScene");
        _manager.LoadLevelAdditive("LV_TestScene", "LV_MainMenu", "LV_MainMenu");
        _manager.LoadLevelAdditive("LV_TestScene", "LV_MainMenu");
        deadline = Time.realtimeSinceStartup + 30f;
        while (_manager.IsLoading && Time.realtimeSinceStartup < deadline) yield return null;
        Check(!_manager.IsLoading && _loads == 6, "Two distinct additive loads finish; duplicate requests are ignored");
        Check(SceneManager.sceneCount == 2 && _manager.IsPlaying, "Both requested scenes loaded and state restored");
        _manager.ReturnToMainMenu();
        deadline = Time.realtimeSinceStartup + 30f;
        while (_manager.IsLoading && Time.realtimeSinceStartup < deadline) yield return null;
        Check(!_manager.IsLoading && SceneManager.sceneCount == 1 && _manager.CurrentState == GameState.MainMenu,
            "Final cleanup returns to a single menu scene");
    }
}
