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
    
    [Header("Platform Settings")]
    [SerializeField] private bool _enableOnIOS = true;
    [SerializeField] private bool _enableOnAndroid = false;
    [SerializeField] private bool _enableInEditor = false;
    
    [Header("Auto-Detection")]
    [Tooltip("Automatically show mobile controls when touch input is detected")]
    [SerializeField] private bool _autoDetectTouch = false;

    private void Awake()
    {
        if (_mobileControlsCanvas == null)
        {
            _mobileControlsCanvas = gameObject;
        }

        UpdateMobileControlsVisibility();
    }

    private void Start()
    {
        // Double-check visibility after all systems are initialized
        UpdateMobileControlsVisibility();
    }

    private void UpdateMobileControlsVisibility()
    {
        bool shouldEnable = ShouldEnableMobileControls();
        
        if (_mobileControlsCanvas != null)
        {
            _mobileControlsCanvas.SetActive(shouldEnable);
        }

        // Also update cursor visibility for mobile
        if (shouldEnable)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
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
    }

    /// <summary>
    /// Toggle mobile controls visibility.
    /// </summary>
    public void ToggleMobileControls()
    {
        if (_mobileControlsCanvas != null)
        {
            _mobileControlsCanvas.SetActive(!_mobileControlsCanvas.activeSelf);
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
