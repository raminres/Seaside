using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Collectible item that requires interaction (press E) to pick up.
/// Works with GameManager.CollectItem() and supports animation/sound.
/// </summary>
public class CollectibleItem : InteractableBase
{
    [Header("Collectible Settings")]
    [SerializeField] private string _collectibleId;
    [SerializeField] private int _value = 1;
    [SerializeField] private bool _useGameManagerCounter = true;

    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private string _collectTrigger = "Collect";
    [SerializeField] private bool _bobUpDown = true;
    [SerializeField] private float _bobSpeed = 2f;
    [SerializeField] private float _bobHeight = 0.1f;
    [SerializeField] private bool _rotate = true;
    [SerializeField] private float _rotateSpeed = 90f;

    [Header("Audio")]
    [SerializeField] private AudioClip _collectSound;

    [Header("Events")]
    [SerializeField] private IntEventSo _onCollectedEvent;
    [SerializeField] private StringEventSo _onItemCollectedEvent;
    [SerializeField] private UnityEvent _onCollected;

    private Vector3 _startPosition;
    private bool _isCollected = false;
    private AudioSource _audioSource;

    private void Awake()
    {
        _interactionType = InteractionType.Instant;
        _startPosition = transform.position;

        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        // Setup audio source
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null && _collectSound != null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
    }

    /// <summary>
    /// Initialize method for compatibility with GameManager.SpawnCollectibles()
    /// </summary>
    public void Initialize(AudioClip clip)
    {
        _collectSound = clip;
        
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        _audioSource.clip = clip;
        _audioSource.playOnAwake = false;
    }

    private void Update()
    {
        if (_isCollected) return;

        if (_bobUpDown)
        {
            float newY = _startPosition.y + Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
            transform.position = new Vector3(_startPosition.x, newY, _startPosition.z);
        }

        if (_rotate)
        {
            transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime);
        }
    }

    public override bool CanInteract => !_isCollected && base.CanInteract;

    protected override void OnInteractInternal(PlayerController player)
    {
        if (_isCollected) return;
        Collect();
    }

    private void Collect()
    {
        _isCollected = true;

        // Notify GameManager (existing behavior)
        if (_useGameManagerCounter && GameManager.Instance != null)
        {
            GameManager.Instance.CollectItem();
        }

        // Fire events
        _onCollectedEvent?.RaiseEvent(_value);
        
        if (!string.IsNullOrEmpty(_collectibleId))
        {
            _onItemCollectedEvent?.RaiseEvent(_collectibleId);
        }

        _onCollected?.Invoke();

        // Play animation
        if (_animator != null)
        {
            _animator.SetTrigger(_collectTrigger);
        }

        // Play sound and destroy
        if (_collectSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_collectSound);
            StartCoroutine(WaitAndDestroy(_collectSound.length));
        }
        else
        {
            // No sound, destroy after short delay for animation
            StartCoroutine(WaitAndDestroy(0.25f));
        }
    }

    private IEnumerator WaitAndDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    // Disable highlight/outline when collected
    public override void OnFocused()
    {
        if (_isCollected) return;
        base.OnFocused();
    }
}