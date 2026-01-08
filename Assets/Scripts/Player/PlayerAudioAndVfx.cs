using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Handles player audio and footstep VFX.
/// Supports different footstep sounds/VFX based on surface material (Rock, Sand, Wood).
/// Footstep methods are called from Animation Events.
/// </summary>
public class PlayerAudioAndVfx : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource _audioSource;

    [Header("Surface Detection")]
    [SerializeField] private float _surfaceCheckDistance = 1.5f;
    [SerializeField] private LayerMask _groundLayers;
    [SerializeField] private Transform _raycastOrigin;

    [Header("Surface Layers")]
    [SerializeField] private LayerMask _rockLayer;
    [SerializeField] private LayerMask _sandLayer;
    [SerializeField] private LayerMask _woodLayer;

    [Header("Default Footstep Sounds")]
    [SerializeField] private AudioClip[] _defaultFootstepClips;
    [SerializeField] private AudioClip[] _defaultRunFootstepClips;

    [Header("Rock Footstep Sounds")]
    [SerializeField] private AudioClip[] _rockFootstepClips;
    [SerializeField] private AudioClip[] _rockRunFootstepClips;

    [Header("Sand Footstep Sounds")]
    [SerializeField] private AudioClip[] _sandFootstepClips;
    [SerializeField] private AudioClip[] _sandRunFootstepClips;

    [Header("Wood Footstep Sounds")]
    [SerializeField] private AudioClip[] _woodFootstepClips;
    [SerializeField] private AudioClip[] _woodRunFootstepClips;

    [Header("Footstep Settings")]
    [SerializeField] [Range(0f, 1f)] private float _footstepVolume = 0.5f;
    [SerializeField] [Range(0f, 0.2f)] private float _footstepPitchVariation = 0.1f;

    [Header("Footstep VFX - Left Foot")]
    [SerializeField] private VisualEffect _defaultFootstepVFXLeft;
    [SerializeField] private VisualEffect _sandFootstepVFXLeft;
    [SerializeField] private VisualEffect _rockFootstepVFXLeft;
    [SerializeField] private VisualEffect _woodFootstepVFXLeft;

    [Header("Footstep VFX - Right Foot")]
    [SerializeField] private VisualEffect _defaultFootstepVFXRight;
    [SerializeField] private VisualEffect _sandFootstepVFXRight;
    [SerializeField] private VisualEffect _rockFootstepVFXRight;
    [SerializeField] private VisualEffect _woodFootstepVFXRight;

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
    private bool _isLeftFoot = true;
    private SurfaceType _currentSurface = SurfaceType.Default;

    public enum SurfaceType
    {
        Default,
        Rock,
        Sand,
        Wood
    }

    public SurfaceType CurrentSurface => _currentSurface;

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
        _audioSource.spatialBlend = 1f;
        _basePitch = _audioSource.pitch;

        if (_playerController == null)
        {
            _playerController = GetComponentInParent<PlayerController>();
        }

        if (_raycastOrigin == null)
        {
            _raycastOrigin = transform;
        }

        // Combine all ground layers for detection
        if (_groundLayers == 0)
        {
            _groundLayers = _rockLayer | _sandLayer | _woodLayer;
        }
    }

    private void Update()
    {
        UpdateCurrentSurface();
    }

    #region Surface Detection

    private void UpdateCurrentSurface()
    {
        if (Physics.Raycast(_raycastOrigin.position, Vector3.down, out RaycastHit hit, _surfaceCheckDistance, _groundLayers))
        {
            int hitLayer = hit.collider.gameObject.layer;
            int hitLayerMask = 1 << hitLayer;

            if ((_rockLayer & hitLayerMask) != 0)
            {
                _currentSurface = SurfaceType.Rock;
            }
            else if ((_sandLayer & hitLayerMask) != 0)
            {
                _currentSurface = SurfaceType.Sand;
            }
            else if ((_woodLayer & hitLayerMask) != 0)
            {
                _currentSurface = SurfaceType.Wood;
            }
            else
            {
                _currentSurface = SurfaceType.Default;
            }
        }
        else
        {
            _currentSurface = SurfaceType.Default;
        }
    }

    private AudioClip[] GetFootstepClipsForSurface(bool isRunning)
    {
        return _currentSurface switch
        {
            SurfaceType.Rock => isRunning 
                ? (_rockRunFootstepClips.Length > 0 ? _rockRunFootstepClips : _rockFootstepClips) 
                : _rockFootstepClips,
            
            SurfaceType.Sand => isRunning 
                ? (_sandRunFootstepClips.Length > 0 ? _sandRunFootstepClips : _sandFootstepClips) 
                : _sandFootstepClips,
            
            SurfaceType.Wood => isRunning 
                ? (_woodRunFootstepClips.Length > 0 ? _woodRunFootstepClips : _woodFootstepClips) 
                : _woodFootstepClips,
            
            _ => isRunning 
                ? (_defaultRunFootstepClips.Length > 0 ? _defaultRunFootstepClips : _defaultFootstepClips) 
                : _defaultFootstepClips
        };
    }

    private VisualEffect GetVFXForSurface(bool isLeftFoot)
    {
        return _currentSurface switch
        {
            SurfaceType.Rock => isLeftFoot 
                ? (_rockFootstepVFXLeft != null ? _rockFootstepVFXLeft : _defaultFootstepVFXLeft)
                : (_rockFootstepVFXRight != null ? _rockFootstepVFXRight : _defaultFootstepVFXRight),
            
            SurfaceType.Sand => isLeftFoot 
                ? (_sandFootstepVFXLeft != null ? _sandFootstepVFXLeft : _defaultFootstepVFXLeft)
                : (_sandFootstepVFXRight != null ? _sandFootstepVFXRight : _defaultFootstepVFXRight),
            
            SurfaceType.Wood => isLeftFoot 
                ? (_woodFootstepVFXLeft != null ? _woodFootstepVFXLeft : _defaultFootstepVFXLeft)
                : (_woodFootstepVFXRight != null ? _woodFootstepVFXRight : _defaultFootstepVFXRight),
            
            _ => isLeftFoot ? _defaultFootstepVFXLeft : _defaultFootstepVFXRight
        };
    }

    #endregion

    #region Animation Event Callbacks

    /// <summary>
    /// Called from Animation Event on footstep frames. Alternates between feet.
    /// </summary>
    public void OnFootstep()
    {
        if (_playerController == null) return;

        bool isRunning = _playerController.CurrentState == PlayerState.Running;

        // Play sound
        AudioClip[] clips = GetFootstepClipsForSurface(isRunning);
        if (clips != null && clips.Length > 0)
        {
            PlayRandomClip(clips, _footstepVolume);
        }

        // Play VFX for current foot
        PlayFootstepVFX(_isLeftFoot);

        // Alternate foot for next step
        _isLeftFoot = !_isLeftFoot;
    }

    /// <summary>
    /// Called from Animation Event - left foot specifically.
    /// </summary>
    public void OnFootstepLeft()
    {
        if (_playerController == null) return;

        bool isRunning = _playerController.CurrentState == PlayerState.Running;

        AudioClip[] clips = GetFootstepClipsForSurface(isRunning);
        if (clips != null && clips.Length > 0)
        {
            PlayRandomClip(clips, _footstepVolume);
        }

        PlayFootstepVFX(true);
    }

    /// <summary>
    /// Called from Animation Event - right foot specifically.
    /// </summary>
    public void OnFootstepRight()
    {
        if (_playerController == null) return;

        bool isRunning = _playerController.CurrentState == PlayerState.Running;

        AudioClip[] clips = GetFootstepClipsForSurface(isRunning);
        if (clips != null && clips.Length > 0)
        {
            PlayRandomClip(clips, _footstepVolume);
        }

        PlayFootstepVFX(false);
    }

    /// <summary>
    /// Called from Animation Event on swim stroke.
    /// </summary>
    public void OnSwimStroke()
    {
        PlayRandomClip(_swimStrokeClips, _swimVolume);
    }

    #endregion

    #region Footstep VFX

    private void PlayFootstepVFX(bool isLeftFoot)
    {
        VisualEffect vfx = GetVFXForSurface(isLeftFoot);
        if (vfx != null)
        {
            vfx.Play();
        }
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

        int index = Random.Range(0, clips.Length);
        AudioClip clip = clips[index];

        _audioSource.pitch = _basePitch + Random.Range(-_footstepPitchVariation, _footstepPitchVariation);
        _audioSource.PlayOneShot(clip, volume);
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        Transform origin = _raycastOrigin != null ? _raycastOrigin : transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin.position, origin.position + Vector3.down * _surfaceCheckDistance);

        // Show current surface in scene view
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(origin.position + Vector3.up * 0.5f, $"Surface: {_currentSurface}");
        #endif
    }

    #endregion
}