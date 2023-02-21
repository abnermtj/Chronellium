Shader "PRISM/PRISMMotionBlur"
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
#include "PRISMColorSpaces.hlsl"

#include "PRISMDoF.hlsl"
//
		//sampler2D _MainTex;
	UNITY_DECLARE_TEX2D(_MainTex);
	float4 _MainTex_TexelSize;

	UNITY_DECLARE_TEX2D(_MotionVectorTexture);

	

#define HALF_MAX 65504.0

	float _Intensity;

	inline float4 SampleMainTexture(float2 uv)
	{
		return UNITY_SAMPLE_TEX2D(_MainTex, uv);
	}

	inline half4 SampleMotionTexture(float2 uv)
	{
		return UNITY_SAMPLE_TEX2D(_MotionVectorTexture, uv).r;
	}


	float4 fragMoBlurPrepass(PostProcessVaryings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

		half4 outColor = SampleMainTexture(uv);

		return outColor;
	}

	float4 fragMoBlur(PostProcessVaryings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

		half4 outColor = SampleMainTexture(uv);

		return outColor;
	}

	half4 fragMoBlurComb(PostProcessVaryings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

		half4 outColor = SampleMainTexture(uv);
		outColor *= SampleMotionTexture(uv);


		//float depthSample = Linear01Depth(SampleSceneDepth(uv));
		//outColor *= depthSample;

		return outColor;
	}

		
		ENDHLSL

	SubShader
	{

		Cull Off ZWrite Off ZTest Always
			Pass
		{
			Name "MoBlur Prepass"
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragMoBlurPrepass
			ENDHLSL
		}

		ZWrite Off
		ZTest Always
		Cull Off
		Pass{
			Name "MoBlur Blur"
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragMoBlur
			ENDHLSL
		}

			Cull Off ZWrite Off ZTest Always
			Pass
		{
			Name "MoBlur Comb"
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragMoBlurComb
			ENDHLSL
		}
			
	}
	Fallback Off
}