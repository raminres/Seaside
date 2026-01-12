using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Bridges mobile UI controls (virtual joysticks, buttons) to PlayerController.
/// Attach to a manager object or the Mobile Controls Canvas.
/// </summary>
public class MobileInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput _playerInput;
    
    [Header("Input Action Names")]
    [SerializeField] private string _moveActionName = "Move";
    [SerializeField] private string _lookActionName = "Look";
    [SerializeField] private string _jumpActionName = "Jump";
    [SerializeField] private string _sprintActionName = "Sprint";

    [Header("Look Sensitivity")]
    [SerializeField] private float _lookSensitivity = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool _debugLog = false;

    // Cached input actions
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;

    // Current input values (set by UI)
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _jumpInput;
    private bool _sprintInput;
    private bool _interactInput;

    private void Awake()
    {
        if (_playerInput == null)
        {
            _playerInput = FindFirstObjectByType<PlayerInput>();
        }
    }

    private void Start()
    {
        // Cache input actions
        if (_playerInput != null)
        {
            _moveAction = _playerInput.actions[_moveActionName];
            _lookAction = _playerInput.actions[_lookActionName];
            _jumpAction = _playerInput.actions[_jumpActionName];
            _sprintAction = _playerInput.actions[_sprintActionName];
        }
    }

    #region Joystick Callbacks (Connect to UIVirtualJoystick.joystickOutputEvent)

    /// <summary>
    /// Called by Move Joystick's joystickOutputEvent.
    /// When joystick is released, UIVirtualJoystick sends Vector2.zero.
    /// </summary>
    public void OnMoveJoystick(Vector2 value)
    {
        _moveInput = value;
        
        if (_debugLog && value.sqrMagnitude > 0.01f)
        {
            Debug.Log($"[MobileInput] Move: {value}");
        }
    }

    /// <summary>
    /// Called by Look Joystick's joystickOutputEvent.
    /// When joystick is released, UIVirtualJoystick sends Vector2.zero.
    /// </summary>
    public void OnLookJoystick(Vector2 value)
    {
        // Only apply sensitivity when there's actual input
        // This ensures zero stays zero
        if (value.sqrMagnitude > 0.001f)
        {
            _lookInput = value * _lookSensitivity;
        }
        else
        {
            _lookInput = Vector2.zero;
        }
        
        if (_debugLog && value.sqrMagnitude > 0.01f)
        {
            Debug.Log($"[MobileInput] Look: {value}");
        }
    }

    #endregion

    #region Button Callbacks (Connect to Button OnClick or use EventTrigger)

    /// <summary>
    /// Called when Jump button is pressed.
    /// </summary>
    public void OnJumpPressed()
    {
        _jumpInput = true;
        if (_debugLog) Debug.Log("[MobileInput] Jump Pressed");
    }

    /// <summary>
    /// Called when Jump button is released.
    /// </summary>
    public void OnJumpReleased()
    {
        _jumpInput = false;
        if (_debugLog) Debug.Log("[MobileInput] Jump Released");
    }

    /// <summary>
    /// Called when Sprint button is pressed (hold).
    /// </summary>
    public void OnSprintPressed()
    {
        _sprintInput = true;
        if (_debugLog) Debug.Log("[MobileInput] Sprint Pressed");
    }

    /// <summary>
    /// Called when Sprint button is released.
    /// </summary>
    public void OnSprintReleased()
    {
        _sprintInput = false;
        if (_debugLog) Debug.Log("[MobileInput] Sprint Released");
    }

    /// <summary>
    /// Toggle sprint on/off (alternative for tap-to-toggle sprint).
    /// </summary>
    public void OnSprintToggle()
    {
        _sprintInput = !_sprintInput;
        if (_debugLog) Debug.Log($"[MobileInput] Sprint Toggled: {_sprintInput}");
    }

    /// <summary>
    /// Called when Interact button is pressed.
    /// </summary>
    public void OnInteractPressed()
    {
        _interactInput = true;
        if (_debugLog) Debug.Log("[MobileInput] Interact Pressed");
    }

    /// <summary>
    /// Called when Interact button is released.
    /// </summary>
    public void OnInteractReleased()
    {
        _interactInput = false;
        if (_debugLog) Debug.Log("[MobileInput] Interact Released");
    }

    #endregion

    #region Input Value Getters (For PlayerController if using direct reference)

    public Vector2 MoveInput => _moveInput;
    public Vector2 LookInput => _lookInput;
    public bool JumpInput => _jumpInput;
    public bool SprintInput => _sprintInput;
    public bool InteractInput => _interactInput;

    /// <summary>
    /// Check if any mobile input is active.
    /// </summary>
    public bool HasMoveInput => _moveInput.sqrMagnitude > 0.01f;
    public bool HasLookInput => _lookInput.sqrMagnitude > 0.01f;

    /// <summary>
    /// Consume jump input (call after processing jump).
    /// </summary>
    public void ConsumeJump()
    {
        _jumpInput = false;
    }

    /// <summary>
    /// Consume interact input (call after processing interact).
    /// </summary>
    public void ConsumeInteract()
    {
        _interactInput = false;
    }

    /// <summary>
    /// Reset all inputs (call when disabling mobile controls).
    /// </summary>
    public void ResetAllInputs()
    {
        _moveInput = Vector2.zero;
        _lookInput = Vector2.zero;
        _jumpInput = false;
        _sprintInput = false;
        _interactInput = false;
        
        if (_debugLog) Debug.Log("[MobileInput] All inputs reset");
    }

    /// <summary>
    /// Reset movement input only.
    /// </summary>
    public void ResetMoveInput()
    {
        _moveInput = Vector2.zero;
    }

    /// <summary>
    /// Reset look input only.
    /// </summary>
    public void ResetLookInput()
    {
        _lookInput = Vector2.zero;
    }

    #endregion

    private void OnDisable()
    {
        // Reset all inputs when disabled to prevent stuck inputs
        ResetAllInputs();
    }
}
