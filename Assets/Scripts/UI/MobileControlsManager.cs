using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages mobile controls visibility based on platform.
/// Attach to the Mobile Controls Canvas or a manager object.
/// </summary>
public class MobileControlsManager : MonoBehaviour
{
    [Header("Mobile UI")]
    [SerializeField] private GameObject _mobileControlsCanvas;
    [SerializeField] private MobileInputHandler _mobileInputHandler;
    
    [Header("Platform Settings")]
    [SerializeField] private bool _enableOnIOS = true;
    [SerializeField] private bool _enableOnAndroid = false;
    [SerializeField] private bool _enableInEditor = false;
    
    [Header("Auto-Detection")]
    [Tooltip("Automatically show mobile controls when touch input is detected")]
    [SerializeField] private bool _autoDetectTouch = false;

    public bool IsMobileControlsEnabled { get; private set; }

    private void Awake()
    {
        if (_mobileControlsCanvas == null)
        {
            _mobileControlsCanvas = gameObject;
        }

        if (_mobileInputHandler == null)
        {
            _mobileInputHandler = GetComponentInChildren<MobileInputHandler>(true);
        }

        IsMobileControlsEnabled = ShouldEnableMobileControls();
        
        UpdateMobileControlsVisibility();
    }

    private void Start()
    {
        // Double-check visibility after all systems are initialized
        UpdateMobileControlsVisibility();
        
        // Sync with PlayerController
        SyncWithPlayerController();
    }

    private void SyncWithPlayerController()
    {
        var playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null && IsMobileControlsEnabled)
        {
            // Use reflection or public method to set mobile input mode
            // For now, we rely on PlayerController finding us
            Debug.Log("[MobileControlsManager] Mobile controls enabled, PlayerController should use mobile input");
        }
    }

    private void UpdateMobileControlsVisibility()
    {
        IsMobileControlsEnabled = ShouldEnableMobileControls();
        
        if (_mobileControlsCanvas != null)
        {
            _mobileControlsCanvas.SetActive(IsMobileControlsEnabled);
        }

        // Keep MobileInputHandler active even if canvas is hidden
        // This allows the input system to work
        if (_mobileInputHandler != null)
        {
            _mobileInputHandler.gameObject.SetActive(IsMobileControlsEnabled);
        }

        // Also update cursor visibility for mobile
        if (IsMobileControlsEnabled)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        Debug.Log($"[MobileControlsManager] Mobile controls enabled: {IsMobileControlsEnabled}");
    }

    private bool ShouldEnableMobileControls()
    {
        #if UNITY_EDITOR
            if (_enableInEditor)
            {
                return true;
            }
            
            // In editor, check for touch simulation
            if (_autoDetectTouch && Touchscreen.current != null)
            {
                return true;
            }
            
            return false;
        #elif UNITY_IOS
            return _enableOnIOS;
        #elif UNITY_ANDROID
            return _enableOnAndroid;
        #else
            // Desktop/Console - check for touch if auto-detect is on
            if (_autoDetectTouch && Touchscreen.current != null)
            {
                return true;
            }
            return false;
        #endif
    }

    /// <summary>
    /// Manually show mobile controls.
    /// </summary>
    public void ShowMobileControls()
    {
        if (_mobileControlsCanvas != null)
        {
            _mobileControlsCanvas.SetActive(true);
        }
        if (_mobileInputHandler != null)
        {
            _mobileInputHandler.gameObject.SetActive(true);
        }
        IsMobileControlsEnabled = true;
    }

    /// <summary>
    /// Manually hide mobile controls.
    /// </summary>
    public void HideMobileControls()
    {
        if (_mobileControlsCanvas != null)
        {
            _mobileControlsCanvas.SetActive(false);
        }
        // Keep input handler active so it can still receive events
        // Just hide the visual UI
    }

    /// <summary>
    /// Toggle mobile controls visibility.
    /// </summary>
    public void ToggleMobileControls()
    {
        if (_mobileControlsCanvas != null)
        {
            bool newState = !_mobileControlsCanvas.activeSelf;
            _mobileControlsCanvas.SetActive(newState);
            IsMobileControlsEnabled = newState;
        }
    }

    /// <summary>
    /// Check if currently running on a mobile platform.
    /// </summary>
    public static bool IsMobilePlatform()
    {
        #if UNITY_IOS || UNITY_ANDROID
            return true;
        #else
            return false;
        #endif
    }

    /// <summary>
    /// Check if touch input is available.
    /// </summary>
    public static bool HasTouchInput()
    {
        return Touchscreen.current != null;
    }
}
