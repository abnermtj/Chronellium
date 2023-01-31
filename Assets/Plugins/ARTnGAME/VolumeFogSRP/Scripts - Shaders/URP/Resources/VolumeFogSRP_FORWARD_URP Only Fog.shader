Shader "Unlit/VolumeFogSRP_FORWARD_URP Only Fog"
{
	Properties
	{
		[HideInInspector]_MainTex("Base (RGB)", 2D) = "white" {}
		_SunThreshold("sun thres", Color) = (0.87, 0.74, 0.65,1)
		_SunColor("sun color", Color) = (1.87, 1.74, 1.65,1)
		_BlurRadius4("blur", Color) = (0.00325, 0.00325, 0,0)
		_SunPosition("sun pos", Color) = (111, 11,339, 11)

			//v0.2 - SHADOWS
			_ExtinctColor("_ExtinctColor", Color) = (0.4, 0.6, 0.7, 1)//
			_ScatterColor("_ScatterColor", Color) = (0.4, 0.6, 0.7, 1)//
			_Brightness("_Brightness", Float) = 1.2//
			_Extinct("_Extinct", Float) = 0.45//
			_Scatter("_Scatter", Float) = 0.45//
			_LightSpread("_LightSpread", Float) = 7//			
			_FogHeight("_FogHeight", Float) = 2
			_RaySamples("_RaySamples", Float) = 7
			blendVolumeLighting("blendVolumeLighting", Float) = 0 
			_stepsControl("_stepsControl", Float) = (0.0, 0.0, 1, 1)// //v1.5
			lightNoiseControl("lightNoiseControl", Float) = (0.0, 0.0, 1, 1)// //v1.5

			//v1.6
			_invertX("Mirror X", Float) = 0
			lightCount("Mirror X", Int) = 3

			//v1.9.9
			lightControlA("Directional - Local powers - Local A,B", Float) = (1, 1, 1, 1)
			lightControlB("Local Lights Power C,D,E,F", Float) = (1, 1, 1, 1)

			//v1.9.9.1
			lightsArrayLength("lightsArrayLength", Int) = 0

			//v1.9.9.3
			shadowsControl("shadowsControl", Float) = (500, 1, 1, 0)

			//1.9.9.4
			volumeSamplingControl("volume Sampling Control", Float) = (1, 1, 1, 1)
		
			//v1.9.9.5 - Ethereal v1.1.8
			_visibleLightsCount("_visible Lights Count", Int) = 1

			//v0.6
			depthDilation("depth Dilation", Float) = 1		  //1
			_TemporalResponse("Temporal Response", Float) = 1 //1
			_TemporalGain("Temporal Gain", Float) = 1		  //1
	}

		HLSLINCLUDE

		

		//#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl" //unity 2018.3 s
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

		//v0.2 - shadows
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

			//FOG URP
//#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
//#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/Colors.hlsl"
#include "ClassicNoise3D.hlsl"
			//END FOG URP

			//v0.2
//#pragma multi_compile _ _SHADOWS_HARD
//#pragma multi_compile _ _SHADOWS_SOFT
#pragma multi_compile _ _ADDITIONAL_LIGHTS
#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
//			// Universal Pipeline keywords
//#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
//#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
//#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
//#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
//

//v0.4a
//#define URP10 ////////// ENABLE IF USING URP 10
//#define URP11 ////////// ENABLE IF USING URP 10 //v1.8 URP11 support

		//#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/SurfaceInput.hlsl"
		//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
		//#include "PostProcessing/Shaders/StdLib.hlsl" //unity 2018.1-2
		//#include "UnityCG.cginc"

	TEXTURE2D(_MainTex);
	TEXTURE2D(_ColorBuffer);
	TEXTURE2D(_Skybox);

	SAMPLER(sampler_MainTex);
	SAMPLER(sampler_ColorBuffer);
	SAMPLER(sampler_Skybox);
	float _Blend;
	
	TEXTURE2D(_CameraDepthTexture);
	SAMPLER(sampler_CameraDepthTexture);
	half4 _CameraDepthTexture_ST;

	half4 _SunThreshold = half4(0.87, 0.74, 0.65, 1);

	half4 _SunColor = half4(0.87, 0.74, 0.65, 1);
	uniform half4 _BlurRadius4 = half4(2.5 / 768, 2.5 / 768, 0.0, 0.0);
	uniform half4 _SunPosition = half4(111, 11, 339, 11);
	uniform half4 _MainTex_TexelSize;

#define SAMPLES_FLOAT 16.0f
#define SAMPLES_INT 16
	
	//v0.6
	float depthDilation;
	float _TemporalResponse;
	float _TemporalGain;

	//v1.9.9.5 - Ethereal v1.1.8
	int _visibleLightsCount;

	//v1.9.9.4
	float4 volumeSamplingControl;
	
	//v1.9.9.3
	float4 shadowsControl;

	//v1.9.9.1
	int lightsArrayLength = 0;
	float4 _LightsArrayPos[32];//position and power
	float4 _LightsArrayDir[32];//direction (0,0,0 for point) and falloff
	float4 _LightsArrayColor[32];

	//v1.9.9
	float4 lightControlA;
	float4 lightControlB;
	float3 lightAcolor;
	float lightAIntensity;
	float lightBIntensity;
	float3 lightBcolor;
	int controlByColor=0;

	//FOG URP /////////////////////
	// Vertex manipulation
	float2 TransformTriangleVertexToUV(float2 vertex)
	{
		float2 uv = (vertex + 1.0) * 0.5;
		return uv;
	}
	///////float4x4 unity_CameraInvProjection; //ALREADY DEFINED in LIBS ABOVE
	//TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	//TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
	TEXTURE2D(_NoiseTex);
	SAMPLER(sampler_NoiseTex);
	//TEXTURE2D_SAMPLER2D(_NoiseTex, sampler_NoiseTex);

	//URP v0.1
	//#pragma multi_compile FOG_LINEAR FOG_EXP FOG_EXP2
	#pragma multi_compile_fog
	#pragma multi_compile _ RADIAL_DIST
	#pragma multi_compile _ USE_SKYBOX

	float _DistanceOffset;
	float _Density;
	float _LinearGrad;
	float _LinearOffs;
	float _Height;
	float _cameraRoll;
	//WORLD RECONSTRUCT	
	//float4x4 _InverseView;
	//float4x4 _camProjection;	////TO REMOVE
	// Fog/skybox information
	half4 _FogColor;
	samplerCUBE _SkyCubemap;
	half4 _SkyCubemap_HDR;
	half4 _SkyTint;
	half _SkyExposure;
	float _SkyRotation;
	float4 _cameraDiff;
	float _cameraTiltSign;
	float _NoiseDensity;
	float _NoiseScale;
	float3 _NoiseSpeed;
	float _NoiseThickness;
	float _OcclusionDrop;
	float _OcclusionExp;
	int noise3D = 0;
	//END FOG URP /////////////////


	/////////////////// MORE FOG URP ///////////////////////////////////////////////////////
	/////////////////// MORE FOG URP ///////////////////////////////////////////////////////
	/////////////////// MORE FOG URP ///////////////////////////////////////////////////////
	// Applies one of standard fog formulas, given fog coordinate (i.e. distance)
	half ComputeFogFactorA(float coord) /// REDFINED, SO CHANGED NAME
	{
		float fog = 0.0;
#if FOG_LINEAR
		// factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
		fog = coord * _LinearGrad + _LinearOffs;
#elif FOG_EXP
		// factor = exp(-density*z)
		fog = _Density * coord;
		fog = exp2(-fog);
#else // FOG_EXP2
		// factor = exp(-(density*z)^2)
		fog = _Density * coord;
		fog = exp2(-fog * fog);
#endif
		return saturate(fog);
	}
	// Distance-based fog
	float ComputeDistance(float3 ray, float depth)
	{
		float dist;
#if RADIAL_DIST
		dist = length(ray * depth);
#else
		dist = depth * _ProjectionParams.z;
#endif
		// Built-in fog starts at near plane, so match that by
		// subtracting the near value. Not a perfect approximation
		// if near plane is very large, but good enough.
		dist -= _ProjectionParams.y;
		return dist;
	}
	////LOCAL LIGHT
	float4 localLightColor;
	float4 localLightPos;
	/////////////////// SCATTER
	bool doDistance;
	bool doHeight;
	// Distance-based fog
	uniform float4 _CameraWS;
	uniform float4x4 _FrustumCornersWS;
	//SM v1.7
	uniform float luminance, Multiplier1, Multiplier2, Multiplier3, bias, lumFac, contrast, turbidity;
	//uniform float mieDirectionalG = 0.7,0.913; 
	float mieDirectionalG;
	float mieCoefficient;//0.054
	float reileigh;
	//SM v1.7
	uniform sampler2D _ColorRamp;
	uniform float _Close;
	uniform float _Far;
	uniform float3 v3LightDir;		// light source
	uniform float FogSky;
	float4 _TintColor; //float3(680E-8, 1550E-8, 3450E-8);
	float4 _TintColorL;
	float4 _TintColorK;
	uniform float ClearSkyFac;
	uniform float4 _HeightParams;
	// x = start distance
	uniform float4 _DistanceParams;

	int4 _SceneFogMode; // x = fog mode, y = use radial flag
	float4 _SceneFogParams;
#ifndef UNITY_APPLY_FOG
	//half4 unity_FogColor; //ALREADY DEFINED !!!!!!!!!!
	half4 unity_FogDensity;
#endif	

	uniform float e = 2.71828182845904523536028747135266249775724709369995957;
	uniform float pi = 3.141592653589793238462643383279502884197169;
	uniform float n = 1.0003;
	uniform float N = 2.545E25;
	uniform float pn = 0.035;
	uniform float3 lambda = float3(680E-9, 550E-9, 450E-9);
	uniform float3 K = float3(0.686, 0.678, 0.666);//const vec3 K = vec3(0.686, 0.678, 0.666);
	uniform float v = 4.0;
	uniform float rayleighZenithLength = 8.4E3;
	uniform float mieZenithLength = 1.25E3;
	uniform float EE = 1000.0;
	uniform float sunAngularDiameterCos = 0.999956676946448443553574619906976478926848692873900859324;
	// 66 arc seconds -> degrees, and the cosine of that
	float cutoffAngle = 3.141592653589793238462643383279502884197169 / 1.95;
	float steepness = 1.5;
	// Linear half-space fog, from https://www.terathon.com/lengyel/Lengyel-UnifiedFog.pdf
	float ComputeHalfSpace(float3 wsDir)
	{
		//float4 _HeightParams = float4(1,1,1,1);
		//wsDir.y = wsDir.y - abs(11.2*_cameraDiff.x);// -0.4;// +abs(11.2*_cameraDiff.x);

		float3 wpos = _CameraWS.xyz + wsDir; // _CameraWS + wsDir;
		float FH = _HeightParams.x;
		float3 C = _CameraWS.xyz;
		float3 V = wsDir;
		float3 P = wpos;
		float3 aV = _HeightParams.w * V;
		float FdotC = _HeightParams.y;
		float k = _HeightParams.z;
		float FdotP = P.y - FH;
		float FdotV = wsDir.y;
		float c1 = k * (FdotP + FdotC);
		float c2 = (1 - 2 * k) * FdotP;
		float g = min(c2, 0.0);
		g = -length(aV) * (c1 - g * g / abs(FdotV + 1.0e-5f));
		return g;
	}

	//SM v1.7
	float3 totalRayleigh(float3 lambda) {
		float pi = 3.141592653589793238462643383279502884197169;
		float n = 1.0003; // refraction of air
		float N = 2.545E25; //molecules per air unit volume 								
		float pn = 0.035;
		return (8.0 * pow(pi, 3.0) * pow(pow(n, 2.0) - 1.0, 2.0) * (6.0 + 3.0 * pn)) / (3.0 * N * pow(lambda, float3(4.0, 4.0, 4.0)) * (6.0 - 7.0 * pn));
	}

	float rayleighPhase(float cosTheta)
	{
		return (3.0 / 4.0) * (1.0 + pow(cosTheta, 2.0));
	}

	float3 totalMie(float3 lambda, float3 K, float T)
	{
		float pi = 3.141592653589793238462643383279502884197169;
		float v = 4.0;
		float c = (0.2 * T) * 10E-18;
		return 0.434 * c * pi * pow((2.0 * pi) / lambda, float3(v - 2.0, v - 2.0, v - 2.0)) * K;
	}

	float hgPhase(float cosTheta, float g)
	{
		float pi = 3.141592653589793238462643383279502884197169;
		return (1.0 / (4.0*pi)) * ((1.0 - pow(g, 2.0)) / pow(abs(1.0 - 2.0*g*cosTheta + pow(g, 2.0)), 1.5));
	}

	float sunIntensity(float zenithAngleCos)
	{
		float cutoffAngle = 3.141592653589793238462643383279502884197169 / 1.95;//pi/
		float steepness = 1.5;
		float EE = 1000.0;
		return EE * max(0.0, 1.0 - exp(-((cutoffAngle - acos(zenithAngleCos)) / steepness)));
	}

	float logLuminance(float3 c)
	{
		return log(c.r * 0.2126 + c.g * 0.7152 + c.b * 0.0722);
	}

	float3 tonemap(float3 HDR)
	{
		float Y = logLuminance(HDR);
		float low = exp(((Y*lumFac + (1.0 - lumFac))*luminance) - bias - contrast / 2.0);
		float high = exp(((Y*lumFac + (1.0 - lumFac))*luminance) - bias + contrast / 2.0);
		float3 ldr = (HDR.rgb - low) / (high - low);
		return float3(ldr);
	}
	/////////////////// END SCATTER
	half _Opacity;
	struct Varyings
	{
		//float2 uv        : TEXCOORD0;
		//float4 vertex : SV_POSITION;
		//UNITY_VERTEX_OUTPUT_STEREO

		float4 position : SV_POSITION;// SV_Position;
		float2 texcoord : TEXCOORD0;
		float3 ray : TEXCOORD1;
		float2 uvFOG : TEXCOORD2;
		float4 interpolatedRay : TEXCOORD3;
		UNITY_VERTEX_OUTPUT_STEREO
	};
	/////////////////// END MORE FOG URP ////////////////////////////////////////////////////
	/////////////////// END MORE FOG URP ////////////////////////////////////////////////////
	/////////////////// END MORE FOG URP ////////////////////////////////////////////////////

	//v0.2 - SHADOWS
	float UVRandom(float2 uv)
	{
		float f = dot(float2(12.9898, 78.233), uv);
		return frac(43758.5453 * sin(f));
	}
	float _Brightness = 1;//
	float _Extinct = 0.5f;//
	float3 _ExtinctColor = float3(0.4, 0.6, 0.7);//
	float _Scatter = 0.5f;//
	float3 _ScatterColor = float3(0.4, 0.6, 0.7);//
	float _LightSpread = 8;//	
	float _FogHeight = 1;
	float _RaySamples = 8;//v1.5
	float blendVolumeLighting = 0;
	float4 _stepsControl;//v1.5
	float4 lightNoiseControl; //v1.5

	float _invertX;//v1.6
	int lightCount = 3; //v1.7

	//v1.9.9.1
	float2 WorldToScreenPos(float3 pos) {
		pos = normalize(pos - _WorldSpaceCameraPos)*(_ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y)) + _WorldSpaceCameraPos;
		float2 uv = 0;
		float3 toCam = mul(unity_WorldToCamera, pos);
		float camPosZ = toCam.z;
		float height = 2 * camPosZ / unity_CameraProjection._m11;
		float width = _ScreenParams.x / _ScreenParams.y * height;
		uv.x = (toCam.x + width / 2) / width;
		uv.y = (toCam.y + height / 2) / height;
		return uv;
	}
	
	float AddDensity(float dense, float scatter, float dist, float3 ray, float3 rayStart)
	{				
		return saturate(dense * (scatter / _FogHeight)  * (1.0 - exp(dist * -ray.y * _FogHeight)) * exp(-_FogHeight * rayStart.y) / (ray.y));
	}
	float3 VolumeFog(float3 sourceImg, float dist, float3 ray, float3 rayStart, float3 WorldPosition, float2 texcoord, float depth)
	{
		float steps = _RaySamples * 5; //v1.5
		float stepLength = dist / steps;
		float3 step = ray * stepLength;

		float3 stepD = step; //step;//v1.1.8f 
		//v1.1.8f - moved fixed step control inside the volumefog function
		if (_stepsControl.x != 0) {//v1.1.8e
			//posToCameraA = (_stepsControl.x + 1) * 50 * (wpos - _WorldSpaceCameraPos) / length(posToCameraA); //v1.1.8e
			stepLength = (_stepsControl.x) * 50 * 0.005 * 1.4;
			//depth = length(ray * stepLength * steps);
			//WorldPosition = length(ray * stepLength * steps);
			//dist = stepLength * steps;
			//WorldPosition = float3(1,1,1)*1000;// _WorldSpaceCameraPos + ray * stepLength * steps;
			//depth = 1;// length(_WorldSpaceCameraPos + ray * stepLength);
			//step = ray * stepLength;
			//stepD =2 * step;
		}

		

		Light directionalLight = GetMainLight();
		half shadowdirectionalLight = GetMainLightShadowStrength();
		ShadowSamplingData shadowSampleData = GetMainLightShadowSamplingData();			

		float3 pos = rayStart + step;
		float ColorE = 0;
		float ColorA = 0;
		
		float lightINT = max(dot(normalize(directionalLight.direction), -ray), 0);
		float3 ColorFOG = lerp(_ScatterColor, directionalLight.color, _LightSpread * pow(lightINT, 7));
		float2 uv = texcoord + _Time.x;
		float attenuate = 0;

		//v1.9.8
		float w = 0.02;
		float o = 0.75;
		float3 coordAlongRay = pos - _WorldSpaceCameraPos
			+ float3(_Time.y * _NoiseSpeed.x*0.1* lightNoiseControl.w,
				_Time.y * _NoiseSpeed.y*0.1* lightNoiseControl.w,
				_Time.y * _NoiseSpeed.z*0.1* lightNoiseControl.w);
		o += 1.5*cnoise(coordAlongRay * 0.17 * lightNoiseControl.z) * w * _stepsControl.w;
		if (_NoiseThickness == 0) {
			o = 1;
		}
		//w *= 0.5;


		//v1.9.9.1 - Light Array
		float3 forward = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));
		for (int i = 0; i < lightsArrayLength; i++)
		{			
			float3 lightPos = _LightsArrayPos[i].xyz;
			float3 lightDir = _LightsArrayDir[i].xyz;
			float lightPower = _LightsArrayPos[i].w;
			float lightFalloff = _LightsArrayDir[i].w;
			float3 lightColor = _LightsArrayColor[i].xyz;

			float3 dir = WorldPosition - lightPos;
			float dist = length(dir);

			float2 pos2d = WorldToScreenPos(lightPos);
			float uvsDist = length(pos2d - texcoord);

			float outOfViewCheck = 1;			
			if (dot(normalize(forward),normalize(lightPos - _WorldSpaceCameraPos)) >= 0) {
				outOfViewCheck = 0;
			}

			//depth test
			float lightPower2 = lightPower;
			if (length(lightPos - _WorldSpaceCameraPos) > length(WorldPosition - _WorldSpaceCameraPos)) { //if behind obstacle, zero intensity
				lightPower2 = 0;
			}

			//if directional spot remove around light
			if (lightDir.x != 0 || lightDir.y != 0 || lightDir.z != 0) {
				if (dot(lightDir, dir) < 0.5 * lightFalloff) { //if behind obstacle, zero intensity
					lightPower2 = 0;
				}
				float2 pos2dEND = WorldToScreenPos(lightPos + lightDir * 2);
				if (dot(normalize(texcoord - pos2d), normalize(pos2dEND - pos2d)) < 0.5 * lightFalloff) { //if behind obstacle, zero intensity
					lightPower2 = 0;
				}
				lightFalloff = 2;
			}

			sourceImg = sourceImg * 1 + lightColor * sourceImg * (1/pow(dist, lightFalloff))*lightPower2
				+ lightColor * sourceImg * (1 / pow(uvsDist, (1/lightFalloff*4.2)))*lightPower2*0.00009 * outOfViewCheck;
		}

		//DIRECTIONAL
		//v1.9.9
		if (lightControlA.x != 0) {

			[loop]
			for (int i = 0; i < steps; ++i)
			{
				float4 coordinates = TransformWorldToShadowCoord(pos);

				attenuate = SampleShadowmap(coordinates, TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture),
					shadowSampleData, shadowdirectionalLight, false);

				//v1.1.6 ETHEREAL
				float divi = 1;
				if (length(pos - _WorldSpaceCameraPos) > length(_WorldSpaceCameraPos - WorldPosition))
				{
					divi = 0; break;
				}

				//v1.9.9.3
				float attn = attenuate;
				if (length(pos - _WorldSpaceCameraPos) > shadowsControl.x)
				{
					attn = 1;
				}
				sourceImg = sourceImg * 1 + o * 0.1*sourceImg * directionalLight.color * attn * lightControlA.x * divi
					- pow(1 - attn, shadowsControl.y * 65) * 0.001 * shadowsControl.z * 0.01;
				//sourceImg = sourceImg * 1 + o * 0.1*sourceImg * directionalLight.color * attenuate * lightControlA.x * divi;
				//sourceImg = sourceImg* pow(attenuate, 0.1) + 0.1*sourceImg * directionalLight.color  * pow(attenuate,2) ;
				//sourceImg = sourceImg * pow(attenuate, 0.04) + 0.0001*directionalLight.color * attenuate;
				//sourceImg = sourceImg * pow(attenuate, 0.04);
				//sourceImg += saturate(_Brightness * (_Scatter / _FogHeight)  * (1.0 - exp(stepLength * -ray.y * _FogHeight)) * exp(-_FogHeight * pos.y) / (ray.y));

				//ColorE += 1000000 * AddDensity(_Brightness, _Extinct * (1 - _ExtinctColor.r), stepLength, ray, pos);
				//ColorA += 0.005* pow(attenuate, 21);// 1000000 * AddDensity(_Brightness, _Scatter * ColorFOG.r * attenuate, stepLength, ray, pos);
				//ColorA += 0.01*saturate(_Brightness * (_Scatter* ColorFOG.r * attenuate / _FogHeight)  * (1.0 - exp(stepLength * -ray.y * _FogHeight)) * exp(-_FogHeight * pos.y) / (ray.y));
				//ColorE += 0.01*saturate(_Brightness * (_Extinct * (1 - _ExtinctColor.r) / _FogHeight)  * (1.0 - exp(stepLength * -ray.y * _FogHeight)) * exp(-_FogHeight * pos.y) / (ray.y));

				float rand = UVRandom(uv + i + 1);
				//pos += (step + step * rand * 0.8);
				pos += (stepD + stepD * rand * 0.8 * volumeSamplingControl.w); //v1.1.8f 
			}
		}
		//int lightCountA = 5; //v1.7

		//SPOT - POINT LIGHTS
		//v1.9.9
		if (lightControlA.y != 0) {
			//https://forum.unity.com/threads/how-to-make-additional-lights-cast-shadows-toon-shader-setup.757730/		
			half distanceAtten = 0;
#ifdef _ADDITIONAL_LIGHTS
			//int pixelLightCount = GetAdditionalLightsCount();
			//https://github.com/Unity-Technologies/Graphics/blob/6de5959b69ae36a22e0745974809353e03665654/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl
			//[unroll]
			//for (uint k = 0u; k < 1; ++k);// for (int k = 0; k < 3; ++k); //for (int k = 0; k < pixelLightCount; ++k)
			{
				float3 stepA = ray * stepLength;
				pos = rayStart + stepA;

				float distToRayStart = length(rayStart - pos);
				//float steps2 = steps - pow(distToRayStart,0.8) * 30;
				//float steps2 = steps + pow(1/distToRayStart, 2) * 30;
				//float steps2 = steps + pow(1 / distToRayStart, 2) * 10;
				float steps2 = steps; //+ 10*pow(1/distToRayStart, 3)*_stepsControl.x; //v1.5
				//steps2 = steps2 - pow(distToRayStart, 7) * 30 * _stepsControl.w;

				float powDIVIDE = (pow(distToRayStart, 0.7*_stepsControl.y)*_stepsControl.z) * lightControlA.y; //v1.9.9

				[loop]
				for (int m = 0; m < steps2; ++m)
				{
					//v1.1.8f
					float lightPower3 = 1;
					if (_stepsControl.x != 0 && length(pos - _WorldSpaceCameraPos) > length(WorldPosition - _WorldSpaceCameraPos)) { //if behind obstacle, zero intensity
						lightPower3 = 0;
					}

					//[loop]
					//for (int k = 0; k < 5; ++k)
					//{
						//LIGHT 1
#ifdef URP10
					Light light = GetAdditionalPerObjectLight(0, pos);// GetAdditionalLight(k, pos, half4(1, 1, 1, 1)); //v0.4 URP 10 need an extra shadowmask variable
					light.shadowAttenuation = AdditionalLightShadow(0u, half4(pos, 1), half4(light.direction, 1), half4(1, 1, 1, 1));
#else
#ifdef URP11//v1.8
					Light light = GetAdditionalPerObjectLight(0u, pos);
					light.shadowAttenuation = AdditionalLightShadow(0u, pos, light.direction, half4(1, 1, 1, 1), half4(0, 0, 0, 0));
#else
					Light light = GetAdditionalPerObjectLight(0, pos);// GetAdditionalLight(k, pos); //v0.4 URP 10 need an extra shadowmask variable
#endif
#endif					
				//LIGHT 1
					float multLightPow = 0.4300346225501;// 0.6805029349687388;// 17088.109200; for 188, for 128  =0.4300346225501
					if (_visibleLightsCount > 1) {//v1.9.9.5
						if (controlByColor == 0 || (controlByColor == 1 &&
							((light.color.r == lightAcolor.x*multLightPow* (lightAIntensity)
								|| light.color.g == lightAcolor.y*multLightPow* (lightAIntensity)
								|| light.color.b == lightAcolor.z*multLightPow* (lightAIntensity)
								)
								//|| (light.color.r == lightBcolor.x*multLightPow* (lightBIntensity))
								)
							)) {
							sourceImg = sourceImg + lightPower3*lightControlA.z * o * 0.04*sourceImg * light.color * light.distanceAttenuation * light.shadowAttenuation / powDIVIDE;//*15
						}
					}
																																								 //}
				//LIGHT 2
					if (lightCount > 1 && _visibleLightsCount > 2) { //v1.9.9.5
#ifdef URP10
						light = GetAdditionalPerObjectLight(1, pos);// GetAdditionalLight(k, pos, half4(1, 1, 1, 1)); //v0.4 URP 10 need an extra shadowmask variable
						light.shadowAttenuation = AdditionalLightShadow(1u, half4(pos, 1), half4(light.direction, 1), half4(1, 1, 1, 1));
#else
#ifdef URP11//v1.8
						//Light light = GetAdditionalLight(0, pos, half4(1, 1, 1, 1));//URP11
						light = GetAdditionalPerObjectLight(1u, pos);
						light.shadowAttenuation = AdditionalLightShadow(1u, pos, light.direction, half4(1, 1, 1, 1), half4(0, 0, 0, 0));
#else
						light = GetAdditionalPerObjectLight(1, pos);// GetAdditionalLight(k, pos); //v0.4 URP 10 need an extra shadowmask variable
#endif
#endif							
						if (controlByColor == 0 || (controlByColor == 1 &&
							((light.color.r == lightAcolor.x*multLightPow* (lightAIntensity)
								|| light.color.g == lightAcolor.y*multLightPow* (lightAIntensity)
								|| light.color.b == lightAcolor.z*multLightPow* (lightAIntensity)
								)
								//|| (light.color.r == lightBcolor.x*multLightPow* (lightBIntensity))
								)
							)) {
							sourceImg = sourceImg + lightPower3 * lightControlA.w * o * 0.04*sourceImg * light.color * light.distanceAttenuation * light.shadowAttenuation / powDIVIDE;
						}
					}
					//LIGHT 3
					if (lightCount > 2 && _visibleLightsCount > 3) {
#ifdef URP10
						light = GetAdditionalPerObjectLight(2, pos);// GetAdditionalLight(k, pos, half4(1, 1, 1, 1)); //v0.4 URP 10 need an extra shadowmask variable
						light.shadowAttenuation = AdditionalLightShadow(2u, half4(pos, 1), half4(light.direction, 1), half4(1, 1, 1, 1));
#else
#ifdef URP11//v1.8
						light = GetAdditionalPerObjectLight(2u, pos);
						light.shadowAttenuation = AdditionalLightShadow(2u, pos, light.direction, half4(1, 1, 1, 1), half4(0, 0, 0, 0));
#else
						light = GetAdditionalPerObjectLight(2, pos);// GetAdditionalLight(k, pos); //v0.4 URP 10 need an extra shadowmask variable
#endif
#endif	
						if (controlByColor == 0 || (controlByColor == 1 &&
							((light.color.r == lightAcolor.x*multLightPow* (lightAIntensity)
								|| light.color.g == lightAcolor.y*multLightPow* (lightAIntensity)
								|| light.color.b == lightAcolor.z*multLightPow* (lightAIntensity)
								)
								//|| (light.color.r == lightBcolor.x*multLightPow* (lightBIntensity))
								)
							)) {
							sourceImg = sourceImg + lightPower3 * lightControlB.x * o * 0.04*sourceImg * light.color * light.distanceAttenuation * light.shadowAttenuation / powDIVIDE;
						}
					}
					//LIGHT 4
					if (lightCount > 3 && _visibleLightsCount > 4) {
#ifdef URP10
						light = GetAdditionalPerObjectLight(3, pos);// GetAdditionalLight(k, pos, half4(1, 1, 1, 1)); //v0.4 URP 10 need an extra shadowmask variable
						light.shadowAttenuation = AdditionalLightShadow(3u, half4(pos, 1), half4(light.direction, 1), half4(1, 1, 1, 1));
#else
#ifdef URP11//v1.8
						light = GetAdditionalPerObjectLight(3u, pos);
						light.shadowAttenuation = AdditionalLightShadow(3u, pos, light.direction, half4(1, 1, 1, 1), half4(0, 0, 0, 0));
#else
						light = GetAdditionalPerObjectLight(3, pos);// GetAdditionalLight(k, pos); //v0.4 URP 10 need an extra shadowmask variable
#endif
#endif	
						if (controlByColor == 0 || (controlByColor == 1 &&
							((light.color.r == lightAcolor.x*multLightPow* (lightAIntensity)
								|| light.color.g == lightAcolor.y*multLightPow* (lightAIntensity)
								|| light.color.b == lightAcolor.z*multLightPow* (lightAIntensity)
								)
								//|| (light.color.r == lightBcolor.x*multLightPow* (lightBIntensity))
								)
							)) {
							sourceImg = sourceImg + lightPower3 * lightControlB.y * o * 0.04*sourceImg * light.color * light.distanceAttenuation * light.shadowAttenuation / powDIVIDE;
						}
					}

					if (lightCount > 4 && _visibleLightsCount > 5) {
						//LIGHT 5
#ifdef URP10
						light = GetAdditionalPerObjectLight(4, pos);// GetAdditionalLight(k, pos, half4(1, 1, 1, 1)); //v0.4 URP 10 need an extra shadowmask variable
						light.shadowAttenuation = AdditionalLightShadow(4u, half4(pos, 1), half4(light.direction, 1), half4(1, 1, 1, 1));
#else
#ifdef URP11//v1.8
						light = GetAdditionalPerObjectLight(4u, pos);
						light.shadowAttenuation = AdditionalLightShadow(4u, pos, light.direction, half4(1, 1, 1, 1), half4(0, 0, 0, 0));
#else
						light = GetAdditionalPerObjectLight(4, pos);// GetAdditionalLight(k, pos); //v0.4 URP 10 need an extra shadowmask variable
#endif
#endif	
						if (controlByColor == 0 || (controlByColor == 1 &&
							((light.color.r == lightAcolor.x*multLightPow* (lightAIntensity)
								|| light.color.g == lightAcolor.y*multLightPow* (lightAIntensity)
								|| light.color.b == lightAcolor.z*multLightPow* (lightAIntensity)
								)
								//|| (light.color.r == lightBcolor.x*multLightPow* (lightBIntensity))
								)
							)) {
							sourceImg = sourceImg + lightPower3 * lightControlB.z * o * 0.04*sourceImg * light.color * light.distanceAttenuation * light.shadowAttenuation / powDIVIDE;
						}
					}
					if (lightCount > 5 && _visibleLightsCount > 6) {
						//LIGHT 5
#ifdef URP10
						light = GetAdditionalPerObjectLight(5, pos);// GetAdditionalLight(k, pos, half4(1, 1, 1, 1)); //v0.4 URP 10 need an extra shadowmask variable
						light.shadowAttenuation = AdditionalLightShadow(5u, half4(pos, 1), half4(light.direction, 1), half4(1, 1, 1, 1));
#else
#ifdef URP11//v1.8
						light = GetAdditionalPerObjectLight(5u, pos);
						light.shadowAttenuation = AdditionalLightShadow(5u, pos, light.direction, half4(1, 1, 1, 1), half4(0, 0, 0, 0));
#else
						light = GetAdditionalPerObjectLight(5, pos);// GetAdditionalLight(k, pos); //v0.4 URP 10 need an extra shadowmask variable
#endif
#endif	
						if (controlByColor == 0 || (controlByColor == 1 &&
							((light.color.r == lightAcolor.x*multLightPow* (lightAIntensity)
								|| light.color.g == lightAcolor.y*multLightPow* (lightAIntensity)
								|| light.color.b == lightAcolor.z*multLightPow* (lightAIntensity)
								)
								//|| (light.color.r == lightBcolor.x*multLightPow* (lightBIntensity))
								)
							)) {
							sourceImg = sourceImg + lightPower3 * lightControlB.w * o * 0.04*sourceImg * light.color * light.distanceAttenuation * light.shadowAttenuation / powDIVIDE;
						}
					}
					//FINAL STEPS
					//float rand = UVRandom(uv + m + 1); //v1.9.9.4

					//find if point light
					//half4 spotDirection = _AdditionalLightsSpotDir[0];
					//if (spotDirection.x == 0) {

					//v1.9.9.4 - volumeSamplingControl
					float rand = volumeSamplingControl.y * 0.1 * (1 - volumeSamplingControl.x) 
						+ volumeSamplingControl.z * UVRandom(uv + m + 1) * (volumeSamplingControl.x);

					pos += stepA + stepA * rand * 0.8;
				}
			}

			//		//2ond light
			//		{
			//			float3 stepA = ray * stepLength;
			//			pos = rayStart + stepA;
			//
			//			float distToRayStart = length(rayStart - pos);
			//			//float steps2 = steps - pow(distToRayStart,0.8) * 30;
			//			//float steps2 = steps + pow(1/distToRayStart, 2) * 30;
			//			//float steps2 = steps + pow(1 / distToRayStart, 2) * 10;
			//			float steps2 = steps + 10 * pow(1 / distToRayStart, 3)*_stepsControl.x; //v1.5
			//			steps2 = steps2 - pow(distToRayStart, 7) * 30 * _stepsControl.w;
			//
			//			[loop]
			//			for (int m = 0; m < steps2; ++m)
			//			{
			//#ifdef URP10
			//				Light light = GetAdditionalPerObjectLight(1, pos);// GetAdditionalLight(k, pos, half4(1, 1, 1, 1)); //v0.4 URP 10 need an extra shadowmask variable
			//				light.shadowAttenuation = AdditionalLightShadow(1u, half4(pos, 1), half4(light.direction, 1), half4(1, 1, 1, 1));
			//#else
			//	#ifdef URP11//v1.8
			//				//Light light = GetAdditionalLight(1, pos, half4(1, 1, 1, 1));//URP11
			//				Light light = GetAdditionalPerObjectLight(1u, pos);
			//				light.shadowAttenuation = AdditionalLightShadow(1u, pos, light.direction, half4(1, 1, 1, 1), half4(0, 0, 0, 0));
			//	#else
			//				Light light = GetAdditionalPerObjectLight(1, pos);// GetAdditionalLight(k, pos); //v0.4 URP 10 need an extra shadowmask variable
			//	#endif
			//#endif
			//				float distanceAttenA = light.distanceAttenuation;
			//				float attenuateA = light.shadowAttenuation;
			//
			//				//v1.5
			//				//sourceImg = sourceImg + 0.04*sourceImg * light.color * distanceAtten * attenuate;
			//				sourceImg = sourceImg + 0.04*sourceImg * light.color * distanceAttenA * attenuateA / (pow(distToRayStart, 0.7*_stepsControl.y)*_stepsControl.z);//*15
			//
			//				float lightINT = max(dot(normalize(light.direction), -ray), 0);
			//				float3  ColorFOG = lerp(_ScatterColor, light.color, _LightSpread * pow(lightINT, 7));
			//
			//				//ColorE += 10* AddDensity(_Brightness, _Extinct * (1 - _ExtinctColor.r),	stepLength, ray, pos);
			//				//ColorA += 110* AddDensity(_Brightness, _Scatter * ColorFOG.r * distanceAtten * attenuate, stepLength, ray, pos);
			//
			//				float rand = UVRandom(uv + m + 1);
			//				pos += stepA + stepA * rand * 0.8;
			//			}
			//		}
			//
			//
			//		//3rd light
			//		{
			//			float3 stepA = ray * stepLength;
			//			pos = rayStart + stepA;
			//
			//			float distToRayStart = length(rayStart - pos);
			//			//float steps2 = steps - pow(distToRayStart,0.8) * 30;
			//			//float steps2 = steps + pow(1/distToRayStart, 2) * 30;
			//			//float steps2 = steps + pow(1 / distToRayStart, 2) * 10;
			//			float steps2 = steps + 10 * pow(1 / distToRayStart, 3)*_stepsControl.x; //v1.5
			//			steps2 = steps2 - pow(distToRayStart, 7) * 30 * _stepsControl.w;
			//
			//			[loop]
			//			for (int m = 0; m < steps2; ++m)
			//			{
			//#ifdef URP10
			//				Light light = GetAdditionalPerObjectLight(2, pos);// GetAdditionalLight(k, pos, half4(1, 1, 1, 1)); //v0.4 URP 10 need an extra shadowmask variable
			//				light.shadowAttenuation = AdditionalLightShadow(2u, half4(pos, 1), half4(light.direction, 1), half4(1, 1, 1, 1));
			//#else
			//	#ifdef URP11 //v1.8
			//				//Light light = GetAdditionalLight(2, pos, half4(1, 1, 1, 1));//URP11
			//				Light light = GetAdditionalPerObjectLight(2u, pos);
			//				light.shadowAttenuation = AdditionalLightShadow(2u, pos, light.direction, half4(1, 1, 1, 1), half4(0, 0, 0, 0));
			//	#else
			//				Light light = GetAdditionalPerObjectLight(2, pos);// GetAdditionalLight(k, pos); //v0.4 URP 10 need an extra shadowmask variable
			//	#endif
			//#endif
			//				float distanceAttenA = light.distanceAttenuation;
			//				float attenuateA = light.shadowAttenuation;
			//
			//				//v1.5
			//				//sourceImg = sourceImg + 0.04*sourceImg * light.color * distanceAtten * attenuate;
			//				sourceImg = sourceImg + 0.04*sourceImg * light.color * distanceAttenA * attenuateA / (pow(distToRayStart, 0.7*_stepsControl.y)*_stepsControl.z);//*15
			//
			//				float lightINT = max(dot(normalize(light.direction), -ray), 0);
			//				float3  ColorFOG = lerp(_ScatterColor, light.color, _LightSpread * pow(lightINT, 7));
			//
			//				//ColorE += 10* AddDensity(_Brightness, _Extinct * (1 - _ExtinctColor.r),	stepLength, ray, pos);
			//				//ColorA += 110* AddDensity(_Brightness, _Scatter * ColorFOG.r * distanceAtten * attenuate, stepLength, ray, pos);
			//
			//				float rand = UVRandom(uv + m + 1);
			//				pos += stepA + stepA * rand * 0.8;
			//			}
			//		}
			//
			//		//v1.7
			//		if (lightCount > 3) {
			//			//4rth light
			//			{
			//				float3 stepA = ray * stepLength;
			//				pos = rayStart + stepA;
			//
			//				float distToRayStart = length(rayStart - pos);
			//				//float steps2 = steps - pow(distToRayStart,0.8) * 30;
			//				//float steps2 = steps + pow(1/distToRayStart, 2) * 30;
			//				//float steps2 = steps + pow(1 / distToRayStart, 2) * 10;
			//				float steps2 = steps + 10 * pow(1 / distToRayStart, 3)*_stepsControl.x; //v1.5
			//				steps2 = steps2 - pow(distToRayStart, 7) * 30 * _stepsControl.w;
			//
			//				[loop]
			//				for (int m = 0; m < steps2; ++m)
			//				{
			//#ifdef URP10
			//					Light light = GetAdditionalPerObjectLight(3, pos);// GetAdditionalLight(k, pos, half4(1, 1, 1, 1)); //v0.4 URP 10 need an extra shadowmask variable
			//					light.shadowAttenuation = AdditionalLightShadow(3u, half4(pos, 1), half4(light.direction, 1), half4(1, 1, 1, 1));
			//#else
			//#ifdef URP11 //v1.8
			//					//Light light = GetAdditionalLight(2, pos, half4(1, 1, 1, 1));//URP11
			//					Light light = GetAdditionalPerObjectLight(3u, pos);
			//					light.shadowAttenuation = AdditionalLightShadow(3u, pos, light.direction, half4(1, 1, 1, 1), half4(0, 0, 0, 0));
			//#else
			//					Light light = GetAdditionalPerObjectLight(3, pos);// GetAdditionalLight(k, pos); //v0.4 URP 10 need an extra shadowmask variable
			//#endif
			//#endif
			//					float distanceAttenA = light.distanceAttenuation;
			//					float attenuateA = light.shadowAttenuation;
			//
			//					//v1.5
			//					//sourceImg = sourceImg + 0.04*sourceImg * light.color * distanceAtten * attenuate;
			//					sourceImg = sourceImg + 0.04*sourceImg * light.color * distanceAttenA * attenuateA / (pow(distToRayStart, 0.7*_stepsControl.y)*_stepsControl.z);//*15
			//
			//					float lightINT = max(dot(normalize(light.direction), -ray), 0);
			//					float3  ColorFOG = lerp(_ScatterColor, light.color, _LightSpread * pow(lightINT, 7));
			//
			//					//ColorE += 10* AddDensity(_Brightness, _Extinct * (1 - _ExtinctColor.r),	stepLength, ray, pos);
			//					//ColorA += 110* AddDensity(_Brightness, _Scatter * ColorFOG.r * distanceAtten * attenuate, stepLength, ray, pos);
			//
			//					float rand = UVRandom(uv + m + 1);
			//					pos += stepA + stepA * rand * 0.8;
			//				}
			//			}
			//		} //END if (lightCount > 3) {
			//
			//		  //v1.7
			//		if (lightCount > 4) {
			//			//4rth light
			//			{
			//				float3 stepA = ray * stepLength;
			//				pos = rayStart + stepA;
			//
			//				float distToRayStart = length(rayStart - pos);
			//				//float steps2 = steps - pow(distToRayStart,0.8) * 30;
			//				//float steps2 = steps + pow(1/distToRayStart, 2) * 30;
			//				//float steps2 = steps + pow(1 / distToRayStart, 2) * 10;
			//				float steps2 = steps + 10 * pow(1 / distToRayStart, 3)*_stepsControl.x; //v1.5
			//				steps2 = steps2 - pow(distToRayStart, 7) * 30 * _stepsControl.w;
			//
			//				[loop]
			//				for (int m = 0; m < steps2; ++m)
			//				{
			//#ifdef URP10
			//					Light light = GetAdditionalPerObjectLight(4, pos);// GetAdditionalLight(k, pos, half4(1, 1, 1, 1)); //v0.4 URP 10 need an extra shadowmask variable
			//					light.shadowAttenuation = AdditionalLightShadow(4u, half4(pos, 1), half4(light.direction, 1), half4(1, 1, 1, 1));
			//#else
			//#ifdef URP11 //v1.8
			//					//Light light = GetAdditionalLight(2, pos, half4(1, 1, 1, 1));//URP11
			//					Light light = GetAdditionalPerObjectLight(4u, pos);
			//					light.shadowAttenuation = AdditionalLightShadow(4u, pos, light.direction, half4(1, 1, 1, 1), half4(0, 0, 0, 0));
			//#else
			//					Light light = GetAdditionalPerObjectLight(4, pos);// GetAdditionalLight(k, pos); //v0.4 URP 10 need an extra shadowmask variable
			//#endif
			//#endif
			//					float distanceAttenA = light.distanceAttenuation;
			//					float attenuateA = light.shadowAttenuation;
			//
			//					//v1.5
			//					//sourceImg = sourceImg + 0.04*sourceImg * light.color * distanceAtten * attenuate;
			//					sourceImg = sourceImg + 0.04*sourceImg * light.color * distanceAttenA * attenuateA / (pow(distToRayStart, 0.7*_stepsControl.y)*_stepsControl.z);//*15
			//
			//					float lightINT = max(dot(normalize(light.direction), -ray), 0);
			//					float3  ColorFOG = lerp(_ScatterColor, light.color, _LightSpread * pow(lightINT, 7));
			//
			//					//ColorE += 10* AddDensity(_Brightness, _Extinct * (1 - _ExtinctColor.r),	stepLength, ray, pos);
			//					//ColorA += 110* AddDensity(_Brightness, _Scatter * ColorFOG.r * distanceAtten * attenuate, stepLength, ray, pos);
			//
			//					float rand = UVRandom(uv + m + 1);
			//					pos += stepA + stepA * rand * 0.8;
			//				}
			//			}
			//		} //END if (lightCount > 3) {

#endif
		}
		//return sourceImg - sourceImg*ColorE + ColorA;
		return sourceImg;
	}
	//END v0.2 SHADOWS



	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
