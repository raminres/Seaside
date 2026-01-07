using System;
using UnityEngine;

/// <summary>
/// Handles animator parameter updates and interaction animations.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerAudio _playerAudio;

    [Header("Animation Smoothing")]
    [SerializeField] private float _animationBlendSpeed = 10f;

    [Header("Interaction Animation Settings")]
    [SerializeField] private float _interactionAnimationDuration = 1f;

    // Animator parameter IDs (cached for performance)
    private static readonly int AnimIDSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimIDGrounded = Animator.StringToHash("Grounded");
    private static readonly int AnimIDJump = Animator.StringToHash("Jump");
    private static readonly int AnimIDFreeFall = Animator.StringToHash("FreeFall");
    private static readonly int AnimIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    private static readonly int AnimIDSwimming = Animator.StringToHash("Swimming");
    
    // Interaction triggers
    private static readonly int AnimIDGrab = Animator.StringToHash("Grab");
    private static readonly int AnimIDInteract = Animator.StringToHash("Interact");
    private static readonly int AnimIDLightFire = Animator.StringToHash("LightFire");

    private Animator _animator;
    private float _animationBlend;
    private bool _hasAnimator;

    // Interaction animation state
    private bool _isPlayingInteractionAnim;
    private Action _onInteractionAnimComplete;

    public bool IsPlayingInteractionAnimation => _isPlayingInteractionAnim;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _hasAnimator = _animator != null;

        if (_playerController == null)
        {
            _playerController = GetComponentInParent<PlayerController>();
        }

        if (_playerAudio == null)
        {
            _playerAudio = GetComponent<PlayerAudio>();
            if (_playerAudio == null)
            {
                _playerAudio = GetComponentInParent<PlayerAudio>();
            }
        }
    }

    private void Update()
    {
        if (!_hasAnimator || _playerController == null) return;

        // Don't update locomotion while playing interaction animation
        if (!_isPlayingInteractionAnim)
        {
            UpdateAnimatorParameters();
        }
    }

    private void UpdateAnimatorParameters()
    {
        float targetSpeed = _playerController.CurrentState switch
        {
            PlayerState.Running => 6f,
            PlayerState.Walking => 2f,
            PlayerState.Swimming => _playerController.Velocity.magnitude,
            _ => 0f
        };

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * _animationBlendSpeed);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        _animator.SetFloat(AnimIDSpeed, _animationBlend);
        _animator.SetFloat(AnimIDMotionSpeed, 1f);
        _animator.SetBool(AnimIDGrounded, _playerController.IsGrounded);
        _animator.SetBool(AnimIDJump, _playerController.CurrentState == PlayerState.Jumping);
        _animator.SetBool(AnimIDFreeFall, _playerController.CurrentState == PlayerState.Falling);
        _animator.SetBool(AnimIDSwimming, _playerController.CurrentState == PlayerState.Swimming);
    }

    #region Interaction Animations

    /// <summary>
    /// Play grab animation (for doors, collectibles).
    /// </summary>
    public void PlayGrabAnimation(Action onComplete = null)
    {
        PlayInteractionAnimation(AnimIDGrab, onComplete);
        _playerAudio?.PlayGrabSound();
    }

    /// <summary>
    /// Play generic interact animation.
    /// </summary>
    public void PlayInteractAnimation(Action onComplete = null)
    {
        PlayInteractionAnimation(AnimIDInteract, onComplete);
        _playerAudio?.PlayInteractSound();
    }

    /// <summary>
    /// Play fire lighting animation.
    /// </summary>
    public void PlayLightFireAnimation(Action onComplete = null)
    {
        PlayInteractionAnimation(AnimIDLightFire, onComplete);
    }

    private void PlayInteractionAnimation(int triggerId, Action onComplete)
    {
        if (!_hasAnimator) 
        {
            onComplete?.Invoke();
            return;
        }

        _isPlayingInteractionAnim = true;
        _onInteractionAnimComplete = onComplete;

        _animator.SetTrigger(triggerId);

        // Get animation length and invoke callback after
        // Using a coroutine would be cleaner, but this works for simplicity
        Invoke(nameof(OnInteractionAnimationComplete), _interactionAnimationDuration);
    }

    private void OnInteractionAnimationComplete()
    {
        _isPlayingInteractionAnim = false;
        _onInteractionAnimComplete?.Invoke();
        _onInteractionAnimComplete = null;
    }

    /// <summary>
    /// Called from Animation Event at the end of interaction animations.
    /// Use this instead of timed invoke for more accurate timing.
    /// </summary>
    public void OnInteractionAnimEnd()
    {
        CancelInvoke(nameof(OnInteractionAnimationComplete));
        OnInteractionAnimationComplete();
    }

    #endregion

    #region Public Methods

    public void PlayAnimation(string animationName)
    {
        if (_hasAnimator)
        {
            _animator.Play(animationName);
        }
    }

    public void SetTrigger(string triggerName)
    {
        if (_hasAnimator)
        {
            _animator.SetTrigger(triggerName);
        }
    }

    public void SetBool(string paramName, bool value)
    {
        if (_hasAnimator)
        {
            _animator.SetBool(paramName, value);
        }
    }

    #endregion
}
