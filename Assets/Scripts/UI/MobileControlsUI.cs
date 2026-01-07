using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages mobile touch controls visibility based on input device.
/// </summary>
public class MobileControlsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _mobileControlsContainer;
    [SerializeField] private PlayerInput _playerInput;

    [Header("Settings")]
    [SerializeField] private bool _showOnTouch = true;
    [SerializeField] private bool _showOnGamepad = false;
    [SerializeField] private bool _showOnKeyboardMouse = false;

    private void Start()
    {
        if (_playerInput == null)
        {
            _playerInput = FindFirstObjectByType<PlayerInput>();
        }

        if (_playerInput != null)
        {
            _playerInput.onControlsChanged += OnControlsChanged;
            OnControlsChanged(_playerInput);
        }

#if UNITY_IOS || UNITY_ANDROID
        if (_mobileControlsContainer != null && _playerInput == null)
        {
            _mobileControlsContainer.SetActive(true);
        }
#endif
    }

    private void OnDestroy()
    {
        if (_playerInput != null)
        {
            _playerInput.onControlsChanged -= OnControlsChanged;
        }
    }

    private void OnControlsChanged(PlayerInput input)
    {
        if (_mobileControlsContainer == null) return;

        bool shouldShow = input.currentControlScheme switch
        {
            "Touch" => _showOnTouch,
            "Gamepad" => _showOnGamepad,
            "Keyboard&Mouse" => _showOnKeyboardMouse,
            _ => false
        };

        _mobileControlsContainer.SetActive(shouldShow);
    }

    public void ShowMobileControls()
    {
        if (_mobileControlsContainer != null)
        {
            _mobileControlsContainer.SetActive(true);
        }
    }

    public void HideMobileControls()
    {
        if (_mobileControlsContainer != null)
        {
            _mobileControlsContainer.SetActive(false);
        }
    }
}
