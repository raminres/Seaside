using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Idle,
    Walking,
    Running,
    Jumping,
    Falling,
    Swimming,
    Interacting,
    OnBoat
}

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 2f;
    [SerializeField] private float _runSpeed = 5.5f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _rotationSmoothTime = 0.12f;

    [Header("Jumping")]
    [SerializeField] private float _jumpHeight = 1.2f;
    [SerializeField] private float _gravity = -15f;
    [SerializeField] private float _jumpTimeout = 0.5f;
    [SerializeField] private float _fallTimeout = 0.15f;
    [SerializeField] private float _hardLandingVelocity = -10f;

    [Header("Ground Check")]
    [SerializeField] private float _groundedOffset = -0.14f;
    [SerializeField] private float _groundedRadius = 0.28f;
    [SerializeField] private LayerMask _groundLayers;

    [Header("Swimming")]
    [SerializeField] private float _swimSpeed = 3f;
    [SerializeField] private float _waterSurfaceY = 0f;
    [SerializeField] private bool _enableSwimming = false;

    [Header("Camera")]
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private float _topClamp = 70f;
    [SerializeField] private float _bottomClamp = -30f;
    [SerializeField] private float _cameraRotationSpeed = 1f;

    [Header("Audio & VFX")]
    [SerializeField] private PlayerAudioAndVfx _playerAudioAndVfx;

    [Header("Animation")]
    [SerializeField] private PlayerAnimation _playerAnimation;

    [Header("Mobile Input (Optional)")]
    [SerializeField] private MobileInputHandler _mobileInputHandler;
    [SerializeField] private bool _useMobileInput = false;

    // State
    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;
    public bool IsGrounded { get; private set; }
    public Vector3 Velocity => _velocity;

    // Components
    private CharacterController _controller;
    private PlayerInput _playerInput;
    private Camera _mainCamera;

    // Input values
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _sprintInput;
    private bool _jumpInput;

    // Internal state
    private Vector3 _velocity;
    private float _targetRotation;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = -53f;
    private float _lastVerticalVelocity;

    // Timers
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // Camera
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private const float Threshold = 0.01f;

    // Swimming state
    private bool _wasSwimming;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();
        _mainCamera = Camera.main;

        if (_mainCamera == null)
        {
            _mainCamera = FindFirstObjectByType<Camera>();
        }

        _jumpTimeoutDelta = _jumpTimeout;
        _fallTimeoutDelta = _fallTimeout;

        if (_cameraTarget != null)
        {
            _cinemachineTargetYaw = _cameraTarget.rotation.eulerAngles.y;
        }

        if (_playerAudioAndVfx == null)
        {
            _playerAudioAndVfx = GetComponentInChildren<PlayerAudioAndVfx>();
        }

        if (_playerAnimation == null)
        {
            _playerAnimation = GetComponentInChildren<PlayerAnimation>();
        }

        // Auto-detect mobile platform
        #if UNITY_IOS || UNITY_ANDROID
            _useMobileInput = true;
        #endif
    }

    private void Start()
    {
        // Check MobileControlsManager first to determine if we should use mobile input
        var mobileControlsManager = FindFirstObjectByType<MobileControlsManager>();
        if (mobileControlsManager != null && mobileControlsManager.IsMobileControlsEnabled)
        {
            _useMobileInput = true;
            Debug.Log("[PlayerController] MobileControlsManager found and enabled - using mobile input");
        }

        // Find mobile input handler (do this in Start to ensure it's initialized)
        if (_mobileInputHandler == null && _useMobileInput)
        {
            // First try to find active handler
            _mobileInputHandler = FindFirstObjectByType<MobileInputHandler>();
            
            // If not found, try to find inactive one (in case canvas was disabled)
            if (_mobileInputHandler == null)
            {
                _mobileInputHandler = FindFirstObjectByType<MobileInputHandler>(FindObjectsInactive.Include);
            }
            
            if (_mobileInputHandler == null)
            {
                Debug.LogWarning("[PlayerController] Mobile input enabled but MobileInputHandler not found!");
            }
            else
            {
                Debug.Log("[PlayerController] Found MobileInputHandler");
            }
        }

        // Initialize cursor state based on platform
        #if UNITY_IOS || UNITY_ANDROID
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        #else
            if (!_useMobileInput)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                // Mobile input in editor - keep cursor visible
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        #endif

        // Ensure input is active
        if (_playerInput != null)
        {
            _playerInput.ActivateInput();
            
            // Disable the Look AND Move actions when using mobile input
            // This prevents <Pointer>/delta from capturing all touch input
            if (_useMobileInput && _mobileInputHandler != null)
            {
                DisableStandardInputActions();
            }
        }
    }

    private void DisableStandardInputActions()
    {
        if (_playerInput == null) return;

        // Disable Look action - this is the main culprit for touch issues
        var lookAction = _playerInput.actions["Look"];
        if (lookAction != null)
        {
            lookAction.Disable();
            Debug.Log("[PlayerController] Disabled Look action for mobile input");
        }

        // Also disable Move action since we're using mobile joystick
        var moveAction = _playerInput.actions["Move"];
        if (moveAction != null)
        {
            moveAction.Disable();
            Debug.Log("[PlayerController] Disabled Move action for mobile input");
        }

        Debug.Log("[PlayerController] Mobile input mode active - standard input actions disabled");
    }

    private void OnEnable()
    {
        // Re-disable actions if we're re-enabled
        if (_useMobileInput && _playerInput != null)
        {
            DisableStandardInputActions();
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        // Process mobile input if available
        ProcessMobileInput();

        // Always do ground check
        GroundCheck();

        // Check if playing interaction animation
        bool isInteracting = (_playerAnimation != null && _playerAnimation.IsPlayingInteractionAnimation) 
                             || CurrentState == PlayerState.Interacting;

        if (isInteracting)
        {
            // Still apply gravity so player doesn't float
            if (!IsGrounded)
            {
                _verticalVelocity += _gravity * Time.deltaTime;
                _controller.Move(new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
            }
            else
            {
                _verticalVelocity = -2f;
            }
            return;
        }

        // Store velocity for landing detection
        _lastVerticalVelocity = _verticalVelocity;

        UpdateState();

        switch (CurrentState)
        {
            case PlayerState.Swimming:
                HandleSwimming();
                break;
            default:
                HandleGroundMovement();
                HandleJumpAndGravity();
                break;
        }
    }

    private void LateUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        // Camera always works, even during interactions
        HandleCameraRotation();
    }

    #region State Management

    private void UpdateState()
    {
        // Check for swimming first
        if (_enableSwimming && transform.position.y < _waterSurfaceY)
        {
            if (CurrentState != PlayerState.Swimming)
            {
                SetState(PlayerState.Swimming);
            }
            return;
        }

        // Exit swimming if above water
        if (CurrentState == PlayerState.Swimming && transform.position.y >= _waterSurfaceY)
        {
            SetState(IsGrounded ? PlayerState.Idle : PlayerState.Falling);
            return;
        }

        // Skip state updates during interaction or when on boat
        if (CurrentState == PlayerState.Interacting || CurrentState == PlayerState.OnBoat)
            return;

        // Ground-based state transitions
        if (IsGrounded)
        {
            Vector2 moveInput = GetCurrentMoveInput();
            bool sprintInput = GetCurrentSprintInput();
            
            if (moveInput.sqrMagnitude > Threshold)
            {
                SetState(sprintInput ? PlayerState.Running : PlayerState.Walking);
            }
            else
            {
                SetState(PlayerState.Idle);
            }
        }
        else
        {
            if (_verticalVelocity > 0)
            {
                SetState(PlayerState.Jumping);
            }
            else
            {
                SetState(PlayerState.Falling);
            }
        }
    }

    private void SetState(PlayerState newState)
    {
        if (CurrentState == newState) return;

        var previousState = CurrentState;
        CurrentState = newState;

        // State exit logic
        switch (previousState)
        {
            case PlayerState.Falling:
                if (newState != PlayerState.Jumping && newState != PlayerState.Swimming)
                {
                    // Determine if hard landing
                    bool hardLanding = _lastVerticalVelocity < _hardLandingVelocity;
                    _playerAudioAndVfx?.PlayLandSound(hardLanding);
                }
                break;

            case PlayerState.Swimming:
                if (!_wasSwimming) break;
                _playerAudioAndVfx?.PlayExitWaterSound();
                _wasSwimming = false;
                break;
        }

        // State enter logic
        switch (newState)
        {
            case PlayerState.Jumping:
                _playerAudioAndVfx?.PlayJumpSound();
                break;

            case PlayerState.Swimming:
                if (!_wasSwimming)
                {
                    _playerAudioAndVfx?.PlayEnterWaterSound();
                    _wasSwimming = true;
                }
                break;
        }
    }

    /// <summary>
    /// Call this when starting an interaction.
    /// </summary>
    public void SetInteracting(bool isInteracting)
    {
        if (isInteracting)
        {
            SetState(PlayerState.Interacting);
        }
        else
        {
            SetState(IsGrounded ? PlayerState.Idle : PlayerState.Falling);
        }
    }

    #endregion

    #region Movement

    private void HandleGroundMovement()
    {
        // Get the correct move input based on mode
        Vector2 moveInput = GetCurrentMoveInput();
        bool sprintInput = GetCurrentSprintInput();
        
        float targetSpeed = sprintInput ? _runSpeed : _walkSpeed;
        if (moveInput == Vector2.zero) targetSpeed = 0f;

        float currentHorizontalSpeed = new Vector3(_velocity.x, 0f, _velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = moveInput.magnitude;

        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            float newSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * _acceleration);
            newSpeed = Mathf.Round(newSpeed * 1000f) / 1000f;
            currentHorizontalSpeed = newSpeed;
        }
        else
        {
            currentHorizontalSpeed = targetSpeed;
        }

        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        if (moveInput != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation,
                ref _rotationVelocity, _rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward;

        _velocity = targetDirection.normalized * currentHorizontalSpeed + new Vector3(0f, _verticalVelocity, 0f);
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void HandleSwimming()
    {
        // Get the correct move input based on mode
        Vector2 moveInput = GetCurrentMoveInput();
        
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        if (inputDirection != Vector3.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation,
                ref _rotationVelocity, _rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        }

        Vector3 moveDirection = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward;

        float targetY = _waterSurfaceY - 0.5f;
        float verticalMove = (targetY - transform.position.y) * 2f;

        _velocity = moveDirection * _swimSpeed * moveInput.magnitude;
        _velocity.y = verticalMove;

        _controller.Move(_velocity * Time.deltaTime);
    }

    #endregion

    #region Input Helpers
    
    private Vector2 GetCurrentMoveInput()
    {
        if (_useMobileInput)
        {
            // Try to find handler if not yet found
            if (_mobileInputHandler == null)
            {
                _mobileInputHandler = FindFirstObjectByType<MobileInputHandler>();
            }
            
            if (_mobileInputHandler != null)
            {
                return _mobileInputHandler.MoveInput;
            }
        }
        return _moveInput;
    }

    private Vector2 GetCurrentLookInput()
    {
        if (_useMobileInput)
        {
            // Try to find handler if not yet found
            if (_mobileInputHandler == null)
            {
                _mobileInputHandler = FindFirstObjectByType<MobileInputHandler>();
            }
            
            if (_mobileInputHandler != null)
            {
                return _mobileInputHandler.LookInput;
            }
        }
        return _lookInput;
    }

    private bool GetCurrentSprintInput()
    {
        if (_useMobileInput)
        {
            // Try to find handler if not yet found
            if (_mobileInputHandler == null)
            {
                _mobileInputHandler = FindFirstObjectByType<MobileInputHandler>();
            }
            
            if (_mobileInputHandler != null)
            {
                return _mobileInputHandler.SprintInput;
            }
        }
        return _sprintInput;
    }

    #endregion

    #region Jump & Gravity

    private void HandleJumpAndGravity()
    {
        if (IsGrounded)
        {
            _fallTimeoutDelta = _fallTimeout;

            if (_verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            if (_jumpInput && _jumpTimeoutDelta <= 0f)
            {
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            }

            if (_jumpTimeoutDelta >= 0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            _jumpTimeoutDelta = _jumpTimeout;

            if (_fallTimeoutDelta >= 0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }

            _jumpInput = false;
        }

        if (_verticalVelocity > _terminalVelocity)
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }
    }

    #endregion

    #region Ground Check

    private void GroundCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _groundedOffset, transform.position.z);
        IsGrounded = Physics.CheckSphere(spherePosition, _groundedRadius, _groundLayers, QueryTriggerInteraction.Ignore);
    }

    #endregion

    #region Camera

    private void HandleCameraRotation()
    {
        if (_cameraTarget == null) return;
        
        // Get the correct look input based on mode
        Vector2 lookInput;
        
        if (_useMobileInput && _mobileInputHandler != null)
        {
            // Mobile mode - get input directly from mobile handler
            lookInput = _mobileInputHandler.LookInput;
        }
        else
        {
            // Standard mode - use the cached input from callbacks
            lookInput = _lookInput;
        }
        
        if (lookInput.sqrMagnitude < Threshold) return;

        bool isCurrentDeviceMouse = !_useMobileInput && _playerInput != null && _playerInput.currentControlScheme == "Keyboard&Mouse";
        float deltaTimeMultiplier = isCurrentDeviceMouse ? 1f : Time.deltaTime;

        _cinemachineTargetYaw += lookInput.x * _cameraRotationSpeed * deltaTimeMultiplier;
        _cinemachineTargetPitch += lookInput.y * _cameraRotationSpeed * deltaTimeMultiplier;

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);

        _cameraTarget.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0f);
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    #endregion

    #region Interaction

    /// <summary>
    /// Play grab animation and optionally rotate toward target.
    /// </summary>
    public void PlayGrabInteraction(Transform target = null, System.Action onComplete = null)
    {
        if (target != null)
        {
            RotateToward(target.position);
        }

        SetInteracting(true);
        _playerAnimation?.PlayGrabAnimation(() =>
        {
            SetInteracting(false);
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// Play fire lighting animation.
    /// </summary>
    public void PlayLightFireInteraction(Transform target = null, System.Action onComplete = null)
    {
        if (target != null)
        {
            RotateToward(target.position);
        }

        SetInteracting(true);
        _playerAnimation?.PlayLightFireAnimation(() =>
        {
            SetInteracting(false);
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// Play generic interaction animation.
    /// </summary>
    public void PlayInteraction(Transform target = null, System.Action onComplete = null)
    {
        if (target != null)
        {
            RotateToward(target.position);
        }

        SetInteracting(true);
        _playerAnimation?.PlayInteractAnimation(() =>
        {
            SetInteracting(false);
            onComplete?.Invoke();
        });
    }

    private void RotateToward(Vector3 position)
    {
        Vector3 direction = (position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    #endregion

    #region Boat Interaction

    /// <summary>
    /// Call this when the boat arrival sequence starts.
    /// Locks the state machine but allows walking and looking.
    /// </summary>
    public void BoardBoat(Transform standPoint, Transform boatRoot)
    {
        // Disable temporarily to teleport safely
        _controller.enabled = false;
        
        transform.position = standPoint.position;
        transform.rotation = standPoint.rotation;
        _targetRotation = transform.eulerAngles.y;
        
        // Ensure camera also snaps so it doesn't try to interpolate from shore to boat
        if (_cameraTarget != null)
        {
            _cinemachineTargetYaw = _cameraTarget.eulerAngles.y;
            _cinemachineTargetPitch = _cameraTarget.eulerAngles.x;
        }

        // Parent to boat to inherit general transform position (though we use ApplyExternalMovement for precision)
        transform.SetParent(boatRoot);
        
        _controller.enabled = true;
        
        SetState(PlayerState.OnBoat);
    }

    /// <summary>
    /// Call this when disembarking. Unparents and restores normal state.
    /// </summary>
    public void DisembarkBoat(Transform disembarkPoint)
    {
        _controller.enabled = false;
        
        transform.position = disembarkPoint.position;
        transform.rotation = disembarkPoint.rotation;
        _targetRotation = transform.eulerAngles.y;
        
        if (_cameraTarget != null)
        {
            _cinemachineTargetYaw = _cameraTarget.eulerAngles.y;
            _cinemachineTargetPitch = _cameraTarget.eulerAngles.x;
        }

        transform.SetParent(null);
        
        _controller.enabled = true;
        
        SetState(PlayerState.Idle);
    }

    /// <summary>
    /// Applies delta movement from a moving platform (the boat).
    /// Called by BoatArrivalController every frame while approaching.
    /// </summary>
    public void ApplyExternalMovement(Vector3 deltaPosition, float deltaYaw)
    {
        if (_controller.enabled)
        {
            // Move physics directly without overriding gravity/walking
            _controller.Move(deltaPosition);
        }

        // Manually adjust the camera target so look controls don't drift as the boat turns
        if (Mathf.Abs(deltaYaw) > 0.001f)
        {
            _cinemachineTargetYaw += deltaYaw;
            _targetRotation += deltaYaw;
        }
    }

    #endregion

    #region Input Callbacks

    public void OnMove(InputValue value)
    {
        // Ignore standard input when using mobile
        if (_useMobileInput && _mobileInputHandler != null) return;
        
        _moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        // Ignore standard input when using mobile
        if (_useMobileInput && _mobileInputHandler != null) return;
        
        _lookInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        // Ignore standard input when using mobile
        if (_useMobileInput && _mobileInputHandler != null) return;
        
        _sprintInput = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        // Ignore standard input when using mobile
        if (_useMobileInput && _mobileInputHandler != null) return;
        
        _jumpInput = value.isPressed;
    }

    #endregion

    #region Mobile Input

    private void ProcessMobileInput()
    {
        if (!_useMobileInput || _mobileInputHandler == null) return;

        // Get mobile inputs directly (replaces standard input entirely)
        _moveInput = _mobileInputHandler.MoveInput;
        _lookInput = _mobileInputHandler.LookInput;
        _sprintInput = _mobileInputHandler.SprintInput;

        // Debug - uncomment to see values every frame
        // if (_moveInput.sqrMagnitude > 0.01f)
        //     Debug.Log($"[PlayerController] MoveInput from mobile: {_moveInput}");
        // if (_lookInput.sqrMagnitude > 0.01f)
        //     Debug.Log($"[PlayerController] LookInput from mobile: {_lookInput}");

        // Jump from mobile (consume after use)
        if (_mobileInputHandler.JumpInput)
        {
            _jumpInput = true;
            _mobileInputHandler.ConsumeJump();
        }
    }

    /// <summary>
    /// Set move input directly (for mobile joystick).
    /// </summary>
    public void SetMoveInput(Vector2 input)
    {
        _moveInput = input;
    }

    /// <summary>
    /// Set look input directly (for mobile joystick).
    /// </summary>
    public void SetLookInput(Vector2 input)
    {
        _lookInput = input;
    }

    /// <summary>
    /// Set sprint state directly (for mobile button).
    /// </summary>
    public void SetSprint(bool isSprinting)
    {
        _sprintInput = isSprinting;
    }

    /// <summary>
    /// Trigger jump (for mobile button).
    /// </summary>
    public void TriggerJump()
    {
        _jumpInput = true;
    }

    #endregion

    #region Editor Gizmos

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0f, 1f, 0f, 0.35f);
        Color transparentRed = new Color(1f, 0f, 0f, 0.35f);

        Gizmos.color = IsGrounded ? transparentGreen : transparentRed;

        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _groundedOffset, transform.position.z);
        Gizmos.DrawSphere(spherePosition, _groundedRadius);
    }

    #endregion
}
