/*

// Add the following directives to your shader for directional and noise support

		#include "Assets/BOXOPHOBIC/Atmospheric Height Fog/Core/Library/AtmosphericHeightFog.cginc"


// Apply Atmospheric Height Fog to transparent shaders like this
// Where finalColor is the shader output color, fogParams.rgb is the fog color and fogParams.a is the fog mask

		float4 fogParams = GetAtmosphericHeightFog(IN.worldPos);
		return ApplyAtmosphericHeightFog(finalColor, fogParams);

*/

#ifndef ATMOSPHERIC_HEIGHT_FOG_INCLUDED
#define ATMOSPHERIC_HEIGHT_FOG_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

half4 AHF_FogColorStart;
half4 AHF_FogColorEnd;
half AHF_FogDistanceStart;
half AHF_FogDistanceEnd;
half AHF_FogDistanceFalloff;
half AHF_FogColorDuo;
half4 AHF_DirectionalColor;
half3 AHF_DirectionalDir;
half AHF_DirectionalIntensity;
half AHF_DirectionalFalloff;
half3 AHF_FogAxisOption;
half AHF_FogHeightEnd;
half AHF_FogHeightStart;
half AHF_FogHeightFalloff;
half AHF_FogLayersMode;
half AHF_NoiseScale;
half3 AHF_NoiseSpeed;
half AHF_NoiseDistanceEnd;
half AHF_NoiseIntensity;
half AHF_FogIntensity;

float4 mod289(float4 x)
{
	return x - floor(x * (1.0 / 289.0)) * 289.0;
}

float4 perm(float4 x)
{
	return mod289(((x * 34.0) + 1.0) * x);
}

float SimpleNoise3D(float3 p)
{
	float3 a = floor(p);
	float3 d = p - a;
	d = d * d * (3.0 - 2.0 * d);
	float4 b = a.xxyy + float4(0.0, 1.0, 0.0, 1.0);
	float4 k1 = perm(b.xyxy);
	float4 k2 = perm(k1.xyxy + b.zzww);
	float4 c = k2 + a.zzzz;
	float4 k3 = perm(c);
	float4 k4 = perm(c + 1.0);
	float4 o1 = frac(k3 * (1.0 / 41.0));
	float4 o2 = frac(k4 * (1.0 / 41.0));
	float4 o3 = o2 * d.z + o1 * (1.0 - d.z);
	float2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);
	return o4.y * d.y + o4.x * (1.0 - d.y);
}