//#if UNITY_UV_STARTS_AT_TOP
		float2 uv1 : TEXCOORD1;
//#endif		
	};

	struct v2f_radial {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 blurVector : TEXCOORD1;
	};

	//FOR URP - REDEFINED ABOVE
	//struct Varyings
	//{
	//	float2 uv        : TEXCOORD0;
	//	float4 vertex : SV_POSITION;
	//	UNITY_VERTEX_OUTPUT_STEREO
	//};

	float Linear01DepthA(float2 uv)
	{
#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		return SAMPLE_TEXTURE2D_ARRAY(_CameraDepthTexture, sampler_CameraDepthTexture, uv, unity_StereoEyeIndex).r;
#else
		return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
#endif
	}

	float4 FragGrey(v2f i) : SV_Target
	{
		float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
		half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);		
		return color/2 + colorB/2;
	}

	half4 fragScreen(v2f i) : SV_Target{

			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
			
			half4 colorA = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
		#if UNITY_UV_STARTS_AT_TOP
																				
			half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv1.xy);
		#else
																				 
			half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);//v1.1
		#endif
			half4 depthMask = saturate(colorB * _SunColor);
			return  1.0f - (1.0f - colorA) * (1.0f - depthMask);
	}


	half4 fragAdd(v2f i) : SV_Target{

		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		//half4 colorA = tex2D(_MainTex, i.uv.xy);
		half4 colorA = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
#if UNITY_UV_STARTS_AT_TOP
		//half4 colorB = tex2D(_ColorBuffer, i.uv1.xy);
		half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv1.xy);
