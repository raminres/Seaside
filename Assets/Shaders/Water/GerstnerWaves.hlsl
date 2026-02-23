// GerstnerWaves.hlsl
// Custom HLSL functions for Shader Graph water shader
// Implements Gerstner wave displacement and normal calculation

#ifndef GERSTNER_WAVES_INCLUDED
#define GERSTNER_WAVES_INCLUDED

// Single Gerstner wave calculation
// Returns: xyz = displacement, w = derivative for normal calculation
float4 GerstnerWave(
    float2 position,
    float2 direction,
    float steepness,
    float wavelength,
    float speed,
    float time)
{
    // Normalize direction
    direction = normalize(direction);
    
    // Calculate wave parameters
    float k = 2.0 * 3.14159265 / wavelength;  // Wave number
    float c = sqrt(9.8 / k);                    // Phase speed (deep water approximation)
    float a = steepness / k;                    // Amplitude
    
    // Phase
    float f = k * (dot(direction, position) - c * speed * time);
    
    // Displacement
    float3 displacement;
    displacement.x = direction.x * a * cos(f);
    displacement.y = a * sin(f);
    displacement.z = direction.y * a * cos(f);
    
    // Tangent derivative for normal calculation
    float derivative = k * a * cos(f);
    
    return float4(displacement, derivative);
}

// Sum of 4 Gerstner waves for primary wave layer
void GerstnerWaves4_float(
    float2 Position,
    float Time,
    float4 WaveLengths,      // Wavelength for each of 4 waves
    float4 Steepnesses,      // Steepness (0-1) for each wave
    float4 Speeds,           // Speed multiplier for each wave
    float2 Direction1,
    float2 Direction2,
    float2 Direction3,
    float2 Direction4,
    out float3 Displacement,
    out float3 Normal)
{
    float3 totalDisplacement = float3(0, 0, 0);
    float3 tangent = float3(1, 0, 0);
    float3 binormal = float3(0, 0, 1);
    
    // Wave 1
    float4 wave1 = GerstnerWave(Position, Direction1, Steepnesses.x, WaveLengths.x, Speeds.x, Time);
    totalDisplacement += wave1.xyz;
    tangent.y += Direction1.x * wave1.w;
    binormal.y += Direction1.y * wave1.w;
    
    // Wave 2
    float4 wave2 = GerstnerWave(Position, Direction2, Steepnesses.y, WaveLengths.y, Speeds.y, Time);
    totalDisplacement += wave2.xyz;
    tangent.y += Direction2.x * wave2.w;
    binormal.y += Direction2.y * wave2.w;
    
    // Wave 3
    float4 wave3 = GerstnerWave(Position, Direction3, Steepnesses.z, WaveLengths.z, Speeds.z, Time);
    totalDisplacement += wave3.xyz;
    tangent.y += Direction3.x * wave3.w;
    binormal.y += Direction3.y * wave3.w;
    
    // Wave 4
    float4 wave4 = GerstnerWave(Position, Direction4, Steepnesses.w, WaveLengths.w, Speeds.w, Time);
    totalDisplacement += wave4.xyz;
    tangent.y += Direction4.x * wave4.w;
    binormal.y += Direction4.y * wave4.w;
    
    Displacement = totalDisplacement;
    Normal = normalize(cross(binormal, tangent));
}

// Simplified version with 2 waves (for secondary/detail layer)
void GerstnerWaves2_float(
    float2 Position,
    float Time,
    float2 WaveLengths,
    float2 Steepnesses,
    float2 Speeds,
    float2 Direction1,
    float2 Direction2,
    out float3 Displacement,
    out float3 Normal)
{
    float3 totalDisplacement = float3(0, 0, 0);
    float3 tangent = float3(1, 0, 0);
    float3 binormal = float3(0, 0, 1);
    
    // Wave 1
    float4 wave1 = GerstnerWave(Position, Direction1, Steepnesses.x, WaveLengths.x, Speeds.x, Time);
    totalDisplacement += wave1.xyz;
    tangent.y += Direction1.x * wave1.w;
    binormal.y += Direction1.y * wave1.w;
    
    // Wave 2
    float4 wave2 = GerstnerWave(Position, Direction2, Steepnesses.y, WaveLengths.y, Speeds.y, Time);
    totalDisplacement += wave2.xyz;
    tangent.y += Direction2.x * wave2.w;
    binormal.y += Direction2.y * wave2.w;
    
    Displacement = totalDisplacement;
    Normal = normalize(cross(binormal, tangent));
}

