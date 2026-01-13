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

    private List<AsyncOperation> _loadOperations = new();

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
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    
        // Ensure we're in MainMenu state when starting from main menu scene
        if (SceneManager.GetActiveScene().name == mainMenuSceneName)
        {
            if (gameState != null)
            {
                gameState.CurrentState = GameState.MainMenu;
            }
        }
    
        UpdateCursorState();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
        MainMenuController mainMenu = FindFirstObjectByType<MainMenuController>(FindObjectsInactive.Include);
    
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
            SceneManager.LoadScene(levelScenes[levelIndex]);
            ChangeGameState(GameState.Playing);
        }
    }

    /// <summary>
    /// Load a level with additive scene loading.
    /// </summary>
    public void LoadLevelAdditive(params string[] sceneNames)
    {
        StartCoroutine(LoadLevelAdditiveAsync(sceneNames));
    }

    private IEnumerator LoadLevelAdditiveAsync(string[] sceneNames)
    {
        ChangeGameState(GameState.MainMenu); // Use MainMenu state during loading for cursor
        _loadOperations.Clear();

        // First, load the persistent gameplay scene if not already loaded
        if (!string.IsNullOrEmpty(persistentGameplayScene) && !IsSceneLoaded(persistentGameplayScene))
        {
            var mainOp = SceneManager.LoadSceneAsync(persistentGameplayScene, LoadSceneMode.Single);
            mainOp.allowSceneActivation = false;
            _loadOperations.Add(mainOp);
        }

        // Queue all additive scenes
        foreach (var sceneName in sceneNames)
        {
            if (string.IsNullOrEmpty(sceneName) || IsSceneLoaded(sceneName)) continue;

            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            op.allowSceneActivation = false;
            _loadOperations.Add(op);
        }

        if (_loadOperations.Count == 0)
        {
            ChangeGameState(GameState.Playing);
            yield break;
        }

        // Wait and report progress
        float totalProgress = 0f;
        int opCount = _loadOperations.Count;
        
        while (totalProgress < 0.9f * opCount)
        {
            totalProgress = 0f;
            foreach (var op in _loadOperations)
            {
                totalProgress += op.progress;
            }

            float normalizedProgress = totalProgress / (opCount * 0.9f);
            onLoadProgress?.RaiseEvent(normalizedProgress);

            yield return null;
        }

        // Activate all scenes
        foreach (var op in _loadOperations)
        {
            op.allowSceneActivation = true;
        }

        // Wait for all to complete
        foreach (var op in _loadOperations)
        {
            while (!op.isDone)
            {
                yield return null;
            }
        }

        onLoadProgress?.RaiseEvent(1f);
        yield return new WaitForSeconds(0.1f);

        ChangeGameState(GameState.Playing);
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
        StartCoroutine(ReturnToMainMenuAsync());
    }

    private IEnumerator ReturnToMainMenuAsync()
    {
        Time.timeScale = 1f;
        
        var op = SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Single);
        while (!op.isDone)
        {
            onLoadProgress?.RaiseEvent(op.progress);
            yield return null;
        }

        ChangeGameState(GameState.MainMenu);
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