#else
		//half4 colorB = tex2D(_ColorBuffer, i.uv.xy);
		half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);
#endif
		half4 depthMask = saturate(colorB * _SunColor);
		return 1 * colorA + depthMask;
	}

	struct Attributes
	{
		float4 positionOS       : POSITION;
		float2 uv               : TEXCOORD0;
	};

	v2f vert(Attributes v) {//v2f vert(AttributesDefault v) { //appdata_img v) {
							//v2f o;
		v2f o = (v2f)0;
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		
		VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);		

		o.pos = float4(vertexInput.positionCS.xyz, 1.0);
		float2 uv = v.uv;
		o.uv = uv;
		o.uv1 = uv.xy;

		return o;
	}

	v2f_radial vert_radial(Attributes v) {
	
		v2f_radial o = (v2f_radial)0;
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);		

		VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
		o.pos = float4(vertexInput.positionCS.xyz, 1.0);
		float2 uv = v.uv;		

		//#if UNITY_UV_STARTS_AT_TOP
				//uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
		//#endif

		o.uv.xy = uv;
		o.blurVector = (_SunPosition.xy - uv.xy) * _BlurRadius4.xy;

		return o;
	}

	half4 frag_radial(v2f_radial i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		half4 color = half4(0,0,0,0);
		for (int j = 0; j < SAMPLES_INT; j++)
		{			
			half4 tmpColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
			color += tmpColor;
			i.uv.xy += i.blurVector;
		}
		return color / SAMPLES_FLOAT;
	}

	half TransformColor(half4 skyboxValue) {
		return dot(max(skyboxValue.rgb - _SunThreshold.rgb, half3(0, 0, 0)), half3(1, 1, 1)); // threshold and convert to greyscale
	}

	//v1.9.9.2
	//half4 frag_depth(v2f i) : SV_Target{

	//	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

	//#if UNITY_UV_STARTS_AT_TOP			
	//		float depthSample = Linear01DepthA(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv1.xy));
	//#else			
	//		float depthSample = Linear01DepthA(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv.xy));
	//#endif
	//	
	//	half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
	//	//float depthSample = Linear01Depth(depthSample, _ZBufferParams);
	//	depthSample = Linear01Depth(depthSample, _ZBufferParams);

	//	// consider maximum radius
	//#if UNITY_UV_STARTS_AT_TOP
	//	half2 vec = _SunPosition.xy - i.uv1.xy;
	//#else
	//	half2 vec = _SunPosition.xy - i.uv.xy;
	//#endif
	//	half dist = saturate(_SunPosition.w - length(vec.xy));

	//	half4 outColor = 0;
	//	
	//	if (depthSample < 0.018) {
	//		outColor = TransformColor(tex) * dist;
	//	}
	//	return outColor * 1;
	//}
	half4 frag_depth(v2f i) : SV_Target{

		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

#if !UNITY_UV_STARTS_AT_TOP
	//float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv1.xy);
	float depthSample = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv.xy), _ZBufferParams); //v0.1 URP i.uv1.xy
