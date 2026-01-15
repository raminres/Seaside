using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// A place where the player can sit (bench, chair, campfire spot, etc.).
/// Moves and rotates player to the correct position when sitting.
/// </summary>
public class SeatInteractable : InteractableBase
{
    [Header("Seat Settings")]
    [SerializeField] private Transform _sitPoint;
    [SerializeField] private float _moveToSeatDuration = 0.3f;
    [SerializeField] private bool _canLookAroundWhileSitting = true;

    [Header("Prompts")]
    [SerializeField] private string _sitPrompt = "Sit";
    [SerializeField] private string _standPrompt = "Stand";

    [Header("Animation")]
    [SerializeField] private string _sitAnimTrigger = "Sit";
    [SerializeField] private string _standAnimTrigger = "Stand";
    [SerializeField] private string _isSittingBool = "IsSitting";

    [Header("Audio")]
    [SerializeField] private AudioClip _sitSound;
    [SerializeField] private AudioClip _standSound;

    [Header("Events")]
    [SerializeField] private UnityEvent _onPlayerSat;
    [SerializeField] private UnityEvent _onPlayerStood;

    [Header("Debug")]
    [SerializeField] private bool _debugLog = false;

    private bool _isOccupied;
    private PlayerController _seatedPlayer;
    private PlayerAnimation _seatedPlayerAnimation;
    private CharacterController _playerCharacterController;
    private PlayerInput _playerInput;
    private InputAction _interactAction;
    private MobileInputHandler _mobileInputHandler;
    private bool _wasInteractPressedLastFrame;
    
    private Vector3 _originalPlayerPosition;
    private Quaternion _originalPlayerRotation;
    
    private bool _isTransitioning;
    private float _transitionProgress;
    private Vector3 _transitionStartPos;
    private Quaternion _transitionStartRot;

    public bool IsOccupied => _isOccupied;

    private void Awake()
    {
        _interactionType = InteractionType.Toggle;

        // Create sit point if not assigned
        if (_sitPoint == null)
        {
            GameObject sitPointObj = new GameObject("SitPoint");
            sitPointObj.transform.SetParent(transform);
            sitPointObj.transform.localPosition = Vector3.zero;
            sitPointObj.transform.localRotation = Quaternion.identity;
            _sitPoint = sitPointObj.transform;
        }
    }

    private void Update()
    {
        // Handle stand up input while seated
        if (_isOccupied && !_isTransitioning)
        {
            bool interactPressed = false;
            
            // Check standard input action
            if (_interactAction != null)
            {
                try
                {
                    if (_interactAction.WasPressedThisFrame())
                    {
                        interactPressed = true;
                        if (_debugLog) Debug.Log("[SeatInteractable] Stand up triggered by keyboard/gamepad");
                    }
                }
                catch (System.Exception)
                {
                    // Action might be disabled, ignore
                }
            }
            
            // Check mobile input - detect rising edge (was not pressed, now is pressed)
            if (_mobileInputHandler != null)
            {
                bool currentlyPressed = _mobileInputHandler.InteractInput;
                
                if (_debugLog && currentlyPressed)
                {
                    Debug.Log($"[SeatInteractable] Mobile interact: current={currentlyPressed}, lastFrame={_wasInteractPressedLastFrame}");
                }
                
                if (currentlyPressed && !_wasInteractPressedLastFrame)
                {
                    interactPressed = true;
                    if (_debugLog) Debug.Log("[SeatInteractable] Stand up triggered by mobile button");
                }
                _wasInteractPressedLastFrame = currentlyPressed;
            }
            
            if (interactPressed)
            {
                if (_debugLog) Debug.Log("[SeatInteractable] Starting stand up sequence");
                StartCoroutine(StandUpSequence());
            }
        }
    }

    private void OnDisable()
    {
        // Clean up if disabled while player is seated
        if (_isOccupied)
        {
            ForceReleasePlayer();
        }
    }

    public override string InteractionPrompt => _isOccupied ? _standPrompt : _sitPrompt;

    public override bool CanInteract
    {
        get
        {
            // Can't interact if transitioning
            if (_isTransitioning) return false;
            
            // Can sit if not occupied
            if (!_isOccupied) return base.CanInteract;
            
            // Can't use normal interaction while occupied (stand up is handled in Update)
            return false;
        }
    }

    protected override void OnInteractInternal(PlayerController player)
    {
        if (_isTransitioning) return;

        if (!_isOccupied)
        {
            if (_debugLog) Debug.Log("[SeatInteractable] Player sitting down");
            StartCoroutine(SitDownSequence(player));
        }
    }

    private System.Collections.IEnumerator SitDownSequence(PlayerController player)
    {
        _isTransitioning = true;
        _isOccupied = true;
        _seatedPlayer = player;
        _seatedPlayerAnimation = player.GetComponentInChildren<PlayerAnimation>();
        _playerCharacterController = player.GetComponent<CharacterController>();
        
        // Get input reference for stand up detection
        _playerInput = player.GetComponent<PlayerInput>();
        if (_playerInput != null)
        {
            _interactAction = _playerInput.actions["Interact"];
            if (_debugLog) Debug.Log($"[SeatInteractable] Got interact action: {_interactAction != null}");
        }
        
        // Get mobile input handler for stand up detection
        _mobileInputHandler = FindFirstObjectByType<MobileInputHandler>();
        if (_mobileInputHandler != null)
        {
            // Consume any existing interact input to prevent immediate stand-up
            _mobileInputHandler.ConsumeInteract();
            _wasInteractPressedLastFrame = true; // Prevent immediate trigger
            if (_debugLog) Debug.Log("[SeatInteractable] Found MobileInputHandler, consumed existing input");
        }

        // Store original position/rotation for standing up
        _originalPlayerPosition = player.transform.position;
        _originalPlayerRotation = player.transform.rotation;

        // Disable character controller to allow manual positioning
        if (_playerCharacterController != null)
        {
            _playerCharacterController.enabled = false;
        }

        // Set player to interacting state
        player.SetInteracting(true);

        // Play sit sound
        if (_sitSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_sitSound);
        }

