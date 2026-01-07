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
    Interacting
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

    [Header("Events")]
    [SerializeField] private GameEventSo _onJump;
    [SerializeField] private GameEventSo _onLand;

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

    // Timers
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // Camera
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private const float Threshold = 0.01f;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();
        _mainCamera = Camera.main;

        _jumpTimeoutDelta = _jumpTimeout;
        _fallTimeoutDelta = _fallTimeout;

        if (_cameraTarget != null)
        {
            _cinemachineTargetYaw = _cameraTarget.rotation.eulerAngles.y;
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        GroundCheck();
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

        // Skip state updates during interaction
        if (CurrentState == PlayerState.Interacting)
            return;

        // Ground-based state transitions
        if (IsGrounded)
        {
            if (_moveInput.sqrMagnitude > Threshold)
            {
                SetState(_sprintInput ? PlayerState.Running : PlayerState.Walking);
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
                    _onLand?.RaiseEvent();
                }
                break;
        }

        // State enter logic
        switch (newState)
        {
            case PlayerState.Jumping:
                _onJump?.RaiseEvent();
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
        float targetSpeed = _sprintInput ? _runSpeed : _walkSpeed;
        if (_moveInput == Vector2.zero) targetSpeed = 0f;

        float currentHorizontalSpeed = new Vector3(_velocity.x, 0f, _velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _moveInput.magnitude;

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

        Vector3 inputDirection = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;

        if (_moveInput != Vector2.zero)
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
        Vector3 inputDirection = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;
        
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

        _velocity = moveDirection * _swimSpeed * _moveInput.magnitude;
        _velocity.y = verticalMove;

        _controller.Move(_velocity * Time.deltaTime);
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
        if (_lookInput.sqrMagnitude < Threshold) return;

        bool isCurrentDeviceMouse = _playerInput.currentControlScheme == "Keyboard&Mouse";
        float deltaTimeMultiplier = isCurrentDeviceMouse ? 1f : Time.deltaTime;

        _cinemachineTargetYaw += _lookInput.x * _cameraRotationSpeed * deltaTimeMultiplier;
        _cinemachineTargetPitch += _lookInput.y * _cameraRotationSpeed * deltaTimeMultiplier;

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

    #region Input Callbacks

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        _lookInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        _sprintInput = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        _jumpInput = value.isPressed;
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
