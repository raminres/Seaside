using UnityEngine;

/// <summary>
/// Controls water material properties at runtime.
/// Attach to water plane/mesh and assign the water material.
/// </summary>
[ExecuteAlways]
public class WaterController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Material _waterMaterial;
    [SerializeField] private WaterPreset _waterPreset;
    
    [Header("Runtime Override")]
    [SerializeField] private bool _usePreset = true;
    
    [Header("Wave Direction Override")]
    [SerializeField] private bool _overrideWaveDirection = false;
    [SerializeField] private Transform _waveDirectionTarget;
    
    [Header("Debug")]
    [SerializeField] private bool _debugGizmos = true;
    
    // Shader property IDs (cached for performance)
    private static readonly int _ShallowColor = Shader.PropertyToID("_ShallowColor");
    private static readonly int _DeepColor = Shader.PropertyToID("_DeepColor");
    private static readonly int _FoamColor = Shader.PropertyToID("_FoamColor");
    private static readonly int _Transparency = Shader.PropertyToID("_Transparency");
    
    // Primary waves
    private static readonly int _PrimaryWaveLengths = Shader.PropertyToID("_PrimaryWaveLengths");
    private static readonly int _PrimarySteepnesses = Shader.PropertyToID("_PrimarySteepnesses");
    private static readonly int _PrimarySpeeds = Shader.PropertyToID("_PrimarySpeeds");
    private static readonly int _PrimaryDirection1 = Shader.PropertyToID("_PrimaryDirection1");
    private static readonly int _PrimaryDirection2 = Shader.PropertyToID("_PrimaryDirection2");
    private static readonly int _PrimaryDirection3 = Shader.PropertyToID("_PrimaryDirection3");
    private static readonly int _PrimaryDirection4 = Shader.PropertyToID("_PrimaryDirection4");
    
    // Secondary waves
    private static readonly int _SecondaryWaveLengths = Shader.PropertyToID("_SecondaryWaveLengths");
    private static readonly int _SecondarySteepnesses = Shader.PropertyToID("_SecondarySteepnesses");
    private static readonly int _SecondarySpeeds = Shader.PropertyToID("_SecondarySpeeds");
    private static readonly int _SecondaryDirection1 = Shader.PropertyToID("_SecondaryDirection1");
    private static readonly int _SecondaryDirection2 = Shader.PropertyToID("_SecondaryDirection2");
    
    // Wave globals
    private static readonly int _WaveScale = Shader.PropertyToID("_WaveScale");
    private static readonly int _WaveSpeed = Shader.PropertyToID("_WaveSpeed");
    
    // Normals
    private static readonly int _NormalMap1 = Shader.PropertyToID("_NormalMap1");
    private static readonly int _NormalMap2 = Shader.PropertyToID("_NormalMap2");
    private static readonly int _NormalStrength = Shader.PropertyToID("_NormalStrength");
    private static readonly int _NormalTiling1 = Shader.PropertyToID("_NormalTiling1");
    private static readonly int _NormalTiling2 = Shader.PropertyToID("_NormalTiling2");
    private static readonly int _NormalSpeed1 = Shader.PropertyToID("_NormalSpeed1");
    private static readonly int _NormalSpeed2 = Shader.PropertyToID("_NormalSpeed2");
    
    // Depth
    private static readonly int _DepthFadeDistance = Shader.PropertyToID("_DepthFadeDistance");
    private static readonly int _ShallowDepth = Shader.PropertyToID("_ShallowDepth");
    
    // Refraction & Reflection
    private static readonly int _RefractionStrength = Shader.PropertyToID("_RefractionStrength");
    private static readonly int _ReflectionStrength = Shader.PropertyToID("_ReflectionStrength");
    private static readonly int _FresnelPower = Shader.PropertyToID("_FresnelPower");
    
    // Foam
    private static readonly int _FoamTexture = Shader.PropertyToID("_FoamTexture");
    private static readonly int _FoamDepthThreshold = Shader.PropertyToID("_FoamDepthThreshold");
    private static readonly int _FoamWaveThreshold = Shader.PropertyToID("_FoamWaveThreshold");
    private static readonly int _FoamSoftness = Shader.PropertyToID("_FoamSoftness");
    private static readonly int _FoamTiling = Shader.PropertyToID("_FoamTiling");
    private static readonly int _FoamSpeed = Shader.PropertyToID("_FoamSpeed");
    
    // Caustics
    private static readonly int _CausticsTexture = Shader.PropertyToID("_CausticsTexture");
    private static readonly int _CausticsStrength = Shader.PropertyToID("_CausticsStrength");
    private static readonly int _CausticsScale = Shader.PropertyToID("_CausticsScale");
    private static readonly int _CausticsSpeed = Shader.PropertyToID("_CausticsSpeed");
    
    // Flow
    private static readonly int _UseFlowMap = Shader.PropertyToID("_UseFlowMap");
    private static readonly int _FlowMap = Shader.PropertyToID("_FlowMap");
    private static readonly int _FlowSpeed = Shader.PropertyToID("_FlowSpeed");
    private static readonly int _FlowStrength = Shader.PropertyToID("_FlowStrength");
    
    // Subsurface
    private static readonly int _SubsurfaceColor = Shader.PropertyToID("_SubsurfaceColor");
    private static readonly int _SubsurfaceStrength = Shader.PropertyToID("_SubsurfaceStrength");
    
    private MaterialPropertyBlock _propertyBlock;
    private Renderer _renderer;
    
    public WaterPreset WaterPreset
    {
        get => _waterPreset;
        set
        {
            _waterPreset = value;
            ApplyPreset();
        }
    }
    
    private void OnEnable()
    {
        _renderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();
        
        if (_waterMaterial == null && _renderer != null)
        {
            _waterMaterial = _renderer.sharedMaterial;
        }
        
        ApplyPreset();
    }
    
    private void Update()
    {
        if (_usePreset && _waterPreset != null)
        {
            // Update wave direction if using target
            if (_overrideWaveDirection && _waveDirectionTarget != null)
            {
                UpdateWaveDirectionFromTarget();
            }
        }
    }
    
    private void OnValidate()
    {
        if (_usePreset && _waterPreset != null)
        {
            ApplyPreset();
        }
    }
    
    /// <summary>
    /// Apply all preset values to the water material
    /// </summary>
    public void ApplyPreset()
    {
        if (_waterMaterial == null || _waterPreset == null) return;
        
        // Colors
        _waterMaterial.SetColor(_ShallowColor, _waterPreset.shallowColor);
        _waterMaterial.SetColor(_DeepColor, _waterPreset.deepColor);
        _waterMaterial.SetColor(_FoamColor, _waterPreset.foamColor);
        _waterMaterial.SetFloat(_Transparency, _waterPreset.transparency);
        
        // Primary waves
        _waterMaterial.SetVector(_PrimaryWaveLengths, new Vector4(
            _waterPreset.primaryWave1.wavelength,
            _waterPreset.primaryWave2.wavelength,
            _waterPreset.primaryWave3.wavelength,
            _waterPreset.primaryWave4.wavelength
        ));
        _waterMaterial.SetVector(_PrimarySteepnesses, new Vector4(
            _waterPreset.primaryWave1.steepness,
            _waterPreset.primaryWave2.steepness,
            _waterPreset.primaryWave3.steepness,
            _waterPreset.primaryWave4.steepness
        ));
        _waterMaterial.SetVector(_PrimarySpeeds, new Vector4(
            _waterPreset.primaryWave1.speed,
            _waterPreset.primaryWave2.speed,
            _waterPreset.primaryWave3.speed,
            _waterPreset.primaryWave4.speed
        ));
        
        Vector2[] primaryDirs = _waterPreset.GetPrimaryWaveDirections();
        _waterMaterial.SetVector(_PrimaryDirection1, primaryDirs[0]);
        _waterMaterial.SetVector(_PrimaryDirection2, primaryDirs[1]);
        _waterMaterial.SetVector(_PrimaryDirection3, primaryDirs[2]);
        _waterMaterial.SetVector(_PrimaryDirection4, primaryDirs[3]);
        
        // Secondary waves
        _waterMaterial.SetVector(_SecondaryWaveLengths, new Vector2(
            _waterPreset.secondaryWave1.wavelength,
            _waterPreset.secondaryWave2.wavelength
        ));
        _waterMaterial.SetVector(_SecondarySteepnesses, new Vector2(
            _waterPreset.secondaryWave1.steepness,
            _waterPreset.secondaryWave2.steepness
        ));
        _waterMaterial.SetVector(_SecondarySpeeds, new Vector2(
            _waterPreset.secondaryWave1.speed,
            _waterPreset.secondaryWave2.speed
        ));
        
        Vector2[] secondaryDirs = _waterPreset.GetSecondaryWaveDirections();
        _waterMaterial.SetVector(_SecondaryDirection1, secondaryDirs[0]);
        _waterMaterial.SetVector(_SecondaryDirection2, secondaryDirs[1]);
        
        // Wave globals
        _waterMaterial.SetFloat(_WaveScale, _waterPreset.waveScale);
        _waterMaterial.SetFloat(_WaveSpeed, _waterPreset.waveSpeed);
        
        // Normal maps
        if (_waterPreset.normalMap1 != null)
            _waterMaterial.SetTexture(_NormalMap1, _waterPreset.normalMap1);
        if (_waterPreset.normalMap2 != null)
            _waterMaterial.SetTexture(_NormalMap2, _waterPreset.normalMap2);
        _waterMaterial.SetFloat(_NormalStrength, _waterPreset.normalStrength);
        _waterMaterial.SetVector(_NormalTiling1, _waterPreset.normalTiling1);
        _waterMaterial.SetVector(_NormalTiling2, _waterPreset.normalTiling2);
        _waterMaterial.SetVector(_NormalSpeed1, _waterPreset.normalSpeed1);
        _waterMaterial.SetVector(_NormalSpeed2, _waterPreset.normalSpeed2);
        
        // Depth
        _waterMaterial.SetFloat(_DepthFadeDistance, _waterPreset.depthFadeDistance);
        _waterMaterial.SetFloat(_ShallowDepth, _waterPreset.shallowDepth);
        
        // Refraction & Reflection
        _waterMaterial.SetFloat(_RefractionStrength, _waterPreset.refractionStrength);
        _waterMaterial.SetFloat(_ReflectionStrength, _waterPreset.reflectionStrength);
        _waterMaterial.SetFloat(_FresnelPower, _waterPreset.fresnelPower);
        
        // Foam
        if (_waterPreset.foamTexture != null)
            _waterMaterial.SetTexture(_FoamTexture, _waterPreset.foamTexture);
        _waterMaterial.SetFloat(_FoamDepthThreshold, _waterPreset.foamDepthThreshold);
        _waterMaterial.SetFloat(_FoamWaveThreshold, _waterPreset.foamWaveThreshold);
        _waterMaterial.SetFloat(_FoamSoftness, _waterPreset.foamSoftness);
        _waterMaterial.SetVector(_FoamTiling, _waterPreset.foamTiling);
        _waterMaterial.SetFloat(_FoamSpeed, _waterPreset.foamSpeed);
        
        // Caustics
        if (_waterPreset.causticsTexture != null)
            _waterMaterial.SetTexture(_CausticsTexture, _waterPreset.causticsTexture);
        _waterMaterial.SetFloat(_CausticsStrength, _waterPreset.causticsStrength);
        _waterMaterial.SetFloat(_CausticsScale, _waterPreset.causticsScale);
        _waterMaterial.SetFloat(_CausticsSpeed, _waterPreset.causticsSpeed);
        
        // Flow
        _waterMaterial.SetFloat(_UseFlowMap, _waterPreset.useFlowMap ? 1f : 0f);
        if (_waterPreset.flowMap != null)
            _waterMaterial.SetTexture(_FlowMap, _waterPreset.flowMap);
        _waterMaterial.SetFloat(_FlowSpeed, _waterPreset.flowSpeed);
        _waterMaterial.SetFloat(_FlowStrength, _waterPreset.flowStrength);
        
        // Subsurface
        _waterMaterial.SetColor(_SubsurfaceColor, _waterPreset.subsurfaceColor);
        _waterMaterial.SetFloat(_SubsurfaceStrength, _waterPreset.subsurfaceStrength);
    }
    
    /// <summary>
    /// Update primary wave direction to point toward a target
    /// </summary>
    private void UpdateWaveDirectionFromTarget()
    {
        if (_waterMaterial == null || _waveDirectionTarget == null) return;
        
        Vector3 toTarget = (_waveDirectionTarget.position - transform.position).normalized;
        Vector2 direction = new Vector2(toTarget.x, toTarget.z).normalized;
        
        _waterMaterial.SetVector(_PrimaryDirection1, direction);
    }
    
    /// <summary>
    /// Set water type preset at runtime
    /// </summary>
    public void SetWaterType(WaterPreset preset)
    {
        _waterPreset = preset;
        ApplyPreset();
    }
    
    /// <summary>
    /// Smoothly transition between two presets
    /// </summary>
    public void TransitionToPreset(WaterPreset targetPreset, float duration)
    {
        StartCoroutine(TransitionCoroutine(targetPreset, duration));
    }
    
    private System.Collections.IEnumerator TransitionCoroutine(WaterPreset target, float duration)
    {
        WaterPreset start = _waterPreset;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Lerp colors
            _waterMaterial.SetColor(_ShallowColor, Color.Lerp(start.shallowColor, target.shallowColor, t));
            _waterMaterial.SetColor(_DeepColor, Color.Lerp(start.deepColor, target.deepColor, t));
            
            // Lerp wave scale
            _waterMaterial.SetFloat(_WaveScale, Mathf.Lerp(start.waveScale, target.waveScale, t));
            _waterMaterial.SetFloat(_WaveSpeed, Mathf.Lerp(start.waveSpeed, target.waveSpeed, t));
            
            yield return null;
        }
        
        _waterPreset = target;
        ApplyPreset();
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!_debugGizmos || _waterPreset == null) return;
        
        // Draw wave directions
        Vector3 center = transform.position;
        float arrowLength = 5f;
        
        // Primary wave 1 direction
        Gizmos.color = Color.blue;
        Vector2 dir1 = _waterPreset.primaryWave1.GetDirection();
        Gizmos.DrawRay(center, new Vector3(dir1.x, 0, dir1.y) * arrowLength);
        
        // Primary wave 2 direction
        Gizmos.color = Color.cyan;
        Vector2 dir2 = _waterPreset.primaryWave2.GetDirection();
        Gizmos.DrawRay(center, new Vector3(dir2.x, 0, dir2.y) * arrowLength * 0.8f);
        
        // Wave direction target
        if (_overrideWaveDirection && _waveDirectionTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(center, _waveDirectionTarget.position);
            Gizmos.DrawWireSphere(_waveDirectionTarget.position, 0.5f);
        }
    }
}