        // Smoothly move player to sit position
        _transitionStartPos = player.transform.position;
        _transitionStartRot = player.transform.rotation;
        _transitionProgress = 0f;

        while (_transitionProgress < 1f)
        {
            _transitionProgress += Time.deltaTime / _moveToSeatDuration;
            float t = Mathf.SmoothStep(0f, 1f, _transitionProgress);

            player.transform.position = Vector3.Lerp(_transitionStartPos, _sitPoint.position, t);
            player.transform.rotation = Quaternion.Slerp(_transitionStartRot, _sitPoint.rotation, t);

            yield return null;
        }

        // Snap to exact position
        player.transform.position = _sitPoint.position;
        player.transform.rotation = _sitPoint.rotation;

        // Trigger sit animation
        if (_seatedPlayerAnimation != null)
        {
            _seatedPlayerAnimation.SetTrigger(_sitAnimTrigger);
            _seatedPlayerAnimation.SetBool(_isSittingBool, true);
        }

        _isTransitioning = false;
        
        // Reset mobile input state after a short delay to allow for input to settle
        yield return null;
        _wasInteractPressedLastFrame = _mobileInputHandler != null && _mobileInputHandler.InteractInput;
        
        _onPlayerSat?.Invoke();
        
        if (_debugLog) Debug.Log("[SeatInteractable] Sit down complete, ready for stand up input");
    }

    private System.Collections.IEnumerator StandUpSequence()
    {
        if (_seatedPlayer == null) yield break;

        _isTransitioning = true;

        // Store reference before clearing
        PlayerController player = _seatedPlayer;
        
        if (_debugLog) Debug.Log("[SeatInteractable] Standing up...");
        
        // Play stand sound
        if (_standSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_standSound);
        }

        // Trigger stand animation
        if (_seatedPlayerAnimation != null)
        {
            _seatedPlayerAnimation.SetTrigger(_standAnimTrigger);
            _seatedPlayerAnimation.SetBool(_isSittingBool, false);
        }

        // Wait a moment for stand animation to start
        yield return new WaitForSeconds(0.3f);

        // Calculate stand position (slightly in front of seat)
        Vector3 standPosition = _sitPoint.position + _sitPoint.forward * 0.5f;
        
        // Keep the Y position reasonable (use original Y or current ground)
        standPosition.y = _originalPlayerPosition.y;

        // Smoothly move player to stand position
        _transitionStartPos = player.transform.position;
        _transitionProgress = 0f;

        while (_transitionProgress < 1f)
        {
            _transitionProgress += Time.deltaTime / _moveToSeatDuration;
            float t = Mathf.SmoothStep(0f, 1f, _transitionProgress);

            player.transform.position = Vector3.Lerp(_transitionStartPos, standPosition, t);

            yield return null;
        }

        // Snap to final position
        player.transform.position = standPosition;

        // Re-enable character controller BEFORE ending interaction
        if (_playerCharacterController != null)
        {
            _playerCharacterController.enabled = true;
        }

        // End interaction state
        player.SetInteracting(false);

        _onPlayerStood?.Invoke();

        // Clear references BEFORE setting flags
        _seatedPlayer = null;
        _seatedPlayerAnimation = null;
        _playerCharacterController = null;
        _playerInput = null;
        _interactAction = null;
        _mobileInputHandler = null;
        _wasInteractPressedLastFrame = false;
        
        // Set flags last
        _isOccupied = false;
        _isTransitioning = false;
        
        if (_debugLog) Debug.Log("[SeatInteractable] Stand up complete, seat is now available");
    }

    /// <summary>
    /// Called by mobile UI button to trigger stand up.
    /// Wire this directly to the Interact button if needed.
    /// </summary>
    public void TriggerStandUp()
    {
        if (_isOccupied && !_isTransitioning)
        {
            if (_debugLog) Debug.Log("[SeatInteractable] TriggerStandUp called directly");
            StartCoroutine(StandUpSequence());
        }
    }

    /// <summary>
    /// Force the player to stand up (e.g., when taking damage or triggered by event).
    /// </summary>
    public void ForceStandUp()
    {
        if (_isOccupied && !_isTransitioning)
        {
            StartCoroutine(StandUpSequence());
        }
    }

    /// <summary>
    /// Emergency release - immediately free the player without animation.
    /// </summary>
    private void ForceReleasePlayer()
    {
        if (_seatedPlayer != null)
        {
            if (_playerCharacterController != null)
            {
                _playerCharacterController.enabled = true;
            }
            
            _seatedPlayer.SetInteracting(false);
            
            if (_seatedPlayerAnimation != null)
            {
                _seatedPlayerAnimation.SetBool(_isSittingBool, false);
            }
        }

        _seatedPlayer = null;
        _seatedPlayerAnimation = null;
        _playerCharacterController = null;
        _playerInput = null;
        _interactAction = null;
        _mobileInputHandler = null;
        _wasInteractPressedLastFrame = false;
        _isOccupied = false;
        _isTransitioning = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw sit point
        Transform point = _sitPoint != null ? _sitPoint : transform;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(point.position, 0.2f);

        // Draw facing direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(point.position, point.forward * 0.5f);

        // Draw a simple chair shape
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Vector3 seatCenter = point.position + Vector3.up * 0.25f;
        Gizmos.DrawCube(seatCenter, new Vector3(0.5f, 0.1f, 0.5f));
    }
}