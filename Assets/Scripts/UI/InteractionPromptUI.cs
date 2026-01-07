using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Simple interaction prompt UI that directly checks PlayerInteraction.
/// Supports keyboard, gamepad, and touch input icons.
/// </summary>
public class InteractionPromptUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteraction _playerInteraction;
    [SerializeField] private PlayerInput _playerInput;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject _promptContainer;
    [SerializeField] private TextMeshProUGUI _promptText;
    [SerializeField] private TextMeshProUGUI _keyText;
    [SerializeField] private Image _inputIcon;
    [SerializeField] private Image _holdProgressBar;

    [Header("Input Icons")]
    [SerializeField] private Sprite _keyboardIcon;
    [SerializeField] private Sprite _gamepadIcon;
    [SerializeField] private Sprite _touchIcon;

    [Header("Input Labels")]
    [SerializeField] private string _keyboardKey = "E";
    [SerializeField] private string _gamepadButton = "A";
    [SerializeField] private string _touchLabel = "TAP";

    [Header("Settings")]
    [SerializeField] private float _fadeSpeed = 8f;

    private CanvasGroup _canvasGroup;
    private bool _shouldShow;
    private string _currentControlScheme;

    private void Awake()
    {
        if (_promptContainer != null)
        {
            _canvasGroup = _promptContainer.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = _promptContainer.AddComponent<CanvasGroup>();
            }
            _canvasGroup.alpha = 0f;
        }

        if (_holdProgressBar != null)
        {
            _holdProgressBar.fillAmount = 0f;
        }
    }

    private void Start()
    {
        // Auto-find PlayerInteraction if not assigned
        if (_playerInteraction == null)
        {
            _playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }

        // Auto-find PlayerInput if not assigned
        if (_playerInput == null)
        {
            _playerInput = FindFirstObjectByType<PlayerInput>();
        }

        // Subscribe to control scheme changes
        if (_playerInput != null)
        {
            _playerInput.onControlsChanged += OnControlsChanged;
            UpdateInputDisplay(_playerInput.currentControlScheme);
        }
        else
        {
            // Default to keyboard
            UpdateInputDisplay("Keyboard&Mouse");
        }
    }

    private void OnDestroy()
    {
        if (_playerInput != null)
        {
            _playerInput.onControlsChanged -= OnControlsChanged;
        }
    }

    private void Update()
    {
        if (_playerInteraction == null || _canvasGroup == null) return;

        // Check if we have a valid interactable
        _shouldShow = _playerInteraction.HasInteractable;

        // Update prompt text
        if (_shouldShow && _playerInteraction.CurrentInteractable != null)
        {
            if (_promptText != null)
            {
                _promptText.text = _playerInteraction.CurrentInteractable.InteractionPrompt;
            }
        }

        // Fade in/out
        float targetAlpha = _shouldShow ? 1f : 0f;
        _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, targetAlpha, Time.deltaTime * _fadeSpeed);

        // Update hold progress bar
        UpdateHoldProgress();
    }

    private void OnControlsChanged(PlayerInput input)
    {
        UpdateInputDisplay(input.currentControlScheme);
    }

    private void UpdateInputDisplay(string controlScheme)
    {
        _currentControlScheme = controlScheme;

        switch (controlScheme)
        {
            case "Keyboard&Mouse":
                if (_keyText != null) _keyText.text = $"[{_keyboardKey}]";
                if (_inputIcon != null && _keyboardIcon != null)
                {
                    _inputIcon.sprite = _keyboardIcon;
                    _inputIcon.enabled = true;
                }
                break;

            case "Gamepad":
                if (_keyText != null) _keyText.text = $"[{_gamepadButton}]";
                if (_inputIcon != null && _gamepadIcon != null)
                {
                    _inputIcon.sprite = _gamepadIcon;
                    _inputIcon.enabled = true;
                }
                break;

            case "Touch":
                if (_keyText != null) _keyText.text = _touchLabel;
                if (_inputIcon != null && _touchIcon != null)
                {
                    _inputIcon.sprite = _touchIcon;
                    _inputIcon.enabled = true;
                }
                break;

            default:
                // Fallback to keyboard
                if (_keyText != null) _keyText.text = $"[{_keyboardKey}]";
                if (_inputIcon != null)
                {
                    if (_keyboardIcon != null)
                    {
                        _inputIcon.sprite = _keyboardIcon;
                        _inputIcon.enabled = true;
                    }
                    else
                    {
                        _inputIcon.enabled = false;
                    }
                }
                break;
        }
    }

    private void UpdateHoldProgress()
    {
        if (_holdProgressBar == null) return;
        if (_playerInteraction.CurrentInteractable == null)
        {
            _holdProgressBar.fillAmount = 0f;
            return;
        }

        if (_playerInteraction.CurrentInteractable.InteractionType != InteractionType.Hold)
        {
            _holdProgressBar.fillAmount = 0f;
        }
    }

    /// <summary>
    /// Call this from PlayerInteraction to update hold progress.
    /// </summary>
    public void SetHoldProgress(float progress)
    {
        if (_holdProgressBar != null)
        {
            _holdProgressBar.fillAmount = progress;
        }
    }
}