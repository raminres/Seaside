using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Bridges mobile UI controls (virtual joysticks, buttons) to game systems.
/// Uses frame-based input tracking to prevent input conflicts between systems.
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
    
    // Interact button state
    private bool _interactButtonDown;      // Physical button state
    private bool _interactPressedThisFrame; // True only on the frame button was pressed
    private int _interactPressFrame = -1;   // Frame number when pressed

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

    private void LateUpdate()
    {
        // Clear the "pressed this frame" flag at end of frame
        // This ensures all systems have a chance to see it during Update
        if (_interactPressedThisFrame && Time.frameCount > _interactPressFrame)
        {
            _interactPressedThisFrame = false;
        }
    }

    #region Joystick Callbacks

    public void OnMoveJoystick(Vector2 value)
    {
        _moveInput = value;
        
        if (value.sqrMagnitude < 0.01f)
        {
            _sprintInput = false;
        }
        
        if (_debugLog && value.sqrMagnitude > 0.01f)
        {
            Debug.Log($"[MobileInput] Move: {value}");
        }
    }

    public void OnLookJoystick(Vector2 value)
    {
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

    #region Button Callbacks

    public void OnJumpPressed()
    {
        _jumpInput = true;
        if (_debugLog) Debug.Log("[MobileInput] Jump Pressed");
    }

    public void OnJumpReleased()
    {
        _jumpInput = false;
        if (_debugLog) Debug.Log("[MobileInput] Jump Released");
    }

    public void OnSprintPressed()
    {
        _sprintInput = true;
        if (_debugLog) Debug.Log("[MobileInput] Sprint Pressed");
    }

    public void OnSprintReleased()
    {
        _sprintInput = false;
        if (_debugLog) Debug.Log("[MobileInput] Sprint Released");
    }

    public void OnSprintToggle()
    {
        _sprintInput = !_sprintInput;
        if (_debugLog) Debug.Log($"[MobileInput] Sprint Toggled: {_sprintInput}");
    }

    public void OnInteractPressed()
    {
        _interactButtonDown = true;
        _interactPressedThisFrame = true;
        _interactPressFrame = Time.frameCount;
        
        if (_debugLog) Debug.Log($"[MobileInput] Interact Pressed (frame {Time.frameCount})");
    }

    public void OnInteractReleased()
    {
        _interactButtonDown = false;
        
        if (_debugLog) Debug.Log($"[MobileInput] Interact Released (frame {Time.frameCount})");
    }

    #endregion

    #region Input Getters

    public Vector2 MoveInput => _moveInput;
    public Vector2 LookInput => _lookInput;
    public bool JumpInput => _jumpInput;
    public bool SprintInput => _sprintInput;
    
    /// <summary>
    /// True if the interact button is currently held down.
    /// </summary>
    public bool InteractButtonDown => _interactButtonDown;
    
    /// <summary>
    /// True only on the frame the interact button was pressed.
    /// Multiple systems can check this in the same frame.
    /// </summary>
    public bool InteractPressedThisFrame => _interactPressedThisFrame;
    
    /// <summary>
    /// Legacy property for backward compatibility.
    /// Returns true if button is down OR was pressed this frame.
    /// </summary>
    public bool InteractInput => _interactButtonDown || _interactPressedThisFrame;
    
    // Legacy properties for compatibility
    public bool InteractHeld => _interactButtonDown;
    public bool InteractTriggered => _interactPressedThisFrame;

    public bool HasMoveInput => _moveInput.sqrMagnitude > 0.01f;
    public bool HasLookInput => _lookInput.sqrMagnitude > 0.01f;

    public void ConsumeJump()
    {
        _jumpInput = false;
    }

    /// <summary>
    /// Legacy method - no longer needed but kept for compatibility.
    /// The input auto-clears at end of frame via LateUpdate.
    /// </summary>
    public void ConsumeInteract()
    {
        // No-op - input clears automatically in LateUpdate
        // This prevents systems from interfering with each other
    }

    public void ResetAllInputs()
    {
        _moveInput = Vector2.zero;
        _lookInput = Vector2.zero;
        _jumpInput = false;
        _sprintInput = false;
        _interactButtonDown = false;
        _interactPressedThisFrame = false;
        
        if (_debugLog) Debug.Log("[MobileInput] All inputs reset");
    }

    public void ResetMoveInput()
    {
        _moveInput = Vector2.zero;
    }

    public void ResetLookInput()
    {
        _lookInput = Vector2.zero;
    }

    #endregion

    private void OnDisable()
    {
        ResetAllInputs();
    }
}