#else
	//float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);
	float depthSample = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv.xy), _ZBufferParams);
#endif

	//half4 tex = tex2D(_MainTex, i.uv.xy);
	half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
	//depthSample = Linear01Depth(depthSample, _ZBufferParams);

	// consider maximum radius
#if !UNITY_UV_STARTS_AT_TOP
	half2 vec = _SunPosition.xy - i.uv.xy; //i.uv1.xy;
#else
	half2 vec = _SunPosition.xy - i.uv.xy;
#endif
	half dist = saturate(_SunPosition.w - length(vec.xy));

	half4 outColor = 0;

	// consider shafts blockers
	//if (depthSample > 0.99)
	//if (depthSample > 0.103)
	if (depthSample > 1 - 0.018) {//if (depthSample < 0.018) {
								  //outColor = TransformColor(tex) * dist;
	}

#if !UNITY_UV_STARTS_AT_TOP
	if (depthSample < 0.018) {
		outColor = TransformColor(tex) * dist;
	}
#else
	if (depthSample > 1 - 0.018) {
		outColor = TransformColor(tex) * dist;
	}
#endif

	return outColor * 1;
	}

	//inline half Luminance(half3 rgb)
	//{
		//return dot(rgb, unity_ColorSpaceLuminance.rgb);
	//	return dot(rgb, rgb);
	//}

	half4 frag_nodepth(v2f i) : SV_Target{

		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

	#if UNITY_UV_STARTS_AT_TOP
			//float4 sky = (tex2D(_Skybox, i.uv1.xy));
			float4 sky = SAMPLE_TEXTURE2D(_Skybox, sampler_Skybox, i.uv1.xy);
	#else
			//float4 sky = (tex2D(_Skybox, i.uv.xy));
			float4 sky = SAMPLE_TEXTURE2D(_Skybox, sampler_Skybox, i.uv.xy);
	#endif

			//float4 tex = (tex2D(_MainTex, i.uv.xy));
			half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
			//sky = float4(0.3, 0.05, 0.05,  1);
			/// consider maximum radius
	#if UNITY_UV_STARTS_AT_TOP
			half2 vec = _SunPosition.xy - i.uv1.xy;
	#else
			half2 vec = _SunPosition.xy - i.uv.xy;
	#endif
			half dist = saturate(_SunPosition.w - length(vec));

			half4 outColor = 0;

			// find unoccluded sky pixels
			// consider pixel values that differ significantly between framebuffer and sky-only buffer as occluded

			if (Luminance(abs(sky.rgb - tex.rgb)) < 0.2) {
				outColor = TransformColor(tex) * dist;
				//outColor = TransformColor(sky) * dist;
			}

			return outColor * 1;
	}


		/////// FOG URP //////////////////////////
		/////// FOG URP //////////////////////////
		/////// FOG URP //////////////////////////
		// Vertex shader that procedurally outputs a full screen triangle
	Varyings Vertex(uint vertexID : SV_VertexID) //Varyings Vertex(Attributes v)
	{
		//Varyings o = (Varyings)0;
		//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

		//VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);

		//float4 vpos = float4(vertexInput.positionCS.x, vertexInput.positionCS.y, vertexInput.positionCS.z, 1.0);
		//o.position = vpos;

		////o.position.z = 0.1;

		//float2 uv = v.uv;
		//
		//o.texcoord = float2(uv.x,uv.y);
		////o.texcoord = uv.xy;

		//float far = _ProjectionParams.z ;
		//float3 rayPers = -mul(unity_CameraInvProjection, vpos.xyzz * far).xyz;
		//rayPers.y = rayPers.y + 1* abs(_cameraDiff.x * 15111);

		//o.ray = rayPers;//lerp(rayPers, rayOrtho, isOrtho);
		//o.uvFOG = uv.xy;
		//half index = vpos.z;
		//o.interpolatedRay.xyz = _FrustumCornersWS[(int)index] ;// vpos;  // _FrustumCornersWS[(int)index];
		//o.interpolatedRay.w = index;
		//return o;

		// Render settings
		float far = _ProjectionParams.z;
		float2 orthoSize = unity_OrthoParams.xy;
		float isOrtho = _ProjectionParams.w; // 0: perspective, 1: orthographic
											 // Vertex ID -> clip space vertex position
		float x = (vertexID != 1) ? -1 : 3;
		float y = (vertexID == 2) ? -3 : 1;
		float3 vpos = float3(x, y, 1.0);

		//v1.6 - reflections _invertX
		/*int sign = 1;
		if (_invertX == 1) {
			sign = -1;
		}*/

		// Perspective: view space vertex position of the far plane
		//float3 rayPers = mul(unity_CameraInvProjection, vpos.xyzz * far).xyz;
		float3 rayPers = mul(unity_CameraInvProjection, float4(vpos.x, 1 * vpos.y,vpos.z,vpos.z) * far).xyz; //v1.6
		//rayPers.y = rayPers.y - abs(_cameraDiff.x * 15111);

		// Orthographic: view space vertex position
		float3 rayOrtho = float3(orthoSize * vpos.xy, 0);

		//v1.6 - reflections _invertX
		//v1.6 - reflections _invertX
		/*int sign = -1;
		if (_invertX == 1) {
			sign = 1;
		}*/

		Varyings o;
		o.position = float4(vpos.x, -1*vpos.y, 1, 1);//o.position = float4(vpos.x, sign*vpos.y, 1, 1); //v1.6
		o.texcoord = (vpos.xy + 1) / 2;
		o.ray = lerp(rayPers, rayOrtho, isOrtho);

		//MINE
		float3 vA = vpos;
		float deg = _cameraRoll;
		float alpha = deg * 3.14 / 180.0;
		float sina, cosa;
		sincos(alpha, sina, cosa);
		float2x2 m = float2x2(cosa, -sina, sina, cosa);

		float3 tmpV = float3(mul(m, vA.xy), vA.z).xyz;
		float2 uvFOG = TransformTriangleVertexToUV(tmpV.xy);
		o.uvFOG = uvFOG.xy;

		half index = vpos.z;
		o.interpolatedRay.xyz = vpos;  // _FrustumCornersWS[(int)index];
		o.interpolatedRay.w = index;

		return o;
	}

	float3 ComputeViewSpacePosition(Varyings input, float z)
	{
		// Render settings
		float near = _ProjectionParams.y;
		float far = _ProjectionParams.z;
		float isOrtho = unity_OrthoParams.w; // 0: perspective, 1: orthographic
											 // Z buffer sample
											 // Far plane exclusion
#if !defined(EXCLUDE_FAR_PLANE)
		float mask = 1;
#elif defined(UNITY_REVERSED_Z)
		float mask = z > 0;
#else
		float mask = z < 1;
#endif

		// Perspective: view space position = ray * depth
		float lindepth = Linear01DepthA(input.texcoord.xy);
		lindepth = Linear01Depth(lindepth, _ZBufferParams);// Linear01Depth(lindepth);
		float3 vposPers = input.ray * lindepth;

		//if (Linear01DepthA(input.texcoord.xy) ==0) {
		//	vposPers = input.ray;
		//}

		// Orthographic: linear depth (with reverse-Z support)
#if defined(UNITY_REVERSED_Z)
		float depthOrtho = -lerp(far, near, z);
#else
		float depthOrtho = -lerp(near, far, z);
#endif
		
		// Orthographic: view space position
		float3 vposOrtho = float3(input.ray.xy, depthOrtho);

		// Result: view space position
		return lerp(vposPers, vposOrtho, isOrtho) * mask;
	}

	half4 VisualizePosition(Varyings input, float3 pos)
	{
		const float grid = 5;
		const float width = 3;

		pos *= grid;

		// Detect borders with using derivatives.
		float3 fw = fwidth(pos);
		half3 bc = saturate(width - abs(1 - 2 * frac(pos)) / fw);

		// Frequency filter
		half3 f1 = smoothstep(1 / grid, 2 / grid, fw);
		half3 f2 = smoothstep(2 / grid, 4 / grid, fw);
		bc = lerp(lerp(bc, 0.5, f1), 0, f2);

		// Blend with the source color.
		half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord);
		c.rgb = SRGBToLinear(lerp(LinearToSRGB(c.rgb), bc, 0.5));

		return c;
	}

		///////////////// FRAGMENT /////////////////////////////////

	float4x4 _InverseView;

	//v1.9.9.1
	/*float2 WorldToScreenPos(float3 pos) {
		pos = normalize(pos - _WorldSpaceCameraPos)*(_ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y)) + _WorldSpaceCameraPos;
		float2 uv = float2(0, 0);
		float4 toCam = mul(unity_WorldToCamera, float4(pos.xyz, 1));
		float camPosZ = toCam.z;
		float height = 2 * camPosZ / unity_CameraProjection._m11;
		float width = _ScreenParams.x / _ScreenParams.y * height;
		uv.x = (toCam.x + width / 2) / width;
		uv.y = (toCam.y + height / 2) / height;
		return uv;
	}*/

	float2 raySphereIntersect(float3 r0, float3 rd, float3 s0, float sr) {

		float a = dot(rd, rd);
		float3 s0_r0 = r0 - s0;
		float b = 2.0 * dot(rd, s0_r0);
		float c = dot(s0_r0, s0_r0) - (sr * sr);
		float disc = b * b - 4.0 * a* c;
		if (disc < 0.0) {
			return float2(-1.0, -1.0);
		}
		else {
			return float2(-b - sqrt(disc), -b + sqrt(disc)) / (2.0 * a);
		}
	}

	float3x3 rotationMatrix(float3 axis, float angle)
	{
		axis = normalize(axis);
		float s = sin(angle);
		float c = cos(angle);
		float oc = 1.0 - c;

		return float3x3 (oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s,
			oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s,
			oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c);
	}
	float4x4 rotationMatrix4(float3 axis, float angle)
	{
		axis = normalize(axis);
		float s = sin(angle);
		float c = cos(angle);
		float oc = 1.0 - c;

		return float4x4 (oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s, 0.0,
			oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s, 0.0,
			oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c, 0.0,
			0.0, 0.0, 0.0, 1.0);

	}

	//v1.6
	//https://docs.unity3d.com/Packages/com.unity.shadergraph@6.9/manual/Rotate-About-Axis-Node.html
	float3 Unity_RotateAboutAxis_Degrees_float(float3 In, float3 Axis, float Rotation)
	{
		Rotation = radians(Rotation);
		float s = sin(Rotation);
		float c = cos(Rotation);
		float one_minus_c = 1.0 - c;

		Axis = normalize(Axis);
		float3x3 rot_mat =
		{ one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
			one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
			one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
		};
		return mul(rot_mat, In);
	}

	half4 Fragment(Varyings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float3 forward = mul((float3x3)(unity_WorldToCamera), float3(0, 0, 1));
		//float zsample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.texcoord);
		//float zsample = Linear01Depth(input.texcoord.xy); //CORRECT WAY

		float zsample = Linear01DepthA(input.texcoord.xy);
		float depth = Linear01Depth(zsample * (zsample < 1.0), _ZBufferParams);// Linear01Depth(lindepth);
		//float3 vposPers = input.ray * lindepth;

		//v0.6
		if (depthDilation != 0) {
			float depthOffset = 0.00001 * 0.41 * 75 * depthDilation;
			float zsampleA1 = Linear01DepthA(input.texcoord.xy + float2(depthOffset, depthOffset));
			float depthA1 = Linear01Depth(zsampleA1 * (zsampleA1 < 1.0), _ZBufferParams);
			float zsampleA2 = Linear01DepthA(input.texcoord.xy + float2(-depthOffset, -depthOffset));
			float depthA2 = Linear01Depth(zsampleA2 * (zsampleA2 < 1.0), _ZBufferParams);
			float zsampleA3 = Linear01DepthA(input.texcoord.xy + float2(-depthOffset, depthOffset));
			float depthA3 = Linear01Depth(zsampleA3 * (zsampleA3 < 1.0), _ZBufferParams);
			float zsampleA4 = Linear01DepthA(input.texcoord.xy + float2(depthOffset, -depthOffset));
			float depthA4 = Linear01Depth(zsampleA4 * (zsampleA4 < 1.0), _ZBufferParams);
			depth = depth * 1 + (depthA1 + depthA2 + depthA3 + depthA4) / 15;
		}

		float3 vpos = ComputeViewSpacePosition(input, zsample);
		float3 wpos = mul(_InverseView, float4(vpos, 1)).xyz;
		//float depth = Linear01Depth(zsample * (zsample < 1.0));
	//	float depth =  Linear01Depth(zsample * (zsample < 1.0)); //////// CHANGE 2 URP
		//float depth = Linear01DepthA(input.texcoord.xy);
		

		//float depthSample = Linear01Depth(input.texcoord.xy); //CORRECT WAY
		//if (depthSample > 0.00000001) { //affect frontal
		//if (depthSample == 0) { 		//affect background
		//if (depthSample < 0.00000001) {	//affect background
		//if (depth > 0.00000001) {
			//return float4(1, 0, 0, 1);
		//}
		//else {
			//return float4(1, 1, 0, 1);
		//}

		//if (depth ==0) {
			//depth = 1; //EXPOSE BACKGROUND AS FORGROUND TO GET SCATTERING
		//}

		float4 wsDir = depth * float4(input.ray, 1); // input.interpolatedRay;	

		//_CameraWS = float4(85.8, -102.19,-10,1);
		float4 wsPos = (_CameraWS) + wsDir;// _CameraWS + wsDir; //////// CHANGE 1 URP
		//return wsPos;
		

		//return ((wsPos) *0.1);

		///// SCATTER
		//float3 lightDirection = float3(-v3LightDir.x - 0 * _cameraDiff.w * forward.x, -v3LightDir.y - 0 * _cameraDiff.w * forward.y, v3LightDir.z);
		//float3 lightDirection = float3(v3LightDir.x - 0 * _cameraDiff.w * forward.x, -v3LightDir.y - 0 * _cameraDiff.w * forward.y, -v3LightDir.z);

		//v1.6
		//float3 lightDirection = float3(-v3LightDir.x - 0 * _cameraDiff.w * forward.x, -v3LightDir.y - 0 * _cameraDiff.w * forward.y, v3LightDir.z);
		int sign = 1;
		if (_invertX == 1) {
			sign = -1;
		}
		///// SCATTER		
		float3 lightDirection = float3(-v3LightDir.x - 0 * _cameraDiff.w * forward.x, -sign * v3LightDir.y - 0 * _cameraDiff.w * forward.y, v3LightDir.z);
		
		float3 CamLEFT = mul((float3x3)(unity_WorldToCamera), float3(1, 0, 0));
		float3 CamUP = mul((float3x3)(unity_WorldToCamera), float3(0, 1, 0));
		if (_invertX == 1) {
			//lightDirection = reflect((float3(-v3LightDir.x, -v3LightDir.y, v3LightDir.z)), CamUP);// -CamUP);			
			lightDirection = Unity_RotateAboutAxis_Degrees_float(lightDirection, float3(1, 0, 0), _cameraTiltSign * 2 * _cameraDiff.x);			
		}

		float3 pointToCamera = (wpos - _WorldSpaceCameraPos);// * 0.47;
		int steps = 2;
		float stepCount = 1;
		float step = length(pointToCamera) / steps;

		//int noise3D = 0;
		half4 noise;
		half4 noise1;
		half4 noise2;
		if (noise3D == 0) {
			float fixFactor1 = 0;
			float fixFactor2 = 0;
			float dividerScale = 1; //1
			float scaler1 = 1.00; //0.05
			float scaler2 = 0.8; //0.01
			float scaler3 = 0.3; //0.01
			float signer1 = 0.004 / (dividerScale * 1.0);//0.4 !!!! (0.005 for 1) (0.4 for 0.05) //0.004
			float signer2 = 0.004 / (dividerScale * 1.0);//0.001

			if (_cameraDiff.w < 0) {
				fixFactor1 = -signer1 * 90 * 2 * 2210 / 1 * (dividerScale / 1);//2210
				fixFactor2 = -signer2 * 90 * 2 * 2210 / 1 * (dividerScale / 1);
			}
			float hor1 = -_cameraDiff.w * signer1 *_cameraDiff.y * 2210 / 1 * (dividerScale / 1) - 1.2 * _WorldSpaceCameraPos.x * 10 + fixFactor1;
			float hor2 = -_cameraDiff.w * signer2 *_cameraDiff.y * 2210 / 1 * (dividerScale / 1) - 1.2 * _WorldSpaceCameraPos.x * 10 + fixFactor2;
			float hor3 = -_cameraDiff.w * signer2 *_cameraDiff.y * 1210 / 1 * (dividerScale / 1) - 1.2 * _WorldSpaceCameraPos.x * 2 + fixFactor2;

			float vert1 = _cameraTiltSign * _cameraDiff.x * 0.77 * 1.05 * 160 + 0.0157*_cameraDiff.y * (pow((input.texcoord.x - 0.1), 2)) - 0.3 * _WorldSpaceCameraPos.y * 30
				- 2 * 0.33 * _WorldSpaceCameraPos.z * 2.13 + 50 * abs(cos(_WorldSpaceCameraPos.z * 0.01)) + 35 * abs(sin(_WorldSpaceCameraPos.z * 0.005));

			float vert2 = _cameraTiltSign * _cameraDiff.x * 0.20 * 1.05 * 160 + 0.0157*_cameraDiff.y * (pow((input.texcoord.x - 0.1), 2)) - 0.3 * _WorldSpaceCameraPos.y * 30
				- 1 * 0.33 * _WorldSpaceCameraPos.z * 3.24 + 75 * abs(sin(_WorldSpaceCameraPos.z * 0.02)) + 85 * abs(cos(_WorldSpaceCameraPos.z * 0.01));

			float vert3 = _cameraTiltSign * _cameraDiff.x * 0.10 * 1.05 * 70 + 0.0117*_cameraDiff.y * (pow((input.texcoord.x - 0.1), 2)) - 0.3 * _WorldSpaceCameraPos.y * 30
				- 1 * 1.03 * _WorldSpaceCameraPos.z * 3.24 + 75 * abs(sin(_WorldSpaceCameraPos.z * 0.02)) + 85 * abs(cos(_WorldSpaceCameraPos.z * 0.01));

			noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, 1 * (dividerScale * (float2(input.texcoord.x*scaler1 * 1, input.texcoord.y*scaler1))
				+ (-0.001*float2((0.94)*hor1, vert1)) + 3 * abs(cos(_Time.y *1.22* 0.012)))) * 2 * 9;
			noise1 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, 1 * (dividerScale * (input.texcoord.xy*scaler2)
				+ (-0.001*float2((0.94)*hor2, vert2) + 3 * abs(cos(_Time.y *1.22* 0.010))))) * 3 * 9;
			noise2 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, 1 * (dividerScale * (input.texcoord.xy*scaler3)
				+ (-0.001*float2((0.94)*hor3, vert3) + 1 * abs(cos(_Time.y *1.22* 0.006))))) * 3 * 9;
		}
		else {

			/////////// NOISE 3D //////////////
			const float epsilon = 0.0001;

			float2 uv = input.texcoord * 4.0 + float2(0.2, 1) * _Time.y * 0.01;

			/*#if defined(SNOISE_AGRAD) || defined(SNOISE_NGRAD)
			#if defined(THREED)
			float3 o = 0.5;
			#else
			float2 o = 0.5;
			#endif
			#else*/
			float o = 0.5 * 1.5;
			//#endif

			float s = 0.011;

			/*#if defined(SNOISE)
			float w = 0.25;
			#else*/
			float w = 0.02;
			//#endif

			//#ifdef FRACTAL
			
			for (int i = 0; i < 5; i++)
				//#endif
			{
				float3 coord = wpos + float3(_Time.y * 3 * _NoiseSpeed.x,
					_Time.y * _NoiseSpeed.y,
					_Time.y * _NoiseSpeed.z);
				float3 period = float3(s, s, 1.0) * 1111;



				//#if defined(CNOISE)
				o += cnoise(coord * 0.17 * _NoiseScale) * w;

				//float3 pointToCamera = (wpos - _WorldSpaceCameraPos);// * 0.47;
				//int steps = 2;
				//float stepCount = 1;
				//float step = length(pointToCamera) / steps;
				for (int j = 0; j < steps; j++) {
					//ray trace noise												
					float3 coordAlongRay = _WorldSpaceCameraPos + normalize(pointToCamera) * step
						+ float3(_Time.y * 6 * _NoiseSpeed.x,
							_Time.y * _NoiseSpeed.y,
							_Time.y * _NoiseSpeed.z);
					o += 1.5*cnoise(coordAlongRay * 0.17 * _NoiseScale) * w * 1;
					//stepCount++;
					if (depth < 0.99999) {
						o += depth * 45 * _NoiseThickness;
					}
					step = step + step;
				}

				s *= 2.0;
				w *= 0.5;
			}
			noise = float4(o, o, o, 1);
			noise1 = float4(o, o, o, 1);
			noise2 = float4(o, o, o, 1);
		}

		float cosTheta = dot(normalize(wsDir.xyz), lightDirection);
		cosTheta = dot(normalize(wsDir.xyz), -lightDirection);

		float lumChange = clamp(luminance * pow(abs(((1 - depth) / (_OcclusionDrop * 0.1 * 2))), _OcclusionExp), luminance, luminance * 2);
		if (depth <= _OcclusionDrop * 0.1 * 1) {
			luminance = lerp(4 * luminance, 1 * luminance, (0.001 * 1) / (_OcclusionDrop * 0.1 - depth + 0.001));
		}

		float3 up = float3(0.0, 1.0, 0.0); //float3(0.0, 0.0, 1.0);			
		float3 lambda = float3(680E-8 + _TintColorL.r * 0.000001, 550E-8 + _TintColorL.g * 0.000001, 450E-8 + _TintColorL.b * 0.000001);
		float3 K = float3(0.686 + _TintColorK.r * 0.1, 0.678 + _TintColorK.g * 0.1, 0.666 + _TintColorK.b * 0.1);
		float  rayleighZenithLength = 8.4E3;
		float  mieZenithLength = 1.25E3;
		float  pi = 3.141592653589793238462643383279502884197169;
		float3 betaR = totalRayleigh(lambda) * reileigh * 1000;
		float3 lambda1 = float3(_TintColor.r, _TintColor.g, _TintColor.b)* 0.0000001;//  680E-8, 1550E-8, 3450E-8); //0.0001//0.00001
		lambda = lambda1;
		float3 betaM = totalMie(lambda1, K, turbidity * Multiplier2) * mieCoefficient;
		float zenithAngle = acos(max(0.0, dot(up, normalize(lightDirection))));
		float sR = rayleighZenithLength / (cos(zenithAngle) + 0.15 * pow(abs(93.885 - ((zenithAngle * 180.0) / pi)), -1.253));
		float sM = mieZenithLength / (cos(zenithAngle) + 0.15 * pow(abs(93.885 - ((zenithAngle * 180.0) / pi)), -1.253));
		float  rPhase = rayleighPhase(cosTheta*0.5 + 0.5);
		float3 betaRTheta = betaR * rPhase;
		float  mPhase = hgPhase(cosTheta, mieDirectionalG) * Multiplier1;
		float3 betaMTheta = betaM * mPhase;
		float3 Fex = exp(-(betaR * sR + betaM * sM));
		float  sunE = sunIntensity(dot(lightDirection, up));
		float3 Lin = ((betaRTheta + betaMTheta) / (betaR + betaM)) * (1 - Fex) + sunE * Multiplier3*0.0001;
		float  sunsize = 0.0001;
		float3 L0 = 1.5 * Fex + (sunE * 1.0 * Fex)*sunsize;
		float3 FragColor = tonemap(Lin + L0);//tonemap(Lin + L0);
		///// END SCATTER

		///////////////return float4(FragColor,1);

		//occlusion !!!!
		float4 sceneColor = Multiplier3 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord.xy);


		//return sceneColor+ (float4(FragColor, 1));
		//return 0 + (float4(FragColor, 1));


		float3 subtractor = saturate(pow(abs(dot(normalize(input.ray), normalize(lightDirection))),36)) - (float3(1, 1, 1)*depth * 1);
		if (depth < _OcclusionDrop * 0.1) {
			FragColor = saturate(FragColor * pow(abs((depth / (_OcclusionDrop * 0.1))), _OcclusionExp));
		}
		else {
			if (depth < 0.9999) {
				FragColor = saturate(FragColor * pow(abs((depth / (_OcclusionDrop * 0.1))), 0.001));
			}
		}

		
		////return float4(FragColor, 1);


		//SCATTER
		int doHeightA = 1;
		int doDistanceA = 1;
		//float g = ComputeDistance(input.ray, depth) - _DistanceOffset/_WorldSpaceCameraPos.y;
		float g =  ComputeDistance(input.ray, depth) - _DistanceOffset;
		if (doDistanceA == 1) {
			//g += ComputeDistance(input.ray, depth) - (_DistanceOffset - 100*_WorldSpaceCameraPos.y);
			g += ComputeDistance(input.ray, depth) -_DistanceOffset ;
		}
		if (doHeightA == 1) {
			g +=  ComputeHalfSpace(wpos - _WorldSpaceCameraPos); //v1.9.9.5 - Ethereal v1.1.8 - 1.1.8e
			//g += ComputeHalfSpace(wpos*0.5) - _DistanceOffset;
			//g += ComputeHalfSpace(wpos*0.05);
			//float4 wsDir1 = depth * input.interpolatedRay;
			//float3 wpos1 = _WorldSpaceCameraPos + (wsDir1);// +wsDir; // _CameraWS + wsDir;
			//float FH = _HeightParams.x;
			//float3 C = _WorldSpaceCameraPos.xyz;
			//float3 V = (wsDir1);
			//float3 P = wpos1;
			//float3 aV = _HeightParams.w * V			*		1;
			//float FdotC = _HeightParams.y;
			//float k = _HeightParams.z;
			//float FdotP = P.y - FH;
			//float FdotV = (wsDir1).y;
			//float c1 = k * (FdotP + FdotC);
			//float c2 = (1 - 2 * k) * FdotP;
			//float g1 = min(c2, 0.0);
			//g1 = -length(aV) * (c1 - g1 * g1 / abs(FdotV + 1.0e-5f));
			//g += g1 * 1;
		}

		g = g * pow(abs((noise.r + 1 * noise1.r + _NoiseDensity * noise2.r * 1)), 1.2)*0.3;
		
		half fogFac = ComputeFogFactorA(max(0.0, g));
		if (zsample <= 1-0.999995) {
			//if (zsample >= 0.999995) {
			if (FogSky <= 0) {
				fogFac = 1.0* ClearSkyFac;
			}
			else {
				if (doDistanceA) {
					fogFac = fogFac * ClearSkyFac;
				}
			}
		}

		float4 Final_fog_color = lerp(unity_FogColor + float4(FragColor * 1, 1), sceneColor, fogFac);

		//return Final_fog_color;

		float fogHeight = _Height;
		half fog = ComputeFogFactorA(max(0.0, g));

		//local light
		float3 visual = 0;// VisualizePosition(input, wpos);
		if (localLightPos.z != 0) { //v1.9.9.5 - Ethereal v1.1.8

			float3 light1 = localLightPos.xyz;
			float dist1 = length(light1 - wpos);

			float2 screenPos = WorldToScreenPos(light1);
			float lightRadius = localLightColor.w;

			float dist2 = length(screenPos - float2(input.texcoord.x, input.texcoord.y * 0.62 + 0.23));
			if (
				length(_WorldSpaceCameraPos - wpos) < length(_WorldSpaceCameraPos - light1) - lightRadius
				&&
				dot(normalize(_WorldSpaceCameraPos - wpos), normalize(_WorldSpaceCameraPos - light1)) > 0.95// 0.999
				) { //occlusion
			}
			else {
				float factorOcclusionDist = length(_WorldSpaceCameraPos - wpos) - (length(_WorldSpaceCameraPos - light1) - lightRadius);
				float factorOcclusionDot = dot(normalize(_WorldSpaceCameraPos - wpos), normalize(_WorldSpaceCameraPos - light1));

				Final_fog_color = lerp(Final_fog_color,
					Final_fog_color  * (1 - ((11 - dist2) / 11))
					+ Final_fog_color * float4(2 * localLightColor.x, 2 * localLightColor.y, 2 * localLightColor.z, 1)*(11 - dist2) / 11,
					(localLightPos.w * saturate(1 * 0.1458 / pow(dist2, 0.95))
						+ 0.04*saturate(pow(1 - input.uvFOG.y * (1 - fogHeight), 1.0)) - 0.04)
				);
			}
		}

		//return sceneColor/2 + Final_fog_color/2;