// Water depth calculation using scene depth
void WaterDepth_float(
    float4 ScreenPosition,
    float3 WorldPosition,
    float DepthFadeDistance,
    out float Depth,
    out float DepthFade)
{
    // Get screen UV
    float2 screenUV = ScreenPosition.xy / ScreenPosition.w;
    
    // Sample scene depth (requires _CameraDepthTexture)
    float sceneDepth = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(screenUV), _ZBufferParams);
    float surfaceDepth = ScreenPosition.w;
    
    // Calculate water depth
    Depth = sceneDepth - surfaceDepth;
    
    // Smooth fade based on depth
    DepthFade = saturate(Depth / DepthFadeDistance);
}

// Fresnel calculation for water surface
void WaterFresnel_float(
    float3 ViewDirection,
    float3 Normal,
    float FresnelPower,
    out float Fresnel)
{
    float NdotV = saturate(dot(Normal, ViewDirection));
    Fresnel = pow(1.0 - NdotV, FresnelPower);
}

// Foam calculation based on depth and wave peaks
void WaterFoam_float(
    float Depth,
    float WaveHeight,
    float FoamDepthThreshold,
    float FoamWaveThreshold,
    float FoamSoftness,
    out float FoamMask)
{
    // Shore foam (based on depth)
    float shoreFoam = 1.0 - saturate(Depth / FoamDepthThreshold);
    shoreFoam = smoothstep(0.0, FoamSoftness, shoreFoam);
    
    // Wave crest foam (based on wave height)
    float waveFoam = saturate((WaveHeight - FoamWaveThreshold) / FoamSoftness);
    
    // Combine
    FoamMask = saturate(shoreFoam + waveFoam);
}

// Caustics projection
void WaterCaustics_float(
    float3 WorldPosition,
    float Time,
    float Scale,
    float Speed,
    float2 Direction,
    out float2 CausticsUV1,
    out float2 CausticsUV2)
{
    // Two layers of caustics moving in slightly different directions
    float2 baseUV = WorldPosition.xz * Scale;
    
    CausticsUV1 = baseUV + Direction * Time * Speed;
    CausticsUV2 = baseUV * 1.3 - Direction * Time * Speed * 0.7;
}

// Refraction offset based on normal
void RefractionOffset_float(
    float3 Normal,
    float Strength,
    float Depth,
    out float2 Offset)
{
    // Reduce refraction in shallow water to avoid artifacts
    float depthFactor = saturate(Depth / 2.0);
    Offset = Normal.xz * Strength * depthFactor;
}

// Flow map support for rivers
void FlowMapUV_float(
    float2 UV,
    float2 FlowDirection,
    float Time,
    float FlowSpeed,
    float FlowPhaseOffset,
    out float2 UV1,
    out float2 UV2,
    out float BlendFactor)
{
    // Create two phases that blend together
    float phase1 = frac(Time * FlowSpeed);
    float phase2 = frac(Time * FlowSpeed + 0.5);
    
    // UV offset based on flow direction
    UV1 = UV - FlowDirection * phase1 * FlowPhaseOffset;
    UV2 = UV - FlowDirection * phase2 * FlowPhaseOffset;
    
    // Blend factor (triangle wave for smooth cycling)
    BlendFactor = abs(2.0 * frac(Time * FlowSpeed) - 1.0);
}

#endif // GERSTNER_WAVES_INCLUDED