// Returns the fog color and alpha based on world position
float4 GetAtmosphericHeightFog(float3 positionWS)
{
	float4 finalColor;

	float3 WorldPosition = positionWS;

	float3 WorldPosition2_g1 = WorldPosition;
	float temp_output_7_0_g975 = AHF_FogDistanceStart;
	float temp_output_155_0_g1 = saturate(((distance(WorldPosition2_g1, _WorldSpaceCameraPos) - temp_output_7_0_g975) / (AHF_FogDistanceEnd - temp_output_7_0_g975)));
#ifdef AHF_DISABLE_FALLOFF
	float staticSwitch467_g1 = temp_output_155_0_g1;
#else
	float staticSwitch467_g1 = pow(abs(temp_output_155_0_g1), AHF_FogDistanceFalloff);
#endif
	half FogDistanceMask12_g1 = staticSwitch467_g1;
	float3 lerpResult258_g1 = lerp((AHF_FogColorStart).rgb, (AHF_FogColorEnd).rgb, (saturate((FogDistanceMask12_g1 - 0.5)) * AHF_FogColorDuo));
	float3 normalizeResult318_g1 = normalize((WorldPosition2_g1 - _WorldSpaceCameraPos));
	float dotResult145_g1 = dot(normalizeResult318_g1, AHF_DirectionalDir);
	float temp_output_140_0_g1 = ((dotResult145_g1 * 0.5 + 0.5) * AHF_DirectionalIntensity);
#ifdef AHF_DISABLE_FALLOFF
	float staticSwitch470_g1 = temp_output_140_0_g1;
#else
	float staticSwitch470_g1 = pow(abs(temp_output_140_0_g1), AHF_DirectionalFalloff);
#endif
	float DirectionalMask30_g1 = staticSwitch470_g1;
	float3 lerpResult40_g1 = lerp(lerpResult258_g1, (AHF_DirectionalColor).rgb, DirectionalMask30_g1);
#ifdef AHF_DISABLE_DIRECTIONAL
	float3 staticSwitch442_g1 = lerpResult258_g1;
#else
	float3 staticSwitch442_g1 = lerpResult40_g1;
#endif
	float3 temp_output_2_0_g974 = staticSwitch442_g1;
	float3 gammaToLinear3_g974 = FastSRGBToLinear(temp_output_2_0_g974);
#ifdef UNITY_COLORSPACE_GAMMA
	float3 staticSwitch1_g974 = temp_output_2_0_g974;
#else
	float3 staticSwitch1_g974 = gammaToLinear3_g974;
#endif
	half3 Final_Color462_g1 = staticSwitch1_g974;
	half3 AHF_FogAxisOption181_g1 = AHF_FogAxisOption;
	float3 break159_g1 = (WorldPosition2_g1 * AHF_FogAxisOption181_g1);
	float temp_output_7_0_g976 = AHF_FogHeightEnd;
	float temp_output_167_0_g1 = saturate((((break159_g1.x + break159_g1.y + break159_g1.z) - temp_output_7_0_g976) / (AHF_FogHeightStart - temp_output_7_0_g976)));
#ifdef AHF_DISABLE_FALLOFF
	float staticSwitch468_g1 = temp_output_167_0_g1;
#else
	float staticSwitch468_g1 = pow(abs(temp_output_167_0_g1), AHF_FogHeightFalloff);
#endif
	half FogHeightMask16_g1 = staticSwitch468_g1;
	float lerpResult328_g1 = lerp((FogDistanceMask12_g1 * FogHeightMask16_g1), saturate((FogDistanceMask12_g1 + FogHeightMask16_g1)), AHF_FogLayersMode);
	float mulTime204_g1 = _TimeParameters.x * 2.0;
	float3 temp_output_197_0_g1 = ((WorldPosition2_g1 * (1.0 / AHF_NoiseScale)) + (-AHF_NoiseSpeed * mulTime204_g1));
	float3 p1_g977 = temp_output_197_0_g1;
	float localSimpleNoise3D1_g977 = SimpleNoise3D(p1_g977);
	float temp_output_7_0_g979 = AHF_NoiseDistanceEnd;
	half NoiseDistanceMask7_g1 = saturate(((distance(WorldPosition2_g1, _WorldSpaceCameraPos) - temp_output_7_0_g979) / (0.0 - temp_output_7_0_g979)));
	float lerpResult198_g1 = lerp(1.0, (localSimpleNoise3D1_g977 * 0.5 + 0.5), (NoiseDistanceMask7_g1 * AHF_NoiseIntensity));
	half NoiseSimplex3D24_g1 = lerpResult198_g1;
#ifdef AHF_DISABLE_NOISE3D
	float staticSwitch42_g1 = lerpResult328_g1;
#else
	float staticSwitch42_g1 = (lerpResult328_g1 * NoiseSimplex3D24_g1);
#endif
	float temp_output_454_0_g1 = (staticSwitch42_g1 * AHF_FogIntensity);
	half Final_Alpha463_g1 = temp_output_454_0_g1;
	float4 appendResult114_g1 = (float4(Final_Color462_g1, Final_Alpha463_g1));
	float4 appendResult457_g1 = (float4(WorldPosition2_g1, 1.0));
#ifdef AHF_DEBUG_WORLDPOS
	float4 staticSwitch456_g1 = appendResult457_g1;
#else
	float4 staticSwitch456_g1 = appendResult114_g1;
#endif

	float3 Color = (staticSwitch456_g1).xyz;
	float Alpha = (staticSwitch456_g1).w;

	finalColor = float4(Color, Alpha);
	return finalColor;
}

// Applies the fog
float3 ApplyAtmosphericHeightFog(float3 color, float4 fog)
{
	return float3(lerp(color.rgb, fog.rgb, fog.a));
}

float4 ApplyAtmosphericHeightFog(float4 color, float4 fog)
{
	return float4(lerp(color.rgb, fog.rgb, fog.a), color.a);
}

// Shader Graph Support
void GetAtmosphericHeightFog_half(float3 positionWS, out float4 Out)
{
	Out = GetAtmosphericHeightFog(positionWS);
}

void ApplyAtmosphericHeightFog_half(float3 color, float4 fog, out float3 Out)
{
	Out = ApplyAtmosphericHeightFog(color, fog);
}

//void ApplyAtmosphericHeightFog_float(float4 color, float4 fog, out float4 Out)
//{
//	Out = ApplyAtmosphericHeightFog(color, fog);
//}

#endif
