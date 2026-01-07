using UnityEngine;

/// <summary>
/// Handles animator parameter updates based on player state.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController _playerController;

    [Header("Animation Smoothing")]
    [SerializeField] private float _animationBlendSpeed = 10f;

    // Animator parameter IDs
    private static readonly int AnimIDSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimIDGrounded = Animator.StringToHash("Grounded");
    private static readonly int AnimIDJump = Animator.StringToHash("Jump");
    private static readonly int AnimIDFreeFall = Animator.StringToHash("FreeFall");
    private static readonly int AnimIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    private static readonly int AnimIDSwimming = Animator.StringToHash("Swimming");

    private Animator _animator;
    private float _animationBlend;
    private bool _hasAnimator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _hasAnimator = _animator != null;

        if (_playerController == null)
        {
            _playerController = GetComponentInParent<PlayerController>();
        }
    }

    private void Update()
    {
        if (!_hasAnimator || _playerController == null) return;

        UpdateAnimatorParameters();
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
}
