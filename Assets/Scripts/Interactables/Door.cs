using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Door that can be opened and closed via interaction.
/// Door always opens away from the player.
/// </summary>
public class Door : InteractableBase
{
    [Header("Door Settings")]
    [SerializeField] private Transform _doorPivot;
    [SerializeField] private float _openAngle = 90f;
    [SerializeField] private float _openSpeed = 5f;
    [SerializeField] private bool _autoClose = true;
    [SerializeField] private float _autoCloseDelay = 3f;
    [SerializeField] private bool _invertOpenDirection = false;

    [Header("Animation (Optional)")]
    [SerializeField] private Animator _doorAnimator;
    [SerializeField] private string _doorOpenBool = "DoorOpen";
    [SerializeField] private bool _useAnimator = false;

    [Header("Door Audio")]
    [SerializeField] private AudioClip _openSound;
    [SerializeField] private AudioClip _closeSound;

    [Header("Door Prompts")]
    [SerializeField] private string _openPrompt = "Open";
    [SerializeField] private string _closePrompt = "Close";

    [Header("Events")]
    [SerializeField] private UnityEvent _onDoorOpened;
    [SerializeField] private UnityEvent _onDoorClosed;

    private bool _isOpen;
    private float _currentAngle;
    private float _targetAngle;
    private float _autoCloseTimer;
    private Quaternion _closedRotation;
    private int _openDirection = 1; // 1 or -1 based on player position

    public bool IsOpen => _isOpen;

    private void Awake()
    {
        _interactionType = InteractionType.Toggle;
        
        if (_doorPivot != null)
        {
            _closedRotation = _doorPivot.localRotation;
        }

        // Setup audio source if not assigned
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.spatialBlend = 1f; // 3D sound
            }
        }
    }

    public override string InteractionPrompt => _isOpen ? _closePrompt : _openPrompt;

    protected override void OnInteractInternal(PlayerController player)
    {
        Debug.Log("Door interaction triggered!");  // Add this line
    
        if (_isOpen)
        {
            Close();
        }
        else
        {
            DetermineOpenDirection(player.transform);
            Open();
        }
    }

    private void DetermineOpenDirection(Transform playerTransform)
    {
        if (_doorPivot == null) return;

        // Get door's forward direction
        Vector3 doorForward = _doorPivot.forward;
        
        // Get direction from door to player
        Vector3 toPlayer = (playerTransform.position - _doorPivot.position).normalized;
        
        // Dot product tells us which side of the door the player is on
        float dot = Vector3.Dot(doorForward, toPlayer);
        
        // Open away from player
        _openDirection = dot > 0 ? -1 : 1;
        if (_invertOpenDirection) _openDirection *= -1;
    }

    private void Update()
    {
        if (!_useAnimator)
        {
            UpdateDoorRotation();
        }

        UpdateAutoClose();
    }

    private void UpdateDoorRotation()
    {
        if (_doorPivot == null) return;
        if (Mathf.Abs(_currentAngle - _targetAngle) < 0.1f) return;

        _currentAngle = Mathf.Lerp(_currentAngle, _targetAngle, Time.deltaTime * _openSpeed);
        _doorPivot.localRotation = _closedRotation * Quaternion.Euler(0f, _currentAngle, 0f);
    }

    private void UpdateAutoClose()
    {
        if (!_autoClose || !_isOpen) return;

        _autoCloseTimer -= Time.deltaTime;
        if (_autoCloseTimer <= 0f)
        {
            Close();
        }
    }

    public void Open()
    {
        if (_isOpen) return;
        
        _isOpen = true;
        _autoCloseTimer = _autoCloseDelay;

        if (_useAnimator && _doorAnimator != null)
        {
            _doorAnimator.SetBool(_doorOpenBool, true);
        }
        else
        {
            _targetAngle = _openAngle * _openDirection;
        }

        if (_openSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_openSound);
        }

        _onDoorOpened?.Invoke();
    }

    public void Close()
    {
        if (!_isOpen) return;
        
        _isOpen = false;

        if (_useAnimator && _doorAnimator != null)
        {
            _doorAnimator.SetBool(_doorOpenBool, false);
        }
        else
        {
            _targetAngle = 0f;
        }

        if (_closeSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_closeSound);
        }

        _onDoorClosed?.Invoke();
    }

    public void Toggle()
    {
        if (_isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    /// <summary>
    /// Call this when player exits the door area (optional auto-close trigger).
    /// </summary>
    public void OnPlayerExitArea()
    {
        if (_autoClose && _isOpen)
        {
            Close();
        }
    }
}
