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

    [Header("Debug")]
    [SerializeField] private bool _debugLog = false;

    private List<IInteractable> _interactablesInRange = new List<IInteractable>();
    private IInteractable _currentInteractable;
    private PlayerController _playerController;
    
    private bool _isHolding;
    private float _holdTimer;

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
        // Auto-find UI if not assigned
        if (_interactionPromptUI == null)
        {
            _interactionPromptUI = FindFirstObjectByType<InteractionPromptUI>();
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        UpdateBestInteractable();
        HandleHoldInteraction();
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
                Debug.Log($"Interactable entered range: {other.gameObject.name}");
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
                Debug.Log($"Interactable left range: {other.gameObject.name}");

            if (interactable == _currentInteractable)
            {
                _currentInteractable.OnUnfocused();
                _currentInteractable = null;
            }
        }
    }

    private void UpdateBestInteractable()
    {
        // Clean up any null references (destroyed objects)
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

        // Update if changed
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
                Debug.Log($"Current interactable: {(_currentInteractable != null ? (_currentInteractable as MonoBehaviour)?.gameObject.name : "null")}");
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

        _currentInteractable.Interact(_playerController);
        
        _isHolding = false;
        _holdTimer = 0f;
        
        if (_interactionPromptUI != null)
        {
            _interactionPromptUI.SetHoldProgress(0f);
        }
    }

    #region Input Callbacks

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
