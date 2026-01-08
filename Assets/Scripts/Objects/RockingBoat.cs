using UnityEngine;

/// <summary>
/// Simulates a boat rocking on water, synced with the water shader's wave parameters.
/// Can read wave settings from a water material or use manual overrides.
/// </summary>
public class RockingBoat : MonoBehaviour
{
    [Header("Water Reference")]
    [SerializeField] private Renderer _waterRenderer;
    [SerializeField] private bool _readFromShader = true;

    [Header("Manual Wave Settings (if not reading from shader)")]
    [SerializeField] private float _waveSpeedX = 1f;
    [SerializeField] private float _waveSpeedY = 1f;
    [SerializeField] private float _waveSize = 0.5f;
    [SerializeField] private Vector2 _waveDirection = new Vector2(1f, 0f);

    [Header("Rocking Motion")]
    [SerializeField] private float _rockAmplitudeX = 4f;  // Pitch (front-back)
    [SerializeField] private float _rockAmplitudeZ = 2f;  // Roll (side-side)
    [SerializeField] private float _rockSpeedMultiplier = 1f;

    [Header("Bobbing Motion (Up/Down)")]
    [SerializeField] private bool _enableBobbing = true;
    [SerializeField] private float _bobAmplitude = 0.2f;
    [SerializeField] private float _bobSpeedMultiplier = 1f;

    [Header("Position Offset")]
    [SerializeField] private float _phaseOffset = 0f;  // Offset to desync multiple boats

    // Shader property IDs (cached)
    private static readonly int WaveSpeedXID = Shader.PropertyToID("_WaveSpeedX");
    private static readonly int WaveSpeedYID = Shader.PropertyToID("_WaveSpeedY");
    private static readonly int WaveSizeID = Shader.PropertyToID("_WaveSize");
    private static readonly int WaveDirectionID = Shader.PropertyToID("_WaveDirection");
    private static readonly int WavesID = Shader.PropertyToID("_Waves");

    // Initial transforms
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private float _initialRotationX;
    private float _initialRotationZ;

    // Current wave values
    private float _currentWaveSpeedX;
    private float _currentWaveSpeedY;
    private float _currentWaveSize;
    private Vector2 _currentWaveSizeVector;
    private Vector2 _currentWaveDirection;
    private bool _wavesEnabled = true;

    private Material _waterMaterial;

    private void Awake()
    {
        _initialPosition = transform.localPosition;
        _initialRotation = transform.localRotation;
        _initialRotationX = transform.localEulerAngles.x;
        _initialRotationZ = transform.localEulerAngles.z;

        if (_waterRenderer != null)
        {
            _waterMaterial = _waterRenderer.sharedMaterial;
        }
    }