//#if USE_SKYBOX
//		// Look up the skybox color.
//		half3 skyColor = DecodeHDR(texCUBE(_SkyCubemap, i.ray), _SkyCubemap_HDR);
//		skyColor *= _SkyTint.rgb * _SkyExposure * unity_ColorSpaceDouble;
//		// Lerp between source color to skybox color with fog amount.
//		return lerp(half4(skyColor, 1), sceneColor, fog);
//#else
		// Lerp between source color to fog color with the fog amount.
		half4 skyColor = lerp(_FogColor, sceneColor, saturate(fog));

		float distToWhite = (Final_fog_color.r - 0.99) + (Final_fog_color.g - 0.99) + (Final_fog_color.b - 0.99);

		//Final_fog_color = Final_fog_color + 0.0*Final_fog_color * float4(8,2,0,1);


		//v0.2 - SHADOWS
		float4 result = Final_fog_color * _FogColor + float4(visual, 0);

		float3 posToCameraA = (wpos - _WorldSpaceCameraPos);

		//v1.1.8f - move inside the volumefog function
		//if (_stepsControl.x != 0) {//v1.1.8e
		//	posToCameraA = (_stepsControl.x + 1) * 50 * (wpos - _WorldSpaceCameraPos) / length(posToCameraA); //v1.1.8e
		//}

		//v1.5
		//if (length(posToCameraA) > 0.9) {
		//	posToCameraA = (wpos - _WorldSpaceCameraPos) * 0.2;
		//}

		//v1.1.8e
		if (zsample <= 1-0.999995 ){//&& _stepsControl.x == 0) { //v1.1.8f
			//float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;
			//wpos = 0 + viewDir * 21;
			posToCameraA = (wpos - _WorldSpaceCameraPos) * 0.1;
		}


