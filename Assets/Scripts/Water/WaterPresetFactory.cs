using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Factory for creating water presets with sensible defaults.
/// Use the menu items to create preset assets.
/// </summary>
public static class WaterPresetFactory
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/Seaside/Water/Ocean Preset")]
    public static void CreateOceanPreset()
    {
        var preset = ScriptableObject.CreateInstance<WaterPreset>();
        ConfigureOcean(preset);
        SavePreset(preset, "OceanPreset");
    }
    
    [MenuItem("Assets/Create/Seaside/Water/River Preset")]
    public static void CreateRiverPreset()
    {
        var preset = ScriptableObject.CreateInstance<WaterPreset>();
        ConfigureRiver(preset);
        SavePreset(preset, "RiverPreset");
    }
    
    [MenuItem("Assets/Create/Seaside/Water/Lake Preset")]
    public static void CreateLakePreset()
    {
        var preset = ScriptableObject.CreateInstance<WaterPreset>();
        ConfigureLake(preset);
        SavePreset(preset, "LakePreset");
    }
    
    [MenuItem("Assets/Create/Seaside/Water/Pond Preset")]
    public static void CreatePondPreset()
    {
        var preset = ScriptableObject.CreateInstance<WaterPreset>();
        ConfigurePond(preset);
        SavePreset(preset, "PondPreset");
    }
    
    private static void SavePreset(WaterPreset preset, string name)
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Water Preset",
            name,
            "asset",
            "Choose location for water preset"
        );
        
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(preset, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = preset;
        }
    }
