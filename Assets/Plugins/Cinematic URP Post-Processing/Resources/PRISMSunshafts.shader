Shader "PRISM/PRISMSunshafts"
{
	HLSLINCLUDE
#pragma warning( disable:4005 )
#include "CUPPCore.hlsl"

// Disable warnings we aren't interested in
#if defined(UNITY_COMPILER_HLSL)
#pragma warning (disable : 3205) // conversion of larger type to smaller
#pragma warning (disable : 3568) // unknown pragma ignored
#pragma warning (disable : 3571) // "pow(f,e) will not work for negative f"; however in majority of our calls to pow we know f is not negative
#pragma warning (disable : 3206) // implicit truncation of vector type
#endif

#include "PRISMCG.hlsl"
#include "PRISMDoF.hlsl"
#include "PRISMColorSpaces.hlsl"
#pragma multi_compile __ ULTRA_QUALITY

//
		//sampler2D _MainTex;
	UNITY_DECLARE_TEX2D(_MainTex);
	float4 _MainTex_TexelSize;

	UNITY_DECLARE_TEX2D(_SunTex);
	float4 _SunTex_TexelSize;

	UNITY_DECLARE_TEX2D(_SunTexIntermed);
	float4 _SunTexIntermed_TexelSize;

	float _UseNoise;
	float _UseBlur;
#define HALF_MAX 65504.0

	float _Intensity;
	//START SUN VARIABLES
	uniform half4 _SunColor;
	uniform half4 _SunPosition;
	uniform float _SunWeight;

	uniform float _SunDecay = 0.96815;
	uniform float _SunDensity = 0.926;
	//END SUN VARIABLES

	inline float4 SampleMainTexture(float2 uv)
	{
		return UNITY_SAMPLE_TEX2D(_MainTex, uv);
	}

	inline float4 SampleSunTexture(float2 uv)
	{
		return UNITY_SAMPLE_TEX2D(_SunTex, uv);
	}

	inline float4 SampleSunTextureIntermed(float2 uv)
	{
		return UNITY_SAMPLE_TEX2D(_SunTexIntermed, uv);
	}

	//Noises====================
	//
	// Interleaved gradient function from CoD AW.
	// http://www.iryoku.com/next-generation-post-processing-in-call-of-duty-advanced-warfare
	float gnoise(float2 uv, float2 offs)
	{
		//_ScreenParams.xy
		uv = uv / _MainTex_TexelSize.xy + offs;
		float3 magic = float3(0.06711056, 0.00583715, 52.9829189);
		return frac(magic.z * frac(dot(uv, magic.xy)));
	}

	float CUPPfrac(float x)
	{
		return x - floor(x);
	}

	float interleavedGradientNoise(float2 n) {
		float f = 0.06711056 * n.x + 0.00583715 * n.y;
		return CUPPfrac(52.9829189 * CUPPfrac(f));
	}

	half4 fragBlurVertical(half2 uv)
	{
		half2 ps = _SunTex_TexelSize.xy;
		half4 c1 = UNITY_SAMPLE_TEX2D(_SunTex, float2(0.5, -4.30908)*ps + uv);
		half4 c2 = UNITY_SAMPLE_TEX2D(_SunTex, float2(0.5, -2.37532)*ps + uv);
		half4 c3 = UNITY_SAMPLE_TEX2D(_SunTex, float2(0.5, -0.5)*ps + uv);
		half4 c4 = UNITY_SAMPLE_TEX2D(_SunTex, float2(0.5, 1.37532)*ps + uv);
		half4 c5 = UNITY_SAMPLE_TEX2D(_SunTex, float2(0.5, 3.30908)*ps + uv);

			/*half4 c1 = UNITY_SAMPLE_TEX2D(_SunTex, float2(0.5, -0.5)*ps + uv);
		half4 c2 = UNITY_SAMPLE_TEX2D(_SunTex, float2(0.5, -1.43376)*ps + uv);
		half4 c3 = UNITY_SAMPLE_TEX2D(_SunTex, float2(0.5, -3.30908)*ps + uv);
		half4 c4 = UNITY_SAMPLE_TEX2D(_SunTex, float2(0.5, -5.1844)*ps + uv);
		half4 c5 = UNITY_SAMPLE_TEX2D(_SunTex, float2(0.5, -7.11816)*ps + uv);*/

		c1 *= 0.055028;
		c2 *= 0.244038;
		c3 *= 0.401870;
		c4 *= 0.244038;
		c5 *= 0.055028;

		return c1 + c2 + c3 + c4 + c5;
	}

	float4 fragSunPrepass(PostProcessVaryings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

		float depthSample = Linear01Depth(SampleSceneDepth(uv));

		//_SunPosition.xyz = GetWorldSpaceViewDir(_MainLightPosition.xyz);

		half2 vec = _SunPosition.xy - uv;
		half dist = saturate(_SunPosition.w - length(vec.xy));

		half4 outColor = 0;

		// consider shafts blockers
		if (depthSample > 0.9999)
		{
			half4 tex = SampleMainTexture(uv);
			outColor = tex * dist;
		}
		//
		//outColor.rgb = ThresholdColor(outColor, 0.2);

		return outColor;
	}

	float4 fragSunBlur(PostProcessVaryings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

#if ULTRA_QUALITY
		const int NUM_SUN_SAMPLES = 32;
#else
		const int NUM_SUN_SAMPLES = 16;
#endif

		float2 deltaTexCoord = (uv - _SunPosition.xy);// (uv - _SunPosition.xy);

		float rand = interleavedGradientNoise(uv / _SunTexIntermed_TexelSize.xy) - 0.5;
		deltaTexCoord *= (1.0 / (float)NUM_SUN_SAMPLES) * _SunDensity;
		float illuminationDecay = 1.0;

		float4 col = (float4)0.0;// = mainCol * 0.01;

		for (int i = 0; i < NUM_SUN_SAMPLES; i++)
		{
			uv -= deltaTexCoord;
			uv += rand * (_SunTexIntermed_TexelSize.xy * 3.);
			//uv -= (deltaTexCoord * rand);
			rand = interleavedGradientNoise(uv / _SunTexIntermed_TexelSize.xy) - 0.5;

#if ULTRA_QUALITY
			float4 sunSampleBeep = SampleSunTextureIntermed(uv)*0.2;
			sunSampleBeep *= (illuminationDecay * _SunWeight);
			col += sunSampleBeep;
			illuminationDecay *= _SunDecay;
#else 
			float4 sunSampleBeep = SampleSunTextureIntermed(uv)*0.2;
			sunSampleBeep *= (illuminationDecay * _SunWeight);
			col += sunSampleBeep;
			illuminationDecay *= _SunDecay;
#endif
		}

		col.rgb = saturate(col.rgb * _SunColor.rgb);

		return col;
	}

	half4 fragSunComb(PostProcessVaryings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

		half4 mainCol = SampleMainTexture(uv);

#if ULTRA_QUALITY
		float4 sunCol = fragBlurVertical(uv);
#else
		half4 sunCol = fragBlurVertical(uv);
#endif
		return mainCol + sunCol;
	}

		
		ENDHLSL

	SubShader
	{

		Cull Off ZWrite Off ZTest Always
			Pass
		{
			Name "Sun Prepass"
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragSunPrepass
			ENDHLSL
		}

		ZWrite Off
		ZTest Always
		Cull Off
		Pass{
			Name "Sun Blur"
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragSunBlur
			ENDHLSL
		}

			Cull Off ZWrite Off ZTest Always
			Pass
		{
			Name "Sun Comb"
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragSunComb
			ENDHLSL
		}
			
	}
	Fallback Off
}