//	//	float normalizeMag = length(posToCameraA);
//	//	float4 shadows = half4(VolumeFog(result.rgb, normalizeMag, posToCameraA / normalizeMag, _WorldSpaceCameraPos, wpos, input.texcoord, depth), result.a);
//	//	shadows = shadows * lightNoiseControl.x + lightNoiseControl.y* fogFac * shadows;//shadows = shadows * 0.6 + 0.75* fogFac * shadows;//v1.5
		
																						
																						//END v0.2 SHADOWS

		//v1.9.9.5 - Ethereal v1.1.8
		//return clamp(shadows, 0, 100000) * blendVolumeLighting + result * (1 - blendVolumeLighting);
		return result * (blendVolumeLighting);


//		//return shadows * blendVolumeLighting + result * (1 - blendVolumeLighting);// result;
		//return Final_fog_color;
//#endif					                
	}

	half4 FragmentTWO(Varyings input) : SV_Target
	{
		float z =SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.texcoord.xy);
		float3 vpos = ComputeViewSpacePosition(input,z);
		//vpos.z = vpos.z +11110;
		float3 wpos = mul(_InverseView, float4(vpos, 1)).xyz;
		
	/*	return float4(_InverseView[3][0], _InverseView[3][1], _InverseView[3][2], _InverseView[3][3]);
		return float4(_InverseView[2][0], _InverseView[2][1], _InverseView[2][2], _InverseView[2][3]);
		return float4(_InverseView[1][0], _InverseView[1][1], _InverseView[1][2], _InverseView[1][3]);
		return float4(_InverseView[0][0], _InverseView[0][1], _InverseView[0][2], _InverseView[0][3]);*/
		//return float4(_ProjectionParams.w, _ProjectionParams.w, _ProjectionParams.w, _ProjectionParams.w);//x=0, y=0.5,z=1,w=0
		//return float4(z,z,z,1);//
		//return float4(input.ray, 1);

		//return float4(vpos, 1);
		return VisualizePosition(input, wpos);
	}
		/////// END FOG URP //////////////////////
		/////// END FOG URP //////////////////////
		/////// END FOG URP //////////////////////


		ENDHLSL

		
		Subshader {
		
			Pass{ //0
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

#pragma vertex vert
#pragma fragment fragScreen

			ENDHLSL
		}

			Pass{//1
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

#pragma vertex vert_radial
#pragma fragment frag_radial

			ENDHLSL
		}

			Pass{//2
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

#pragma vertex vert
#pragma fragment frag_depth

			ENDHLSL
		}

			Pass{//3
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

#pragma vertex vert
#pragma fragment frag_nodepth

			ENDHLSL
		}

			Pass{//4
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

#pragma vertex vert
#pragma fragment fragAdd

			ENDHLSL
		}

			//PASS5
			Pass{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

#pragma vertex vert
#pragma fragment FragGrey

			ENDHLSL
		}


				///////////// PASS FOG
				Pass{ //6
				ZTest Always Cull Off ZWrite Off

				HLSLPROGRAM

				#pragma vertex Vertex
				#pragma fragment Fragment

				ENDHLSL
				}
				///////////// END PASS FOG

			//v0.6
			///// TEMPORAL AA
			////////////// TEMPORAL - https://github.com/cdrinmatane/SSRT - MIT LICENSE
				//https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@13.1/manual/renderer-features/how-to-fullscreen-blit-in-xr-spi.html
				Pass // 7
				{
					Name "TemporalReproj"
					HLSLPROGRAM
					#pragma vertex vert
					#pragma fragment frag

					struct AttributesA
					{
						float4 positionHCS   : POSITION;
						float2 uv           : TEXCOORD0;
						UNITY_VERTEX_INPUT_INSTANCE_ID
					};

					struct VaryingsA
					{
						float4  positionCS  : SV_POSITION;
						float2  uv          : TEXCOORD0;
						UNITY_VERTEX_OUTPUT_STEREO
					};

					VaryingsA vert(AttributesA input)
					{
						VaryingsA output;
						UNITY_SETUP_INSTANCE_ID(input);
						UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

						// Note: The pass is setup with a mesh already in clip
						// space, that's why, it's enough to just output vertex
						// positions
						output.positionCS = float4(input.positionHCS.xyz, 1.0);

						#if UNITY_UV_STARTS_AT_TOP
						output.positionCS.y *= -1;
						#endif

						output.uv = input.uv;
						return output;
					}

					//sampler2D _PreviousColor;
					//sampler2D _PreviousDepth;
					TEXTURE2D_X(_PreviousColor);
					SAMPLER(sampler_PreviousColor);
					TEXTURE2D_X(_PreviousDepth);
					SAMPLER(sampler_PreviousDepth);
					//float4 sampler_PreviousColor;

					// Transformation matrices
					float4x4 _CameraToWorldMatrix;
					float4x4 _InverseProjectionMatrix;
					float4x4 _LastFrameViewProjectionMatrix;
					float4x4 _InverseViewProjectionMatrix;
					float4x4 _LastFrameInverseViewProjectionMatrix;
					//float _TemporalResponse;
					/*struct v2f
					{
						float4 pos : SV_POSITION;
						float4 uv : TEXCOORD0;
					};*/
					float LinearEyeDepthB(float z)
					{
						return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
					}
					float4 frag(VaryingsA  input) : SV_Target
					{
						//UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
						//float4 color = SAMPLE_TEXTURE2D(_CloudTex, sampler_CloudTex, input.uv);
						//return color*1;

						float2 uv = input.uv.xy;
						float2 oneOverResolution = (1.0 / _ScreenParams.xy);

						float4 gi = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, input.uv.xy);// tex2D(_MainTex, input.uv.xy);


						//DEBUG
						//float4 giP = SAMPLE_TEXTURE2D(_PreviousColor, sampler_PreviousColor, input.uv.xy);
						//return giP;//

						float4 depthIMG = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.uv.xy);
						float depth = LinearEyeDepthB(depthIMG.x);// depthIMG.x; //LinearEyeDepth(depthIMG.x);
						float4 currentPos = float4(input.uv.x * 2.0 - 1.0, input.uv.y * 2.0 - 1.0, depth * 2.0 - 1.0, 1.0);

						float4 fragpos = mul(_InverseViewProjectionMatrix, float4(float3(uv * 2 - 1, depth), 1));
						fragpos.xyz /= fragpos.w;
						float4 thisWorldPosition = fragpos;

						float2 motionVectors = float2(0,0);// tex2Dlod(_CameraMotionVectorsTexture, float4(input.uv.xy, 0.0, 0.0)).xy;
						float2 reprojCoord = input.uv.xy - motionVectors.xy;

						float prevDepth = SAMPLE_DEPTH_TEXTURE(_PreviousDepth, sampler_PreviousDepth, float2(reprojCoord + oneOverResolution * 0.0)).x;
						//tex2Dlod(_PreviousDepth, sampler_PreviousDepth, float4(reprojCoord + oneOverResolution * 0.0, 0.0, 0.0)).x;//LinearEyeDepth(tex2Dlod(_PreviousDepth, float4(reprojCoord + oneOverResolution * 0.0, 0.0, 0.0)).x);
					prevDepth = LinearEyeDepthB(prevDepth);

					float4 previousWorldPosition = mul(_LastFrameInverseViewProjectionMatrix, float4(reprojCoord.xy * 2.0 - 1.0, prevDepth, 1.0)); //prevDepth * 2.0 - 1.0, 1.0));
					previousWorldPosition /= previousWorldPosition.w;

					float blendWeight = 0.15 * _TemporalResponse;

					float posSimilarity = saturate(1.0 - distance(previousWorldPosition.xyz, thisWorldPosition.xyz) * 1.0);
					blendWeight = lerp(1.0, blendWeight, posSimilarity);

					float4 minPrev = float4(10000, 10000, 10000, 10000);
					float4 maxPrev = float4(0, 0, 0, 0);

					float4 s0 = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, float2(input.uv.xy + oneOverResolution * float2(0.5, 0.5)));// tex2Dlod(_MainTex, float4(input.uv.xy + oneOverResolution * float2(0.5, 0.5), 0, 0));
					minPrev = s0;
					maxPrev = s0;
					s0 = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, float2(input.uv.xy + oneOverResolution * float2(0.5, -0.5)));
					// tex2Dlod(_MainTex, float4(input.uv.xy + oneOverResolution * float2(0.5, -0.5), 0, 0));
					minPrev = min(minPrev, s0);
					maxPrev = max(maxPrev, s0);
					s0 = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, float2(input.uv.xy + oneOverResolution * float2(-0.5, 0.5)));
					//  tex2Dlod(_MainTex, float4(input.uv.xy + oneOverResolution * float2(-0.5, 0.5), 0, 0));
					minPrev = min(minPrev, s0);
					maxPrev = max(maxPrev, s0);
					s0 = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, float2(input.uv.xy + oneOverResolution * float2(-0.5, -0.5)));
					//  tex2Dlod(_MainTex, float4(input.uv.xy + oneOverResolution * float2(-0.5, -0.5), 0, 0));
					minPrev = min(minPrev, s0);
					maxPrev = max(maxPrev, s0);

					float4 prevGI = SAMPLE_TEXTURE2D(_PreviousColor, sampler_PreviousColor, float2(reprojCoord.xy));
					// //tex2Dlod(_PreviousColor, float4(reprojCoord, 0.0, 0.0));
					prevGI = lerp(prevGI, clamp(prevGI, minPrev, maxPrev), 0.25);

					gi = lerp(prevGI, gi, float4(blendWeight, blendWeight, blendWeight, blendWeight)*0.65 * _TemporalGain);

					return gi;// (gi + prevGI) / 2;
				}
					ENDHLSL
			}
			Pass //8
			{
				Name"GetDepth"
				HLSLPROGRAM//CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				struct AttributesA
				{
					float4 positionHCS   : POSITION;
					float2 uv           : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct VaryingsA
				{
					float4  positionCS  : SV_POSITION;
					float2  uv          : TEXCOORD0;
					UNITY_VERTEX_OUTPUT_STEREO
				};

				VaryingsA vert(AttributesA input)
				{
					VaryingsA output;
					UNITY_SETUP_INSTANCE_ID(input);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

					// Note: The pass is setup with a mesh already in clip
					// space, that's why, it's enough to just output vertex
					// positions
					output.positionCS = float4(input.positionHCS.xyz, 1.0);

					#if UNITY_UV_STARTS_AT_TOP
					output.positionCS.y *= -1;
					#endif

					output.uv = input.uv;
					return output;
				}

				/*struct v2f
				{
					float4 pos : SV_POSITION;
					float4 uv : TEXCOORD0;
				};*/
				float4 frag(VaryingsA input) : COLOR0
				{
					float2 coord = input.uv.xy + (1.0 / _ScreenParams.xy) * 0.5;
					//float4 tex = tex2D(_CameraDepthTexture, coord);

					float4 tex = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, coord);

					return tex;
				}
				ENDHLSL
			}
					///// END TEMPORAL AA
					////// BLEND - //v0.6a
					Pass // 9
				{
					Name "Blend"
					HLSLPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					float4 controlCloudEdgeA;// = float4(0.65, 1.22, 1.14, 1.125);
					float controlCloudAlphaPower;// = 2;
					float controlBackAlphaPower;
					struct AttributesA
					{
						float4 positionHCS   : POSITION;
						float2 uv           : TEXCOORD0;
						UNITY_VERTEX_INPUT_INSTANCE_ID
					};
					struct VaryingsA
					{
						float4  positionCS  : SV_POSITION;
						float2  uv          : TEXCOORD0;
						UNITY_VERTEX_OUTPUT_STEREO
					};
					VaryingsA vert(AttributesA input)
					{
						VaryingsA output;
						UNITY_SETUP_INSTANCE_ID(input);
						UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
						output.positionCS = float4(input.positionHCS.xyz, 1.0);
						#if UNITY_UV_STARTS_AT_TOP
						output.positionCS.y *= -1;
						#endif
						output.uv = input.uv;
						return output;
					}
					float LinearEyeDepthB(float z)
					{
						return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
					}
					float4 frag(VaryingsA  input) : SV_Target
					{
						UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
						float2 uv = input.uv.xy;
						//float2 oneOverResolution = (1.0 / _ScreenParams.xy);
						float4 cloud = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, input.uv.xy);
						float4 back = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv.xy);
						//float4 depthIMG = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.uv.xy);
						//float depth = LinearEyeDepthB(depthIMG.x);	

						float2 coord = input.uv.xy + (1.0 / _ScreenParams.xy) * 0.5;
						//float4 tex = tex2D(_CameraDepthTexture, coord);
						float4 tex = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, coord);
						float zsample = tex.x;
						float depth = Linear01Depth(zsample * (zsample < 1.0), _ZBufferParams);

						//v0.6
						//return float4(depth, depth, depth, depth);
						//float zsample = Linear01DepthA(i.uv.xy);
						//float depth = Linear01Depth(zsample * (zsample < 1.0), _ZBufferParams);
						//return float4(back.rgb * (1.0 - cloud.a) + cloud.rgb, 1.0)*depth + float4((1 - depth) * back.rgb, 0); // blend them
						//if (depthDilation == 0) {
						//	depth = 1;
						//}
						//float4 controlCloudEdgeA = float4(0.6, 0.8, 0.8,0.7); //float4(0.65, 1.22, 1.14, 1.125);
						//float controlCloudAlphaPower = 2;
						//float controlBackAlphaPower = 1;
						//return cloud;
						//if (depth >1- 0.999) {
						//if (depth < 0.9999) {//forground
						if (depth >= 0.9999) {//forground
							return (controlCloudEdgeA.x*float4(back.rgb* (controlCloudEdgeA.y), 1)
								+ 0.03*controlCloudEdgeA.z*float4(cloud.rgb * 1, 1))
								*controlCloudEdgeA.w *depth + controlBackAlphaPower * 1.5*float4((1 - depth) * back.rgb, 1) *(controlCloudAlphaPower + cloud.rgba);
						}
						return (controlCloudEdgeA.x*float4(back.rgb* (controlCloudEdgeA.y), 1)
								+ controlCloudEdgeA.z*float4(cloud.rgb * 1, 1))
								*controlCloudEdgeA.w *depth + controlBackAlphaPower * 1.5*float4((1 - depth) * back.rgb, 1) *(controlCloudAlphaPower + cloud.rgba);
						/*
						if (depthDilation == 0) {
							depth = 1;
						}
						return (controlCloudEdgeA.x*float4(back.rgb* (controlCloudEdgeA.y - pow(cloud.a, controlBackAlphaPower)), 1)
							+ controlCloudEdgeA.z*float4(cloud.rgb* pow(cloud.a, controlCloudAlphaPower + 0.001), 1))
						*controlCloudEdgeA.w *depth + float4((1 - depth) * back.rgb, 1);
						*/


						//return colorMAIN + color * (1);// return float4(depth, depth, depth, depth )
					}
					ENDHLSL
				}
					////// END BLEND

						/////// SSMS
						// 10: Prefilter
						Pass
					{
						ZTest Always Cull Off ZWrite Off
						CGPROGRAM
						#pragma multi_compile _ UNITY_COLORSPACE_GAMMA
						#include "SSMS.cginc"
						#pragma vertex vert
						#pragma fragment frag_prefilter
						#pragma target 3.0
						ENDCG
					}
						// 11: Prefilter with anti-flicker
						Pass
					{
						ZTest Always Cull Off ZWrite Off
						CGPROGRAM
						#define ANTI_FLICKER 1
						#pragma multi_compile _ UNITY_COLORSPACE_GAMMA
						#include "SSMS.cginc"
						#pragma vertex vert
						#pragma fragment frag_prefilter
						#pragma target 3.0
						ENDCG
					}
						// 12: First level downsampler
						Pass
					{
						ZTest Always Cull Off ZWrite Off
						CGPROGRAM
						#include "SSMS.cginc"
						#pragma vertex vert
						#pragma fragment frag_downsample1
						#pragma target 3.0
						ENDCG
					}
						// 13: First level downsampler with anti-flicker
						Pass
					{
						ZTest Always Cull Off ZWrite Off
						CGPROGRAM
						#define ANTI_FLICKER 1
						#include "SSMS.cginc"
						#pragma vertex vert
						#pragma fragment frag_downsample1
						#pragma target 3.0
						ENDCG
					}
						// 14: Second level downsampler
						Pass
					{
						ZTest Always Cull Off ZWrite Off
						CGPROGRAM
						#include "SSMS.cginc"
						#pragma vertex vert
						#pragma fragment frag_downsample2
						#pragma target 3.0
						ENDCG
					}
						// 15: Upsampler
						Pass
					{
						ZTest Always Cull Off ZWrite Off
						CGPROGRAM
						#include "SSMS.cginc"
						#pragma vertex vert_multitex
						#pragma fragment frag_upsample
						#pragma target 3.0
						ENDCG
					}
						// 16: High quality upsampler
						Pass
					{
						ZTest Always Cull Off ZWrite Off
						CGPROGRAM
						#define HIGH_QUALITY 1
						#include "SSMS.cginc"
						#pragma vertex vert_multitex
						#pragma fragment frag_upsample
						#pragma target 3.0
						ENDCG
					}
						// 17: Combiner
						Pass
					{
						ZTest Always Cull Off ZWrite Off
						CGPROGRAM
						#pragma multi_compile _ UNITY_COLORSPACE_GAMMA
						#include "SSMS.cginc"
						#pragma vertex vert_multitex
						#pragma fragment frag_upsample_final
						#pragma target 3.0
						ENDCG
					}
						// 18: High quality combiner
						Pass
					{
						ZTest Always Cull Off ZWrite Off
						CGPROGRAM
						#define HIGH_QUALITY 1
						#pragma multi_compile _ UNITY_COLORSPACE_GAMMA
						#include "SSMS.cginc"
						#pragma vertex vert_multitex
						#pragma fragment frag_upsample_final
						#pragma target 3.0
						ENDCG
					}
						// 19: Combiner2
						Pass
					{
						ZTest Always Cull Off ZWrite Off
						CGPROGRAM
						#pragma multi_compile _ UNITY_COLORSPACE_GAMMA
						#include "SSMS.cginc"
						#pragma vertex vert_multitex
						#pragma fragment frag_upsample_final2
						#pragma target 3.0
						ENDCG
					}
						/////// END SSMS

	}
}