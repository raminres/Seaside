using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Canvases")]
    public GameObject pauseMenuCanvas;
    public GameObject winGameCanvas;

    [Header("Mobile")]
    [Tooltip("Pause button for mobile - assign a UI button that calls PauseGame()")]
    public GameObject mobilePauseButton;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("Scene Name")]
    public string mainMenuSceneName = "MainMenu";
    public string gameSceneName = "LV_Diaroma";

    private bool isPaused = false;

    [Header("Input Action Asset")]
    public InputActionReference pauseActionReference;

    private PlayerController _playerController;
    private PlayerInput _playerInput;
    private MobileControlsManager _mobileControlsManager;

    private void Awake()
    {
        FindPlayerReferences();
        
        // Ensure AudioSource can play when time is paused
        if (audioSource != null)
        {
            audioSource.ignoreListenerPause = true;
        }

        _mobileControlsManager = FindFirstObjectByType<MobileControlsManager>();
    }

    private void Start()
    {
        // Show mobile pause button on mobile platforms OR if MobileControlsManager exists (editor testing)
        if (mobilePauseButton != null)
        {
            bool showMobileButton = false;
            
            #if UNITY_IOS || UNITY_ANDROID
                showMobileButton = true;
            #endif
            
            // Also show if MobileControlsManager is present and active (editor testing)
            if (_mobileControlsManager != null && _mobileControlsManager.gameObject.activeInHierarchy)
            {
                showMobileButton = true;
            }
            
            mobilePauseButton.SetActive(showMobileButton);
        }

        // Initialize cursor state for desktop
        #if !UNITY_IOS && !UNITY_ANDROID
            // Only lock cursor if not testing mobile in editor
            if (_mobileControlsManager == null || !_mobileControlsManager.gameObject.activeInHierarchy)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        #endif
    }

    private void FindPlayerReferences()
    {
        _playerController = FindFirstObjectByType<PlayerController>();
        if (_playerController == null)
        {
            Debug.LogWarning("PlayerController not found. Player movement won't be disabled during pause.");
        }
        else
        {
            _playerInput = _playerController.GetComponent<PlayerInput>();
        }
    }

    private void OnEnable()
    {
        if (pauseActionReference != null)
        {
            pauseActionReference.action.Enable();
            pauseActionReference.action.performed += OnPauseAction;
        }
    }

    private void OnDisable()
    {
        if (pauseActionReference != null)
        {
            pauseActionReference.action.performed -= OnPauseAction;
        }
    }

    private void OnPauseAction(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    /// <summary>
    /// Toggle pause state. Call this from mobile pause button.
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        PlayClickSound();
        isPaused = true;

        // Set time scale BEFORE changing game state
        Time.timeScale = 0f;
        
        pauseMenuCanvas.SetActive(true);
        
        // Hide mobile controls during pause
        if (_mobileControlsManager != null)
        {
            _mobileControlsManager.HideMobileControls();
        }
        
        // Hide mobile pause button during pause menu
        if (mobilePauseButton != null)
        {
            mobilePauseButton.SetActive(false);
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeGameState(GameState.Paused);
        }

        // Disable player movement
        if (_playerController != null)
        {
            _playerController.enabled = false;
        }

        // Show cursor for UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        PlayClickSound();
        isPaused = false;

        pauseMenuCanvas.SetActive(false);
        
        // Reset time scale
        Time.timeScale = 1f;
        
        // Show mobile controls again (check for mobile platform OR editor with mobile enabled)
        bool isMobile = false;
        #if UNITY_IOS || UNITY_ANDROID
            isMobile = true;
        #endif
        
        // Also check if MobileControlsManager was enabled (for editor testing)
        if (_mobileControlsManager != null)
        {
            // If we have a mobile controls manager, show it regardless of platform
            // (assumes it was visible before pause if it exists)
            _mobileControlsManager.ShowMobileControls();
        }
        
        if (mobilePauseButton != null)
        {
            // Show pause button if on mobile OR if mobile controls manager exists (editor testing)
            mobilePauseButton.SetActive(isMobile || _mobileControlsManager != null);
        }
        
        // Handle cursor based on platform
        if (isMobile)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeGameState(GameState.Playing);
        }

        // Re-enable player
        if (_playerController != null)
        {
            _playerController.enabled = true;
        }
    }

    public void QuitToMainMenu()
    {
        PlayClickSound();
        ResetTimeScale();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    public void RestartGame()
    {
        PlayClickSound();
        ResetTimeScale();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeGameState(GameState.Playing);
        }
        
        SceneManager.LoadScene(gameSceneName);
    }

    public void BackToMainMenuFromWin()
    {
        PlayClickSound();
        ResetTimeScale();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    private void ResetTimeScale()
    {
        Time.timeScale = 1f;
        isPaused = false;
        
        // Re-enable player before scene change
        if (_playerController != null)
        {
            _playerController.enabled = true;
        }
    }

    public void PlayHoverSound()
    {
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    public void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}