#endif

    /// <summary>
    /// Configure preset for ocean water - large waves, deep blue
    /// </summary>
    public static void ConfigureOcean(WaterPreset preset)
    {
        preset.waterType = WaterType.Ocean;
        
        // Colors - deep ocean blue
        preset.shallowColor = new Color(0.15f, 0.45f, 0.55f, 1f);
        preset.deepColor = new Color(0.02f, 0.08f, 0.18f, 1f);
        preset.foamColor = new Color(0.95f, 0.98f, 1f, 1f);
        preset.transparency = 0.85f;
        
        // Primary waves - large, dramatic
        preset.primaryWave1 = new WaveSettings(0f, 12f, 0.5f, 1f);
        preset.primaryWave2 = new WaveSettings(25f, 9f, 0.4f, 1.1f);
        preset.primaryWave3 = new WaveSettings(-15f, 7f, 0.35f, 0.95f);
        preset.primaryWave4 = new WaveSettings(40f, 5f, 0.25f, 1.2f);
        
        // Secondary waves - medium detail
        preset.secondaryWave1 = new WaveSettings(10f, 2.5f, 0.15f, 1.4f);
        preset.secondaryWave2 = new WaveSettings(-20f, 1.8f, 0.12f, 1.6f);
        
        preset.waveScale = 1f;
        preset.waveSpeed = 1f;
        
        // Normals
        preset.normalStrength = 1.2f;
        preset.normalTiling1 = new Vector2(8f, 8f);
        preset.normalTiling2 = new Vector2(12f, 12f);
        preset.normalSpeed1 = new Vector2(0.03f, 0.02f);
        preset.normalSpeed2 = new Vector2(-0.02f, 0.015f);
        
        // Depth
        preset.depthFadeDistance = 15f;
        preset.shallowDepth = 3f;
        
        // Refraction & Reflection
        preset.refractionStrength = 0.08f;
        preset.reflectionStrength = 0.6f;
        preset.fresnelPower = 4f;
        
        // Foam - moderate shore foam, visible wave crests
        preset.foamDepthThreshold = 1.5f;
        preset.foamWaveThreshold = 0.4f;
        preset.foamSoftness = 0.35f;
        preset.foamTiling = new Vector2(25f, 25f);
        preset.foamSpeed = 0.4f;
        
        // Caustics
        preset.causticsStrength = 0.4f;
        preset.causticsScale = 0.08f;
        preset.causticsSpeed = 0.15f;
        
        // No flow for ocean
        preset.useFlowMap = false;
        preset.flowSpeed = 0f;
        preset.flowStrength = 0f;
        
        // Subsurface
        preset.subsurfaceColor = new Color(0.1f, 0.35f, 0.3f, 1f);
        preset.subsurfaceStrength = 0.6f;
    }
    
    /// <summary>
    /// Configure preset for river water - flowing, turbulent
    /// </summary>
    public static void ConfigureRiver(WaterPreset preset)
    {
        preset.waterType = WaterType.River;
        
        // Colors - clearer, greenish
        preset.shallowColor = new Color(0.25f, 0.5f, 0.45f, 1f);
        preset.deepColor = new Color(0.08f, 0.2f, 0.18f, 1f);
        preset.foamColor = Color.white;
        preset.transparency = 0.75f;
        
        // Primary waves - short, choppy
        preset.primaryWave1 = new WaveSettings(0f, 3f, 0.2f, 1.5f);
        preset.primaryWave2 = new WaveSettings(15f, 2.5f, 0.18f, 1.6f);
        preset.primaryWave3 = new WaveSettings(-10f, 2f, 0.15f, 1.4f);
        preset.primaryWave4 = new WaveSettings(25f, 1.5f, 0.12f, 1.8f);
        
        // Secondary waves - small ripples
        preset.secondaryWave1 = new WaveSettings(5f, 0.8f, 0.08f, 2f);
        preset.secondaryWave2 = new WaveSettings(-8f, 0.6f, 0.06f, 2.2f);
        
        preset.waveScale = 0.6f;
        preset.waveSpeed = 1.3f;
        
        // Normals - faster, more turbulent
        preset.normalStrength = 0.9f;
        preset.normalTiling1 = new Vector2(15f, 15f);
        preset.normalTiling2 = new Vector2(20f, 20f);
        preset.normalSpeed1 = new Vector2(0.08f, 0.02f);
        preset.normalSpeed2 = new Vector2(0.06f, -0.01f);
        
        // Depth - shallower
        preset.depthFadeDistance = 5f;
        preset.shallowDepth = 1f;
        
        // Refraction & Reflection
        preset.refractionStrength = 0.12f;
        preset.reflectionStrength = 0.3f;
        preset.fresnelPower = 3f;
        
        // Foam - more foam from turbulence
        preset.foamDepthThreshold = 0.8f;
        preset.foamWaveThreshold = 0.2f;
        preset.foamSoftness = 0.25f;
        preset.foamTiling = new Vector2(30f, 30f);
        preset.foamSpeed = 0.8f;
        
        // Caustics - stronger in clear water
        preset.causticsStrength = 0.6f;
        preset.causticsScale = 0.12f;
        preset.causticsSpeed = 0.25f;
        
        // Flow map enabled
        preset.useFlowMap = true;
        preset.flowSpeed = 0.6f;
        preset.flowStrength = 0.7f;
        
        // Subsurface
        preset.subsurfaceColor = new Color(0.15f, 0.4f, 0.25f, 1f);
        preset.subsurfaceStrength = 0.5f;
    }
    
    /// <summary>
    /// Configure preset for lake water - calm, reflective
    /// </summary>
    public static void ConfigureLake(WaterPreset preset)
    {
        preset.waterType = WaterType.Lake;
        
        // Colors - calm blue-green
        preset.shallowColor = new Color(0.2f, 0.45f, 0.4f, 1f);
        preset.deepColor = new Color(0.05f, 0.15f, 0.2f, 1f);
        preset.foamColor = new Color(0.9f, 0.95f, 1f, 1f);
        preset.transparency = 0.9f;
        
        // Primary waves - very gentle
        preset.primaryWave1 = new WaveSettings(0f, 8f, 0.08f, 0.6f);
        preset.primaryWave2 = new WaveSettings(45f, 6f, 0.06f, 0.7f);
        preset.primaryWave3 = new WaveSettings(-30f, 5f, 0.05f, 0.55f);
        preset.primaryWave4 = new WaveSettings(60f, 4f, 0.04f, 0.65f);
        
        // Secondary waves - subtle
        preset.secondaryWave1 = new WaveSettings(20f, 2f, 0.03f, 0.8f);
        preset.secondaryWave2 = new WaveSettings(-25f, 1.5f, 0.02f, 0.9f);
        
        preset.waveScale = 0.4f;
        preset.waveSpeed = 0.6f;
        
        // Normals - gentle
        preset.normalStrength = 0.6f;
        preset.normalTiling1 = new Vector2(12f, 12f);
        preset.normalTiling2 = new Vector2(18f, 18f);
        preset.normalSpeed1 = new Vector2(0.015f, 0.01f);
        preset.normalSpeed2 = new Vector2(-0.01f, 0.008f);
        
        // Depth
        preset.depthFadeDistance = 12f;
        preset.shallowDepth = 2.5f;
        
        // Refraction & Reflection - more reflective
        preset.refractionStrength = 0.06f;
        preset.reflectionStrength = 0.75f;
        preset.fresnelPower = 5f;
        
        // Foam - minimal
        preset.foamDepthThreshold = 0.5f;
        preset.foamWaveThreshold = 0.6f;
        preset.foamSoftness = 0.4f;
        preset.foamTiling = new Vector2(20f, 20f);
        preset.foamSpeed = 0.2f;
        
        // Caustics
        preset.causticsStrength = 0.5f;
        preset.causticsScale = 0.1f;
        preset.causticsSpeed = 0.1f;
        
        // No flow
        preset.useFlowMap = false;
        preset.flowSpeed = 0f;
        preset.flowStrength = 0f;
        
        // Subsurface
        preset.subsurfaceColor = new Color(0.12f, 0.35f, 0.28f, 1f);
        preset.subsurfaceStrength = 0.4f;
    }
    
    /// <summary>
    /// Configure preset for small pond - very still, clear
    /// </summary>
    public static void ConfigurePond(WaterPreset preset)
    {
        preset.waterType = WaterType.Pond;
        
        // Colors - murky green-brown
        preset.shallowColor = new Color(0.25f, 0.35f, 0.25f, 1f);
        preset.deepColor = new Color(0.1f, 0.15f, 0.1f, 1f);
        preset.foamColor = new Color(0.85f, 0.9f, 0.8f, 1f);
        preset.transparency = 0.7f;
        
        // Primary waves - barely visible
        preset.primaryWave1 = new WaveSettings(0f, 4f, 0.02f, 0.3f);
        preset.primaryWave2 = new WaveSettings(90f, 3f, 0.015f, 0.35f);
        preset.primaryWave3 = new WaveSettings(45f, 2.5f, 0.01f, 0.25f);
        preset.primaryWave4 = new WaveSettings(-45f, 2f, 0.008f, 0.4f);
        
        // Secondary waves - tiny ripples
        preset.secondaryWave1 = new WaveSettings(30f, 1f, 0.01f, 0.5f);
        preset.secondaryWave2 = new WaveSettings(-60f, 0.8f, 0.008f, 0.6f);
        
        preset.waveScale = 0.2f;
        preset.waveSpeed = 0.3f;
        
        // Normals - very subtle
        preset.normalStrength = 0.4f;
        preset.normalTiling1 = new Vector2(8f, 8f);
        preset.normalTiling2 = new Vector2(12f, 12f);
        preset.normalSpeed1 = new Vector2(0.008f, 0.005f);
        preset.normalSpeed2 = new Vector2(-0.005f, 0.004f);
        
        // Depth - shallow
        preset.depthFadeDistance = 4f;
        preset.shallowDepth = 1f;
        
        // Refraction & Reflection
        preset.refractionStrength = 0.04f;
        preset.reflectionStrength = 0.8f;
        preset.fresnelPower = 5f;
        
        // Foam - almost none
        preset.foamDepthThreshold = 0.3f;
        preset.foamWaveThreshold = 0.8f;
        preset.foamSoftness = 0.5f;
        preset.foamTiling = new Vector2(15f, 15f);
        preset.foamSpeed = 0.1f;
        
        // Caustics - visible in clear spots
        preset.causticsStrength = 0.3f;
        preset.causticsScale = 0.15f;
        preset.causticsSpeed = 0.05f;
        
        // No flow
        preset.useFlowMap = false;
        preset.flowSpeed = 0f;
        preset.flowStrength = 0f;
        
        // Subsurface - murky
        preset.subsurfaceColor = new Color(0.15f, 0.25f, 0.15f, 1f);
        preset.subsurfaceStrength = 0.3f;
    }
}
