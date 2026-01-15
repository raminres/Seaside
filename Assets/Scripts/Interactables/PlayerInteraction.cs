using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player interaction detection using triggers.
/// Add a trigger collider to the player for detection range.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private float _maxInteractionAngle = 60f;

    [Header("UI Reference (Optional)")]
    [SerializeField] private InteractionPromptUI _interactionPromptUI;

    [Header("Mobile Input (Optional)")]
    [SerializeField] private MobileInputHandler _mobileInputHandler;

    [Header("Debug")]
    [SerializeField] private bool _debugLog = false;

    private List<IInteractable> _interactablesInRange = new List<IInteractable>();
    private IInteractable _currentInteractable;
    private PlayerController _playerController;
    
    private bool _isHolding;
    private float _holdTimer;
    private bool _wasButtonDownLastFrame;

    public IInteractable CurrentInteractable => _currentInteractable;
    public bool HasInteractable => _currentInteractable != null && _currentInteractable.CanInteract;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();

        if (_cameraTransform == null)
        {
            _cameraTransform = Camera.main?.transform;
        }
    }

    private void Start()
    {
        if (_interactionPromptUI == null)
        {
            _interactionPromptUI = FindFirstObjectByType<InteractionPromptUI>();
        }

        if (_mobileInputHandler == null)
        {
            _mobileInputHandler = FindFirstObjectByType<MobileInputHandler>();
            
            if (_mobileInputHandler != null && _debugLog)
            {
                Debug.Log("[PlayerInteraction] Found MobileInputHandler");
            }
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        UpdateBestInteractable();
        ProcessMobileInteract();
        HandleHoldInteraction();
    }

    private void ProcessMobileInteract()
    {
        if (_mobileInputHandler == null) return;

        // Check for new press this frame
        bool pressedThisFrame = _mobileInputHandler.InteractPressedThisFrame;
        bool buttonDown = _mobileInputHandler.InteractButtonDown;

        // New press detected
        if (pressedThisFrame)
        {
            if (_debugLog) Debug.Log("[PlayerInteraction] Mobile interact pressed this frame");
            
            if (_currentInteractable != null && _currentInteractable.CanInteract)
            {
                if (_debugLog) Debug.Log($"[PlayerInteraction] Starting interaction with {(_currentInteractable as MonoBehaviour)?.gameObject.name}");
                
                switch (_currentInteractable.InteractionType)
                {
                    case InteractionType.Instant:
                    case InteractionType.Toggle:
                        CompleteInteraction();
                        break;

                    case InteractionType.Hold:
                        _isHolding = true;
                        _holdTimer = 0f;
                        break;
                }
            }
        }
        
        // Button released - cancel hold if in progress
        if (!buttonDown && _wasButtonDownLastFrame)
        {
            if (_debugLog) Debug.Log("[PlayerInteraction] Mobile interact released");
            
            if (_isHolding)
            {
                _isHolding = false;
                _holdTimer = 0f;
                
                if (_interactionPromptUI != null)
                {
                    _interactionPromptUI.SetHoldProgress(0f);
                }
            }
        }

        _wasButtonDownLastFrame = buttonDown;
    }

    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable == null)
        {
            interactable = other.GetComponentInParent<IInteractable>();
        }

        if (interactable != null && !_interactablesInRange.Contains(interactable))
        {
            _interactablesInRange.Add(interactable);
            
            if (_debugLog)
                Debug.Log($"[PlayerInteraction] Interactable entered range: {other.gameObject.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable == null)
        {
            interactable = other.GetComponentInParent<IInteractable>();
        }

        if (interactable != null)
        {
            _interactablesInRange.Remove(interactable);
            
            if (_debugLog)
                Debug.Log($"[PlayerInteraction] Interactable left range: {other.gameObject.name}");

            if (interactable == _currentInteractable)
            {
                _currentInteractable.OnUnfocused();
                _currentInteractable = null;
            }
        }
    }

    private void UpdateBestInteractable()
    {
        _interactablesInRange.RemoveAll(i => i == null || (i as MonoBehaviour) == null);

        IInteractable bestInteractable = null;
        float bestScore = float.MaxValue;

        foreach (var interactable in _interactablesInRange)
        {
            if (!interactable.CanInteract) continue;

            MonoBehaviour mb = interactable as MonoBehaviour;
            if (mb == null) continue;

            Vector3 directionToInteractable = (mb.transform.position - _cameraTransform.position).normalized;
            float angle = Vector3.Angle(_cameraTransform.forward, directionToInteractable);

            if (angle <= _maxInteractionAngle)
            {
                if (angle < bestScore)
                {
                    bestScore = angle;
                    bestInteractable = interactable;
                }
            }
        }

        if (bestInteractable != _currentInteractable)
        {
            _currentInteractable?.OnUnfocused();
            _currentInteractable = bestInteractable;
            _currentInteractable?.OnFocused();

            _isHolding = false;
            _holdTimer = 0f;
            
            if (_interactionPromptUI != null)
            {
                _interactionPromptUI.SetHoldProgress(0f);
            }
            
            if (_debugLog)
                Debug.Log($"[PlayerInteraction] Current interactable: {(_currentInteractable != null ? (_currentInteractable as MonoBehaviour)?.gameObject.name : "null")}");
        }
    }

    private void HandleHoldInteraction()
    {
        if (!_isHolding || _currentInteractable == null) return;

        if (_currentInteractable.InteractionType == InteractionType.Hold)
        {
            _holdTimer += Time.deltaTime;
            float progress = _holdTimer / _currentInteractable.HoldDuration;
            
            if (_interactionPromptUI != null)
            {
                _interactionPromptUI.SetHoldProgress(progress);
            }

            if (_holdTimer >= _currentInteractable.HoldDuration)
            {
                CompleteInteraction();
            }
        }
    }

    private void CompleteInteraction()
    {
        if (_currentInteractable == null) return;

        if (_debugLog) Debug.Log($"[PlayerInteraction] Completing interaction with {(_currentInteractable as MonoBehaviour)?.gameObject.name}");

        _currentInteractable.Interact(_playerController);
        
        _isHolding = false;
        _holdTimer = 0f;
        
        if (_interactionPromptUI != null)
        {
            _interactionPromptUI.SetHoldProgress(0f);
        }
    }

    #region Input Callbacks (Keyboard/Gamepad)

    public void OnInteract(InputValue value)
    {
        if (_currentInteractable == null || !_currentInteractable.CanInteract)
            return;

        bool pressed = value.isPressed;

        switch (_currentInteractable.InteractionType)
        {
            case InteractionType.Instant:
            case InteractionType.Toggle:
                if (pressed)
                {
                    CompleteInteraction();
                }
                break;

            case InteractionType.Hold:
                if (pressed)
                {
                    _isHolding = true;
                    _holdTimer = 0f;
                }
                else
                {
                    _isHolding = false;
                    _holdTimer = 0f;
                    
                    if (_interactionPromptUI != null)
                    {
                        _interactionPromptUI.SetHoldProgress(0f);
                    }
                }
                break;
        }
    }

    #endregion
}
