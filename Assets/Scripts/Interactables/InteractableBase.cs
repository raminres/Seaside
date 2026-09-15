using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Base class for interactable objects. Extend this for specific behaviors.
/// </summary>
public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] protected string _interactionPrompt = "Interact";
    [SerializeField] protected InteractionType _interactionType = InteractionType.Instant;
    [SerializeField] protected float _holdDuration = 1f;
    [SerializeField] protected bool _canInteract = true;

    [Header("Visual Feedback")]
    [SerializeField] protected GameObject _highlightObject;
    [SerializeField] protected Renderer _outlineRenderer;
    [SerializeField] protected string _outlinePropertyName = "_OutlineWidth";
    [SerializeField] protected float _outlineWidth = 0.02f;

    [Header("Audio")]
    [SerializeField] protected AudioSource _audioSource;
    [SerializeField] protected AudioClip _interactSound;

    [Header("Events")]
    [SerializeField] protected UnityEvent _onInteract;
    [SerializeField] protected UnityEvent _onFocused;
    [SerializeField] protected UnityEvent _onUnfocused;

    public virtual string InteractionPrompt => _interactionPrompt;
    public virtual bool CanInteract => _canInteract;
    public virtual InteractionType InteractionType => _interactionType;
    public virtual float HoldDuration => _holdDuration;

    protected bool _isFocused;
    private MaterialPropertyBlock _outlinePropertyBlock;
    private int _outlinePropertyId;

    public virtual void Interact(PlayerController player)
    {
        if (!CanInteract) return;

        PlayInteractSound();
        _onInteract?.Invoke();
        OnInteractInternal(player);
    }

    /// <summary>
    /// Override this for specific interaction behavior.
    /// </summary>
    protected abstract void OnInteractInternal(PlayerController player);

    public virtual void OnFocused()
    {
        if (_isFocused) return;
        _isFocused = true;

        if (_highlightObject != null)
        {
            _highlightObject.SetActive(true);
        }

        SetOutlineWidth(_outlineWidth);

        _onFocused?.Invoke();
    }

    public virtual void OnUnfocused()
    {
        if (!_isFocused) return;
        _isFocused = false;

        if (_highlightObject != null)
        {
            _highlightObject.SetActive(false);
        }

        SetOutlineWidth(0f);

        _onUnfocused?.Invoke();
    }

    protected void PlayInteractSound()
    {
        if (_interactSound != null)
        {
            if (_audioSource != null)
            {
                _audioSource.PlayOneShot(_interactSound);
            }
            else
            {
                AudioSource.PlayClipAtPoint(_interactSound, transform.position);
            }
        }
    }

    public void SetInteractable(bool canInteract)
    {
        _canInteract = canInteract;
    }

    public void SetPrompt(string newPrompt)
    {
        _interactionPrompt = newPrompt;
    }

    private void SetOutlineWidth(float width)
    {
        if (_outlineRenderer == null || string.IsNullOrWhiteSpace(_outlinePropertyName)) return;
        if (_outlineRenderer.sharedMaterial == null || !_outlineRenderer.sharedMaterial.HasProperty(_outlinePropertyName)) return;

        _outlinePropertyBlock ??= new MaterialPropertyBlock();
        _outlinePropertyId = Shader.PropertyToID(_outlinePropertyName);
        _outlineRenderer.GetPropertyBlock(_outlinePropertyBlock);
        _outlinePropertyBlock.SetFloat(_outlinePropertyId, width);
        _outlineRenderer.SetPropertyBlock(_outlinePropertyBlock);
    }
}
