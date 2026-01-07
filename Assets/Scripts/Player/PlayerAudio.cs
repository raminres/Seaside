using UnityEngine;

/// <summary>
/// Handles player audio: footsteps, jump, land, swim.
/// Footstep methods are called from Animation Events.
/// </summary>
public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource _audioSource;

    [Header("Footstep Sounds")]
    [SerializeField] private AudioClip[] _footstepClips;
    [SerializeField] private AudioClip[] _runFootstepClips;
    [SerializeField] [Range(0f, 1f)] private float _footstepVolume = 0.5f;
    [SerializeField] [Range(0f, 0.2f)] private float _footstepPitchVariation = 0.1f;

    [Header("Jump & Land Sounds")]
    [SerializeField] private AudioClip _jumpSound;
    [SerializeField] private AudioClip _landSound;
    [SerializeField] private AudioClip _hardLandSound;
    [SerializeField] [Range(0f, 1f)] private float _jumpLandVolume = 0.6f;

    [Header("Swim Sounds")]
    [SerializeField] private AudioClip[] _swimStrokeClips;
    [SerializeField] private AudioClip _enterWaterSound;
    [SerializeField] private AudioClip _exitWaterSound;
    [SerializeField] [Range(0f, 1f)] private float _swimVolume = 0.4f;

    [Header("Interaction Sounds")]
    [SerializeField] private AudioClip _grabSound;
    [SerializeField] private AudioClip _interactSound;
    [SerializeField] [Range(0f, 1f)] private float _interactionVolume = 0.5f;

    [Header("References")]
    [SerializeField] private PlayerController _playerController;

    private float _basePitch = 1f;

    private void Awake()
    {
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 1f; // 3D sound
        _basePitch = _audioSource.pitch;

        if (_playerController == null)
        {
            _playerController = GetComponentInParent<PlayerController>();
        }
    }

    #region Animation Event Callbacks

    /// <summary>
    /// Called from Animation Event on footstep frames.
    /// </summary>
    public void OnFootstep()
    {
        if (_playerController == null) return;

        // Choose clip array based on movement state
        AudioClip[] clips = _playerController.CurrentState == PlayerState.Running 
            ? (_runFootstepClips.Length > 0 ? _runFootstepClips : _footstepClips)
            : _footstepClips;

        PlayRandomClip(clips, _footstepVolume);
    }

    /// <summary>
    /// Called from Animation Event - left foot.
    /// </summary>
    public void OnFootstepLeft()
    {
        OnFootstep();
    }

    /// <summary>
    /// Called from Animation Event - right foot.
    /// </summary>
    public void OnFootstepRight()
    {
        OnFootstep();
    }

    /// <summary>
    /// Called from Animation Event on swim stroke.
    /// </summary>
    public void OnSwimStroke()
    {
        PlayRandomClip(_swimStrokeClips, _swimVolume);
    }

    #endregion

    #region Public Methods (Called from PlayerController)

    public void PlayJumpSound()
    {
        PlayClip(_jumpSound, _jumpLandVolume);
    }

    public void PlayLandSound(bool hardLanding = false)
    {
        AudioClip clip = hardLanding && _hardLandSound != null ? _hardLandSound : _landSound;
        PlayClip(clip, _jumpLandVolume);
    }

    public void PlayEnterWaterSound()
    {
        PlayClip(_enterWaterSound, _swimVolume);
    }

    public void PlayExitWaterSound()
    {
        PlayClip(_exitWaterSound, _swimVolume);
    }

    public void PlayGrabSound()
    {
        PlayClip(_grabSound, _interactionVolume);
    }

    public void PlayInteractSound()
    {
        PlayClip(_interactSound, _interactionVolume);
    }

    #endregion

    #region Private Methods

    private void PlayClip(AudioClip clip, float volume)
    {
        if (clip == null || _audioSource == null) return;

        _audioSource.pitch = _basePitch;
        _audioSource.PlayOneShot(clip, volume);
    }

    private void PlayRandomClip(AudioClip[] clips, float volume)
    {
        if (clips == null || clips.Length == 0 || _audioSource == null) return;

        // Random clip
        int index = Random.Range(0, clips.Length);
        AudioClip clip = clips[index];

        // Random pitch variation
        _audioSource.pitch = _basePitch + Random.Range(-_footstepPitchVariation, _footstepPitchVariation);
        _audioSource.PlayOneShot(clip, volume);
    }

    #endregion
}
