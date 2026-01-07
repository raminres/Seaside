using UnityEngine;

/// <summary>
/// Controls the day/night cycle using a preset ScriptableObject.
/// Swap presets at runtime for different moods.
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    [Header("Preset")]
    [SerializeField] private DayNightPreset _preset;

    [Header("Time")]
    [SerializeField] [Range(0f, 24f)] private float _currentTime = 12f;
    [SerializeField] private bool _progressTime = true;

    [Header("Light References")]
    [SerializeField] private Light _sunLight;
    [SerializeField] private Light _moonLight;

    [Header("Events (Optional)")]
    [SerializeField] private GameEventSo _onSunrise;
    [SerializeField] private GameEventSo _onSunset;
    [SerializeField] private GameEventSo _onMidnight;
    [SerializeField] private GameEventSo _onNoon;

    [Header("Debug")]
    [SerializeField] private bool _debugMode;

    // Time constants
    private const float SunriseTime = 6f;
    private const float SunsetTime = 18f;
    private const float NoonTime = 12f;

    // State tracking for events
    private bool _wasDaytime;
    private bool _wasNoon;
    private bool _wasMidnight;

    // Properties
    public float CurrentTime => _currentTime;
    public float NormalizedTime => _currentTime / 24f;
    public bool IsDaytime => _currentTime >= SunriseTime && _currentTime < SunsetTime;
    public DayNightPreset CurrentPreset => _preset;

    private void Start()
    {
        // Initialize state
        _wasDaytime = IsDaytime;
        UpdateLighting();
    }

    private void Update()
    {
        if (_preset == null) return;

        if (_progressTime)
        {
            UpdateTime();
        }

        UpdateLighting();
        CheckTimeEvents();
    }

    private void UpdateTime()
    {
        float timeProgressionRate = 24f / (_preset.dayLengthMinutes * 60f);
        _currentTime += Time.deltaTime * timeProgressionRate;

        if (_currentTime >= 24f)
        {
            _currentTime -= 24f;
        }
    }

    private void UpdateLighting()
    {
        if (_preset == null) return;

        float t = NormalizedTime;

        UpdateSun(t);
        UpdateMoon(t);
        UpdateAmbient(t);
        UpdateFog(t);
        UpdateSkybox(t);
    }

    private void UpdateSun(float t)
    {
        if (_sunLight == null) return;

        // Rotate sun
        float sunAngle = (t * 360f) - 90f;
        _sunLight.transform.rotation = Quaternion.Euler(sunAngle, _preset.sunOrbitAngle, 0f);

        // Color and intensity
        if (_preset.sunColor != null)
            _sunLight.color = _preset.sunColor.Evaluate(t);

        if (_preset.sunIntensity != null)
            _sunLight.intensity = _preset.sunIntensity.Evaluate(t) * _preset.maxSunIntensity;

        // Disable when not visible for performance
        _sunLight.enabled = _sunLight.intensity > 0.01f;
    }

    private void UpdateMoon(float t)
    {
        if (_moonLight == null) return;

        // Moon is opposite to sun
        float moonAngle = ((t + 0.5f) % 1f * 360f) - 90f;
        _moonLight.transform.rotation = Quaternion.Euler(moonAngle, _preset.sunOrbitAngle, 0f);

        _moonLight.color = _preset.moonColor;

        // Moon visible when sun is not
        float sunIntensityValue = _preset.sunIntensity != null ? _preset.sunIntensity.Evaluate(t) : (IsDaytime ? 1f : 0f);
        float moonVisibility = 1f - sunIntensityValue;
        _moonLight.intensity = moonVisibility * _preset.moonIntensity;

        _moonLight.enabled = _moonLight.intensity > 0.01f;
    }

    private void UpdateAmbient(float t)
    {
        if (_preset.ambientColor != null)
            RenderSettings.ambientLight = _preset.ambientColor.Evaluate(t);

        if (_preset.ambientIntensity != null)
            RenderSettings.ambientIntensity = _preset.ambientIntensity.Evaluate(t);
    }

    private void UpdateFog(float t)
    {
        if (!_preset.controlFog) return;

        if (_preset.fogColor != null)
            RenderSettings.fogColor = _preset.fogColor.Evaluate(t);

        if (_preset.fogDensity != null)
            RenderSettings.fogDensity = _preset.fogDensity.Evaluate(t) * _preset.maxFogDensity;
    }

    private void UpdateSkybox(float t)
    {
        if (_preset.skyboxMaterial == null) return;
        if (_preset.skyboxExposure == null) return;
        if (string.IsNullOrEmpty(_preset.skyboxExposureParam)) return;

        float exposure = _preset.skyboxExposure.Evaluate(t);
        _preset.skyboxMaterial.SetFloat(_preset.skyboxExposureParam, exposure);
    }

    private void CheckTimeEvents()
    {
        bool isDaytime = IsDaytime;
        bool isNoon = _currentTime >= NoonTime - 0.1f && _currentTime < NoonTime + 0.1f;
        bool isMidnight = _currentTime >= 23.9f || _currentTime < 0.1f;

        // Sunrise
        if (isDaytime && !_wasDaytime)
        {
            _onSunrise?.RaiseEvent();
            if (_debugMode) Debug.Log("☀️ Sunrise");
        }

        // Sunset
        if (!isDaytime && _wasDaytime)
        {
            _onSunset?.RaiseEvent();
            if (_debugMode) Debug.Log("🌅 Sunset");
        }

        // Noon
        if (isNoon && !_wasNoon)
        {
            _onNoon?.RaiseEvent();
            if (_debugMode) Debug.Log("🌞 Noon");
        }

        // Midnight
        if (isMidnight && !_wasMidnight)
        {
            _onMidnight?.RaiseEvent();
            if (_debugMode) Debug.Log("🌙 Midnight");
        }

        _wasDaytime = isDaytime;
        _wasNoon = isNoon;
        _wasMidnight = isMidnight;
    }

    #region Public Methods

    /// <summary>
    /// Set time of day (0-24).
    /// </summary>
    public void SetTime(float time)
    {
        _currentTime = Mathf.Repeat(time, 24f);
        UpdateLighting();
    }

    /// <summary>
    /// Set time as normalized value (0-1).
    /// </summary>
    public void SetNormalizedTime(float t)
    {
        SetTime(t * 24f);
    }

    /// <summary>
    /// Pause or resume time progression.
    /// </summary>
    public void SetTimeProgression(bool progress)
    {
        _progressTime = progress;
    }

    /// <summary>
    /// Swap to a different preset at runtime.
    /// </summary>
    public void SetPreset(DayNightPreset newPreset)
    {
        _preset = newPreset;
        UpdateLighting();
    }

    /// <summary>
    /// Skip to sunrise (6:00).
    /// </summary>
    public void SkipToSunrise() => SetTime(SunriseTime);

    /// <summary>
    /// Skip to noon (12:00).
    /// </summary>
    public void SkipToNoon() => SetTime(NoonTime);

    /// <summary>
    /// Skip to sunset (18:00).
    /// </summary>
    public void SkipToSunset() => SetTime(SunsetTime);

    /// <summary>
    /// Skip to midnight (0:00).
    /// </summary>
    public void SkipToMidnight() => SetTime(0f);

    /// <summary>
    /// Add or subtract hours from current time.
    /// </summary>
    public void AddHours(float hours)
    {
        SetTime(_currentTime + hours);
    }

    #endregion

    #region Editor

    private void OnValidate()
    {
        // Update in editor when time slider changes
        if (_debugMode && _preset != null)
        {
            UpdateLighting();
        }
    }

    #endregion
}
