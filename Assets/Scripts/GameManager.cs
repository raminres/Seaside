using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private GameStateSo gameState;
    [SerializeField] private GameEventSo onGameStateChangeEvent;
    [SerializeField] private GameEventSo onGamePaused;
    [SerializeField] private GameEventSo onGameResumed;

    [Header("Scene Management")]
    [SerializeField] private string mainMenuSceneName = "LV_MainMenu";
    [SerializeField] private string persistentGameplayScene = "Main"; // New: persistent scene for additive loading
    [SerializeField] private string[] levelScenes = { "LV_Level1", "LV_Level2", "LV_Level3" };

    [Header("Additive Scene Loading")]
    [SerializeField] private FloatEventSo onLoadProgress;
    [SerializeField] private bool useAdditiveLoading = true;

    [Header("Volume Settings")]
    [SerializeField] private GameEventSo onVolumeChangeEvent;

    [Header("Collectible Settings")]
    [SerializeField] private GameObject collectiblePrefab;
    [SerializeField] private int collectibleCount = 10;
    [SerializeField] private float minX = -10f, maxX = 10f;
    [SerializeField] private float minZ = -10f, maxZ = 10f;
    [SerializeField] private float yPosition = 1f;
    [SerializeField] private AudioClip collectibleAudio;

    [Header("UI References")]
    private TextMeshProUGUI counterText;
    private int collectedCount = 0;

    [Header("Win State UI")]
    private GameObject winGameCanvas;
    private Animator winGameAnimator;

    [Header("Level Selection UI")]
    [SerializeField] private GameObject levelSelectionCanvas;
    [SerializeField] private Animator levelSelectionAnimator;

    // Properties
    public GameState CurrentState => gameState != null ? gameState.CurrentState : GameState.MainMenu;
    public bool IsPaused => CurrentState == GameState.Paused;
    public bool IsPlaying => CurrentState == GameState.Playing;

    public bool IsLoading { get; private set; }

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Disable VSync so targetFrameRate is respected
        QualitySettings.vSyncCount = 0;
        
        // Load FPS setting from PlayerPrefs (default 60)
        int savedFPS = PlayerPrefs.GetInt("TargetFPS", 60);
        Application.targetFrameRate = savedFPS;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    
        // The initial sceneLoaded event occurs before Start, so initialize it explicitly.
        Scene initialScene = SceneManager.GetActiveScene();
        OnSceneLoaded(initialScene, LoadSceneMode.Single);
        ChangeGameState(initialScene.name == mainMenuSceneName
            ? GameState.MainMenu : GameState.Playing);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }
    private void ClearMainMenuReferences()
    {
        levelSelectionCanvas = null;
        levelSelectionAnimator = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == mainMenuSceneName)
        {
            AssignLevelSelectionUI();
            ChangeGameState(GameState.MainMenu);
        }
        else
        {
            // Clear main menu references when entering any other scene
            ClearMainMenuReferences();
        
            if (scene.name != persistentGameplayScene)
            {
                AssignCounterText();
                AssignWinGameCanvas();
                ResetCollectibles();
            }
        }
    }
    private void AssignLevelSelectionUI()
    {
        MainMenuController mainMenu = FindAnyObjectByType<MainMenuController>(FindObjectsInactive.Include);
    
        if (mainMenu != null)
        {
            levelSelectionCanvas = mainMenu.levelSelectionCanvas;
            levelSelectionAnimator = mainMenu.levelSelectionAnimator;
        }
        else
        {
            levelSelectionCanvas = null;
            levelSelectionAnimator = null;
        }
    }

    #endregion

    #region Game State Management

    public void ChangeGameState(GameState newState)
    {
        if (gameState == null)
        {
            Debug.LogWarning("GameStateSO reference is missing in GameManager!");
            return;
        }

        GameState previousState = gameState.CurrentState;
        gameState.CurrentState = newState;

        HandleStateTransition(previousState, newState);
        onGameStateChangeEvent?.RaiseEvent();
    }

    private void HandleStateTransition(GameState from, GameState to)
    {
        switch (to)
        {
            case GameState.Paused:
                Time.timeScale = 0f;
                onGamePaused?.RaiseEvent();
                break;

            case GameState.Playing:
                Time.timeScale = 1f;
                if (from == GameState.Paused)
                {
                    onGameResumed?.RaiseEvent();
                }
                break;

            case GameState.MainMenu:
            case GameState.GameOver:
                Time.timeScale = 1f;
                break;
        }

        UpdateCursorState();
    }

    private void UpdateCursorState()
    {
        bool showCursor = CurrentState == GameState.MainMenu || 
                          CurrentState == GameState.Paused || 
                          CurrentState == GameState.GameOver;
        
        Cursor.visible = showCursor;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
        {
            ChangeGameState(GameState.Paused);
        }
        else if (CurrentState == GameState.Paused)
        {
            ChangeGameState(GameState.Playing);
        }
    }

    public void SetPlaying()
    {
        ChangeGameState(GameState.Playing);
    }

    public void SetPaused()
    {
        ChangeGameState(GameState.Paused);
    }

    #endregion

    #region Scene Loading

    /// <summary>
    /// Load a level by index (legacy method - kept for compatibility).
    /// </summary>
    public void LoadLevel(int levelIndex)
    {
        if (IsLoading) return;

        if (levelIndex < 0 || levelIndex >= levelScenes.Length)
        {
            Debug.LogError($"Invalid level index: {levelIndex}");
            return;
        }

        if (useAdditiveLoading)
        {
            LoadLevelAdditive(levelScenes[levelIndex]);
        }
        else
        {
            if (!CanLoadScene(levelScenes[levelIndex])) return;
            StartCoroutine(LoadSingleLevelAsync(levelScenes[levelIndex]));
        }
    }

    private IEnumerator LoadSingleLevelAsync(string sceneName)
    {
        IsLoading = true;
        try
        {
            onLoadProgress?.RaiseEvent(0f);
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (!operation.isDone)
            {
                onLoadProgress?.RaiseEvent(Mathf.Clamp01(operation.progress / 0.9f));
                yield return null;
            }
            onLoadProgress?.RaiseEvent(1f);
            ChangeGameState(GameState.Playing);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanLoadScene(string sceneName)
    {
        if (!string.IsNullOrWhiteSpace(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName))
            return true;

        Debug.LogError($"[GameManager] Scene '{sceneName}' is not available in Build Settings.");
        return false;
    }

    /// <summary>
    /// Load a level with additive scene loading.
    /// </summary>
    public void LoadLevelAdditive(params string[] sceneNames)
    {
        if (IsLoading || sceneNames == null || sceneNames.Length == 0) return;

        // Validate the whole request before unloading anything or changing game state.
        var requestedScenes = new List<string>();
        if (!string.IsNullOrEmpty(persistentGameplayScene))
            requestedScenes.Add(persistentGameplayScene);

        foreach (string sceneName in sceneNames)
        {
            if (!CanLoadScene(sceneName)) return;
            if (!requestedScenes.Contains(sceneName)) requestedScenes.Add(sceneName);
        }
        if (!string.IsNullOrEmpty(persistentGameplayScene) && !CanLoadScene(persistentGameplayScene)) return;

        StartCoroutine(LoadLevelAdditiveAsync(requestedScenes));
    }

    private IEnumerator LoadLevelAdditiveAsync(List<string> sceneNames)
    {
        IsLoading = true;
        try
        {
            ChangeGameState(GameState.MainMenu);
            onLoadProgress?.RaiseEvent(0f);

            for (int i = 0; i < sceneNames.Count; i++)
            {
                string sceneName = sceneNames[i];
                if (!IsSceneLoaded(sceneName))
                {
                    LoadSceneMode mode = sceneName == persistentGameplayScene
                        ? LoadSceneMode.Single : LoadSceneMode.Additive;
                    // Finish activation before queuing the next load. Holding a scene at
                    // 0.9 with allowSceneActivation=false stalls Unity's async queue.
                    var operation = SceneManager.LoadSceneAsync(sceneName, mode);
                    while (!operation.isDone)
                    {
                        onLoadProgress?.RaiseEvent((i + Mathf.Clamp01(operation.progress / 0.9f)) / sceneNames.Count);
                        yield return null;
                    }
                }
                onLoadProgress?.RaiseEvent((i + 1f) / sceneNames.Count);
            }

            ChangeGameState(GameState.Playing);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Unload a specific additive scene.
    /// </summary>
    public void UnloadScene(string sceneName)
    {
        if (IsSceneLoaded(sceneName))
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }
    }

    /// <summary>
    /// Return to main menu.
    /// </summary>
    public void ReturnToMainMenu()
    {
        if (IsLoading || !CanLoadScene(mainMenuSceneName)) return;
        StartCoroutine(ReturnToMainMenuAsync());
    }

    private IEnumerator ReturnToMainMenuAsync()
    {
        IsLoading = true;
        try
        {
            Time.timeScale = 1f;
            onLoadProgress?.RaiseEvent(0f);
            var op = SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Single);
            while (!op.isDone)
            {
                onLoadProgress?.RaiseEvent(Mathf.Clamp01(op.progress / 0.9f));
                yield return null;
            }
            onLoadProgress?.RaiseEvent(1f);
            ChangeGameState(GameState.MainMenu);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    #endregion

    #region Level Selection UI

    public void ShowLevelSelection()
    {
        if (levelSelectionCanvas == null) return;
    
        levelSelectionCanvas.SetActive(true);
        levelSelectionAnimator?.SetTrigger("Appear");
    }

    public void HideLevelSelection()
    {
        if (levelSelectionCanvas == null) return;

        if (levelSelectionAnimator != null)
        {
            levelSelectionAnimator.SetTrigger("Disappear");
            StartCoroutine(DisableCanvasAfterAnimation(levelSelectionCanvas, levelSelectionAnimator, "Disappear"));
        }
        else
        {
            levelSelectionCanvas.SetActive(false);
        }
    }

    public void SelectLevel(int levelIndex)
    {
        HideLevelSelection();
        LoadLevel(levelIndex);
    }

    private IEnumerator DisableCanvasAfterAnimation(GameObject canvas, Animator animator, string animationName)
    {
        float animationLength = GetAnimationClipLength(animator, animationName);
        yield return new WaitForSeconds(animationLength);
    
        // Check if canvas still exists before accessing it
        if (canvas != null)
        {
            canvas.SetActive(false);
        }
    }

    private float GetAnimationClipLength(Animator animator, string clipName)
    {
        if (animator.runtimeAnimatorController == null) return 0.5f;

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }
        return 0.5f;
    }

    #endregion

    #region Collectibles

    private void AssignCounterText()
    {
        GameObject counterObject = GameObject.Find("CollectibleCounter");
        if (counterObject != null)
        {
            counterText = counterObject.GetComponent<TextMeshProUGUI>();
        }
        UpdateCounterUI();
    }

    private void AssignWinGameCanvas()
    {
        winGameCanvas = GameObject.Find("CanvasWinGame");
        if (winGameCanvas != null)
        {
            winGameAnimator = winGameCanvas.GetComponent<Animator>();
            winGameCanvas.SetActive(false);
        }
    }

    public void ResetCollectibles()
    {
        collectedCount = 0;
        UpdateCounterUI();
        
        if (collectiblePrefab != null)
        {
            SpawnCollectibles();
        }
    }

    public void UpdateCounterUI()
    {
        if (counterText != null)
        {
            counterText.text = collectedCount.ToString();
        }
    }

    public void SpawnCollectibles()
    {
        for (int i = 0; i < collectibleCount; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(minX, maxX),
                yPosition,
                Random.Range(minZ, maxZ)
            );

            GameObject collectible = Instantiate(collectiblePrefab, randomPosition, Quaternion.identity);
        
            var collectibleComponent = collectible.GetComponent<CollectibleItem>();  // Changed from Collectible
            if (collectibleComponent != null)
            {
                collectibleComponent.Initialize(collectibleAudio);
            }
        }
    }

    public void CollectItem()
    {
        collectedCount++;
        UpdateCounterUI();

        if (collectedCount >= collectibleCount)
        {
            Debug.Log("All collectibles collected! Player Wins!");
            WinGame();
        }
    }

    private void WinGame()
    {
        ChangeGameState(GameState.GameOver);
        
        if (winGameCanvas != null)
        {
            winGameCanvas.SetActive(true);
            winGameAnimator?.SetTrigger("Appear");
        }
    }

    #endregion

    #region Audio

    public void UpdateVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.Save();
        onVolumeChangeEvent?.RaiseEvent();
    }

    public float GetVolume()
    {
        return PlayerPrefs.GetFloat("Volume", 1f);
    }

    #endregion

    #region FPS Settings

    /// <summary>
    /// Set target frame rate. Use 30 or 60.
    /// </summary>
    public void SetTargetFPS(int fps)
    {
        // Clamp to valid values
        fps = fps <= 30 ? 30 : 60;
        
        Application.targetFrameRate = fps;
        PlayerPrefs.SetInt("TargetFPS", fps);
        PlayerPrefs.Save();
        
        Debug.Log($"[GameManager] Target FPS set to {fps}");
    }

    /// <summary>
    /// Toggle between 30 and 60 FPS.
    /// </summary>
    public void ToggleFPS()
    {
        int currentFPS = GetTargetFPS();
        int newFPS = currentFPS == 30 ? 60 : 30;
        SetTargetFPS(newFPS);
    }

    /// <summary>
    /// Get current target FPS setting.
    /// </summary>
    public int GetTargetFPS()
    {
        return PlayerPrefs.GetInt("TargetFPS", 60);
    }

    /// <summary>
    /// Check if high FPS mode (60) is enabled.
    /// </summary>
    public bool IsHighFPSEnabled()
    {
        return GetTargetFPS() == 60;
    }

    /// <summary>
    /// Set high FPS mode on or off.
    /// </summary>
    public void SetHighFPSMode(bool enabled)
    {
        SetTargetFPS(enabled ? 60 : 30);
    }

    #endregion

    #region Application

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion
}
