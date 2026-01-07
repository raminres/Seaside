using UnityEngine;

/// <summary>
/// Preset for day/night cycle lighting settings.
/// Create different presets for different moods (Sunny, Stormy, etc.)
/// </summary>
[CreateAssetMenu(fileName = "DayNightPreset", menuName = "Seaside/Environment/Day Night Preset")]
public class DayNightPreset : ScriptableObject
{
    [Header("Time Settings")]
    [Tooltip("How many real-time minutes for a full 24-hour cycle")]
    public float dayLengthMinutes = 10f;

    [Header("Sun Settings")]
    public Gradient sunColor;
    public AnimationCurve sunIntensity;
    [Range(0f, 3f)] public float maxSunIntensity = 1.2f;
    [Tooltip("Y-axis rotation of sun path")]
    [Range(0f, 360f)] public float sunOrbitAngle = 170f;

    [Header("Moon Settings")]
    public Color moonColor = new Color(0.6f, 0.7f, 1f);
    [Range(0f, 2f)] public float moonIntensity = 0.3f;

    [Header("Ambient Light")]
    public Gradient ambientColor;
    public AnimationCurve ambientIntensity;

    [Header("Fog")]
    public bool controlFog = true;
    public Gradient fogColor;
    public AnimationCurve fogDensity;
    [Range(0f, 0.1f)] public float maxFogDensity = 0.02f;

    [Header("Skybox (Optional)")]
    [Tooltip("Leave null to not control skybox")]
    public Material skyboxMaterial;
    [Tooltip("Exposure parameter name in skybox shader")]
    public string skyboxExposureParam = "_Exposure";
    public AnimationCurve skyboxExposure;

    /// <summary>
    /// Creates a preset with sensible default values.
    /// Called when you create a new preset asset.
    /// </summary>
    private void Reset()
    {
        SetDefaultValues();
    }

    [ContextMenu("Reset to Default Values")]
    public void SetDefaultValues()
    {
        dayLengthMinutes = 10f;
        maxSunIntensity = 1.2f;
        sunOrbitAngle = 170f;
        moonColor = new Color(0.6f, 0.7f, 1f);
        moonIntensity = 0.3f;
        controlFog = true;
        maxFogDensity = 0.02f;
        skyboxExposureParam = "_Exposure";

        // Sun Color Gradient (warm sunrise/sunset, white midday)
        sunColor = new Gradient();
        sunColor.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.2f, 0.2f, 0.4f), 0f),      // Midnight - dark blue
                new GradientColorKey(new Color(1f, 0.5f, 0.3f), 0.23f),     // Pre-sunrise - orange
                new GradientColorKey(new Color(1f, 0.85f, 0.7f), 0.3f),     // Sunrise - warm
                new GradientColorKey(new Color(1f, 0.95f, 0.9f), 0.5f),     // Noon - white
                new GradientColorKey(new Color(1f, 0.85f, 0.7f), 0.7f),     // Pre-sunset - warm
                new GradientColorKey(new Color(1f, 0.4f, 0.2f), 0.77f),     // Sunset - orange/red
                new GradientColorKey(new Color(0.2f, 0.2f, 0.4f), 1f)       // Midnight - dark blue
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );

        // Sun Intensity Curve
        sunIntensity = new AnimationCurve();
        sunIntensity.AddKey(new Keyframe(0f, 0f));       // Midnight
        sunIntensity.AddKey(new Keyframe(0.2f, 0f));     // Before sunrise
        sunIntensity.AddKey(new Keyframe(0.25f, 0.3f));  // Sunrise start
        sunIntensity.AddKey(new Keyframe(0.35f, 1f));    // Morning
        sunIntensity.AddKey(new Keyframe(0.5f, 1f));     // Noon
        sunIntensity.AddKey(new Keyframe(0.65f, 1f));    // Afternoon
        sunIntensity.AddKey(new Keyframe(0.75f, 0.3f));  // Sunset end
        sunIntensity.AddKey(new Keyframe(0.8f, 0f));     // After sunset
        sunIntensity.AddKey(new Keyframe(1f, 0f));       // Midnight
        SmoothCurve(sunIntensity);

        // Ambient Color Gradient
        ambientColor = new Gradient();
        ambientColor.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.05f, 0.05f, 0.1f), 0f),    // Midnight
                new GradientColorKey(new Color(0.3f, 0.2f, 0.2f), 0.25f),   // Sunrise
                new GradientColorKey(new Color(0.5f, 0.5f, 0.5f), 0.5f),    // Noon
                new GradientColorKey(new Color(0.3f, 0.2f, 0.2f), 0.75f),   // Sunset
                new GradientColorKey(new Color(0.05f, 0.05f, 0.1f), 1f)     // Midnight
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );

        // Ambient Intensity Curve
        ambientIntensity = new AnimationCurve();
        ambientIntensity.AddKey(new Keyframe(0f, 0.1f));     // Midnight
        ambientIntensity.AddKey(new Keyframe(0.25f, 0.4f));  // Sunrise
        ambientIntensity.AddKey(new Keyframe(0.5f, 1f));     // Noon
        ambientIntensity.AddKey(new Keyframe(0.75f, 0.4f));  // Sunset
        ambientIntensity.AddKey(new Keyframe(1f, 0.1f));     // Midnight
        SmoothCurve(ambientIntensity);

        // Fog Color Gradient
        fogColor = new Gradient();
        fogColor.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.1f, 0.1f, 0.15f), 0f),     // Midnight
                new GradientColorKey(new Color(0.7f, 0.5f, 0.4f), 0.25f),   // Sunrise
                new GradientColorKey(new Color(0.8f, 0.85f, 0.9f), 0.5f),   // Noon
                new GradientColorKey(new Color(0.7f, 0.4f, 0.3f), 0.75f),   // Sunset
                new GradientColorKey(new Color(0.1f, 0.1f, 0.15f), 1f)      // Midnight
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );

        // Fog Density Curve (denser at night and morning)
        fogDensity = new AnimationCurve();
        fogDensity.AddKey(new Keyframe(0f, 0.8f));      // Midnight
        fogDensity.AddKey(new Keyframe(0.25f, 1f));     // Sunrise (morning fog)
        fogDensity.AddKey(new Keyframe(0.4f, 0.3f));    // Late morning
        fogDensity.AddKey(new Keyframe(0.5f, 0.2f));    // Noon (clear)
        fogDensity.AddKey(new Keyframe(0.75f, 0.4f));   // Sunset
        fogDensity.AddKey(new Keyframe(1f, 0.8f));      // Midnight
        SmoothCurve(fogDensity);

        // Skybox Exposure Curve
        skyboxExposure = new AnimationCurve();
        skyboxExposure.AddKey(new Keyframe(0f, 0.2f));      // Midnight
        skyboxExposure.AddKey(new Keyframe(0.25f, 0.8f));   // Sunrise
        skyboxExposure.AddKey(new Keyframe(0.5f, 1.2f));    // Noon
        skyboxExposure.AddKey(new Keyframe(0.75f, 0.8f));   // Sunset
        skyboxExposure.AddKey(new Keyframe(1f, 0.2f));      // Midnight
        SmoothCurve(skyboxExposure);
    }

    private void SmoothCurve(AnimationCurve curve)
    {
        for (int i = 0; i < curve.keys.Length; i++)
        {
            curve.SmoothTangents(i, 0f);
        }
    }
}