    private void Start()
    {
        // Add random phase offset if not set (for multiple boats)
        if (_phaseOffset == 0f)
        {
            _phaseOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        UpdateWaveParameters();
    }

    private void Update()
    {
        if (_readFromShader)
        {
            UpdateWaveParameters();
        }
        else
        {
            _currentWaveSpeedX = _waveSpeedX;
            _currentWaveSpeedY = _waveSpeedY;
            _currentWaveSize = _waveSize;
            _currentWaveDirection = _waveDirection;
        }

        ApplyRockingMotion();
    }

    private void UpdateWaveParameters()
    {
        if (_waterMaterial == null) return;

        // Read parameters from water shader
        if (_waterMaterial.HasProperty(WaveSpeedXID))
            _currentWaveSpeedX = _waterMaterial.GetFloat(WaveSpeedXID);

        if (_waterMaterial.HasProperty(WaveSpeedYID))
            _currentWaveSpeedY = _waterMaterial.GetFloat(WaveSpeedYID);

        // _WaveSize is Vector2 - use X component as primary size, Y as secondary
        if (_waterMaterial.HasProperty(WaveSizeID))
        {
            Vector2 waveSize = _waterMaterial.GetVector(WaveSizeID);
            _currentWaveSize = (waveSize.x + waveSize.y) * 0.5f; // Average of X and Y
            _currentWaveSizeVector = waveSize;
        }

        // _WaveDirection is a rotation in degrees
        if (_waterMaterial.HasProperty(WaveDirectionID))
        {
            float directionDegrees = _waterMaterial.GetFloat(WaveDirectionID);
            float directionRadians = directionDegrees * Mathf.Deg2Rad;
            _currentWaveDirection = new Vector2(Mathf.Cos(directionRadians), Mathf.Sin(directionRadians));
        }

        // _Waves is a boolean - check if waves are enabled
        if (_waterMaterial.HasProperty(WavesID))
        {
            _wavesEnabled = _waterMaterial.GetFloat(WavesID) > 0.5f;
        }
    }

    private void ApplyRockingMotion()
    {
        // Don't rock if waves are disabled in shader
        if (!_wavesEnabled)
        {
            transform.localPosition = _initialPosition;
            transform.localEulerAngles = new Vector3(
                _initialRotationX,
                transform.localEulerAngles.y,
                _initialRotationZ
            );
            return;
        }

        float time = Time.time + _phaseOffset;

        // Calculate wave influence based on direction
        float waveInfluenceX = _currentWaveDirection.x;
        float waveInfluenceZ = _currentWaveDirection.y;

        // Combined wave speed
        float avgWaveSpeed = (_currentWaveSpeedX + _currentWaveSpeedY) * 0.5f * _rockSpeedMultiplier;

        // Calculate rocking angles
        // Pitch (X rotation) - affected by wave direction X
        float pitchAngle = Mathf.Sin(time * avgWaveSpeed) * _rockAmplitudeX * _currentWaveSize;
        pitchAngle *= Mathf.Abs(waveInfluenceX) + 0.5f; // Always some movement

        // Roll (Z rotation) - affected by wave direction Y  
        float rollAngle = Mathf.Sin(time * avgWaveSpeed * 0.7f + 0.5f) * _rockAmplitudeZ * _currentWaveSize;
        rollAngle *= Mathf.Abs(waveInfluenceZ) + 0.5f;

        // Apply rotation
        float newRotationX = _initialRotationX + pitchAngle;
        float newRotationZ = _initialRotationZ + rollAngle;

        transform.localEulerAngles = new Vector3(
            newRotationX,
            transform.localEulerAngles.y,
            newRotationZ
        );

        // Apply bobbing (up/down motion)
        if (_enableBobbing)
        {
            float bobSpeed = avgWaveSpeed * _bobSpeedMultiplier;
            float bobOffset = Mathf.Sin(time * bobSpeed) * _bobAmplitude * _currentWaveSize;

            transform.localPosition = new Vector3(
                _initialPosition.x,
                _initialPosition.y + bobOffset,
                _initialPosition.z
            );
        }
    }

    /// <summary>
    /// Set a new water renderer at runtime.
    /// </summary>
    public void SetWaterRenderer(Renderer waterRenderer)
    {
        _waterRenderer = waterRenderer;
        if (_waterRenderer != null)
        {
            _waterMaterial = _waterRenderer.sharedMaterial;
        }
    }

    /// <summary>
    /// Manually set wave parameters (disables shader reading).
    /// </summary>
    public void SetWaveParameters(float speedX, float speedY, float size, Vector2 direction)
    {
        _readFromShader = false;
        _waveSpeedX = speedX;
        _waveSpeedY = speedY;
        _waveSize = size;
        _waveDirection = direction;
    }

    /// <summary>
    /// Reset boat to initial position and rotation.
    /// </summary>
    public void ResetPosition()
    {
        transform.localPosition = _initialPosition;
        transform.localRotation = _initialRotation;
    }

    private void OnValidate()
    {
        // Update in editor
        if (!Application.isPlaying && _waterRenderer != null)
        {
            _waterMaterial = _waterRenderer.sharedMaterial;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Show wave direction
        Gizmos.color = Color.cyan;
        Vector3 waveDir3D = new Vector3(_currentWaveDirection.x, 0f, _currentWaveDirection.y).normalized;
        Gizmos.DrawRay(transform.position, waveDir3D * 2f);

        // Show bob range
        if (_enableBobbing)
        {
            Gizmos.color = Color.yellow;
            Vector3 pos = Application.isPlaying ? _initialPosition : transform.localPosition;
            Gizmos.DrawLine(
                transform.parent != null ? transform.parent.TransformPoint(pos + Vector3.up * _bobAmplitude) : pos + Vector3.up * _bobAmplitude,
                transform.parent != null ? transform.parent.TransformPoint(pos - Vector3.up * _bobAmplitude) : pos - Vector3.up * _bobAmplitude
            );
        }
    }
}