using UnityEngine;

/// <summary>
/// ScriptableObject preset for water configuration.
/// Create presets for Ocean, River, Lake, etc.
/// </summary>
[CreateAssetMenu(fileName = "WaterPreset", menuName = "Seaside/Water/Water Preset")]
public class WaterPreset : ScriptableObject
{
    [Header("Water Type")]
    public WaterType waterType = WaterType.Ocean;
    
    [Header("Colors")]
    [ColorUsage(false, true)]
    public Color shallowColor = new Color(0.2f, 0.5f, 0.5f, 1f);
    [ColorUsage(false, true)]
    public Color deepColor = new Color(0.0f, 0.1f, 0.2f, 1f);
    [ColorUsage(false, true)]
    public Color foamColor = Color.white;
    [Range(0f, 1f)]
    public float transparency = 0.8f;
    
    [Header("Primary Waves (Large)")]
    public WaveSettings primaryWave1 = new WaveSettings(0f, 10f, 0.5f, 1f);
    public WaveSettings primaryWave2 = new WaveSettings(30f, 8f, 0.4f, 1.1f);
    public WaveSettings primaryWave3 = new WaveSettings(-20f, 6f, 0.3f, 0.9f);
    public WaveSettings primaryWave4 = new WaveSettings(45f, 4f, 0.2f, 1.2f);
    
    [Header("Secondary Waves (Detail)")]
    public WaveSettings secondaryWave1 = new WaveSettings(15f, 2f, 0.15f, 1.5f);
    public WaveSettings secondaryWave2 = new WaveSettings(-25f, 1.5f, 0.1f, 1.8f);
    
    [Header("Wave Global Settings")]
    [Range(0f, 2f)]
    public float waveScale = 1f;
    [Range(0f, 2f)]
    public float waveSpeed = 1f;
    
    [Header("Normal Maps")]
    public Texture2D normalMap1;
    public Texture2D normalMap2;
    [Range(0f, 2f)]
    public float normalStrength = 1f;
    public Vector2 normalTiling1 = new Vector2(10f, 10f);
    public Vector2 normalTiling2 = new Vector2(15f, 15f);
    public Vector2 normalSpeed1 = new Vector2(0.02f, 0.02f);
    public Vector2 normalSpeed2 = new Vector2(-0.015f, 0.01f);
    
    [Header("Depth Settings")]
    [Range(0.1f, 50f)]
    public float depthFadeDistance = 10f;
    [Range(0.1f, 20f)]
    public float shallowDepth = 2f;
    
    [Header("Refraction")]
    [Range(0f, 1f)]
    public float refractionStrength = 0.1f;
    
    [Header("Reflection")]
    [Range(0f, 1f)]
    public float reflectionStrength = 0.5f;
    [Range(1f, 10f)]
    public float fresnelPower = 4f;
    
    [Header("Foam")]
    public Texture2D foamTexture;
    [Range(0f, 5f)]
    public float foamDepthThreshold = 1f;
    [Range(0f, 2f)]
    public float foamWaveThreshold = 0.5f;
    [Range(0f, 1f)]
    public float foamSoftness = 0.3f;
    public Vector2 foamTiling = new Vector2(20f, 20f);
    [Range(0f, 2f)]
    public float foamSpeed = 0.5f;
    
    [Header("Caustics")]
    public Texture2D causticsTexture;
    [Range(0f, 2f)]
    public float causticsStrength = 0.5f;
    [Range(0.01f, 1f)]
    public float causticsScale = 0.1f;
    [Range(0f, 1f)]
    public float causticsSpeed = 0.2f;
    
    [Header("Flow (Rivers)")]
    public bool useFlowMap = false;
    public Texture2D flowMap;
    [Range(0f, 2f)]
    public float flowSpeed = 0.5f;
    [Range(0f, 1f)]
    public float flowStrength = 0.5f;
    
    [Header("Subsurface Scattering")]
    [ColorUsage(false, true)]
    public Color subsurfaceColor = new Color(0.1f, 0.4f, 0.3f, 1f);
    [Range(0f, 2f)]
    public float subsurfaceStrength = 0.5f;
    
    /// <summary>
    /// Get all primary wave directions as Vector2 array
    /// </summary>
    public Vector2[] GetPrimaryWaveDirections()
    {
        return new Vector2[]
        {
            primaryWave1.GetDirection(),
            primaryWave2.GetDirection(),
            primaryWave3.GetDirection(),
            primaryWave4.GetDirection()
        };
    }
    
    /// <summary>
    /// Get all secondary wave directions as Vector2 array
    /// </summary>
    public Vector2[] GetSecondaryWaveDirections()
    {
        return new Vector2[]
        {
            secondaryWave1.GetDirection(),
            secondaryWave2.GetDirection()
        };
    }
}

public enum WaterType
{
    Ocean,
    River,
    Lake,
    Pond,
    Custom
}

[System.Serializable]
public class WaveSettings
{
    [Tooltip("Direction in degrees (0 = +X, 90 = +Z)")]
    [Range(-180f, 180f)]
    public float direction = 0f;
    
    [Tooltip("Distance between wave peaks in world units")]
    [Range(0.5f, 50f)]
    public float wavelength = 10f;
    
    [Tooltip("Wave steepness (0 = flat, 1 = max before looping)")]
    [Range(0f, 1f)]
    public float steepness = 0.5f;
    
    [Tooltip("Speed multiplier for this wave")]
    [Range(0f, 3f)]
    public float speed = 1f;
    
    public WaveSettings(float dir, float wl, float steep, float spd)
    {
        direction = dir;
        wavelength = wl;
        steepness = steep;
        speed = spd;
    }
    
    /// <summary>
    /// Convert direction angle to Vector2
    /// </summary>
    public Vector2 GetDirection()
    {
        float rad = direction * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }
}
