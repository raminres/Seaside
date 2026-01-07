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

        if (_outlineRenderer != null)
        {
            var material = _outlineRenderer.material;
            if (material.HasProperty(_outlinePropertyName))
            {
                material.SetFloat(_outlinePropertyName, _outlineWidth);
            }
        }

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

        if (_outlineRenderer != null)
        {
            var material = _outlineRenderer.material;
            if (material.HasProperty(_outlinePropertyName))
            {
                material.SetFloat(_outlinePropertyName, 0f);
            }
        }

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
}
