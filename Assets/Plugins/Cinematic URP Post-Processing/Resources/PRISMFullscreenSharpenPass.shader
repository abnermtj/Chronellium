Shader "PRISM/PRISMFullscreenSharpenPass"
{
	HLSLINCLUDE
#pragma warning( disable:4005 )
#include "CUPPCore.hlsl"
//#include "HLSLSupport.cginc"
// Disable warnings we aren't interested in
#if defined(UNITY_COMPILER_HLSL)
#pragma warning (disable : 3205) // conversion of larger type to smaller
#pragma warning (disable : 3568) // unknown pragma ignored
#pragma warning (disable : 3571) // "pow(f,e) will not work for negative f"; however in majority of our calls to pow we know f is not negative
#pragma warning (disable : 3206) // implicit truncation of vector type
#endif
#include "PRISMCG.hlsl"
#pragma multi_compile __ MEDIAN_SHARPEN
#pragma multi_compile __ DEPTH_SHARPEN

#if DEPTH_SHARPEN
#include "PRISMDoF.hlsl"
#endif
//
		//sampler2D _MainTex;
	UNITY_DECLARE_TEX2D(_MainTex);
	float4 _MainTex_TexelSize;

	//SHARPEN AREA===========================================================================================================================
#define offset_bias 1.0
#define sharp_clamp 0.25
//End sharpen variables

// Mean of Rec. 709 & 601 luma coefficients
#define lumacoeff        float3(0.2558, 0.6511, 0.0931)
#define HALF_MAX 65504.0

	float _Intensity;

	inline float4 SampleMainTexture(float2 uv)
	{
		return UNITY_SAMPLE_TEX2D(_MainTex, uv);
	}

	///.. -1 ..
	///-1  5 -1
	///.. -1 ..
	half4 sharpen(float2 uv, float _SharpenAmount, half2 PixelSize)
	{
		float4 colorInput = SampleMainTexture(uv);

		float3 ori = colorInput.rgb;

		// -- Combining the strength and luma multipliers --
		float3 sharp_strength_luma = (lumacoeff * _SharpenAmount); //I'll be combining even more multipliers with it later on

		// -- Gaussian filter --
		//   [ .25, .50, .25]     [ 1 , 2 , 1 ]
		//   [ .50,   1, .50]  =  [ 2 , 4 , 2 ]
		//   [ .25, .50, .25]     [ 1 , 2 , 1 ]
		float px = PixelSize.x;//1.0/
		float py = PixelSize.y;

		float3 blur_ori = SampleMainTexture(uv + float2(px, -py) * 0.5 * offset_bias).rgb; // South East
		blur_ori += SampleMainTexture(uv + float2(-px, -py) * 0.5 * offset_bias).rgb;  // South West
		blur_ori += SampleMainTexture(uv + float2(px, py) * 0.5 * offset_bias).rgb; // North East
		blur_ori += SampleMainTexture(uv + float2(-px, py) * 0.5 * offset_bias).rgb; // North West

		blur_ori *= 0.25;  // ( /= 4) Divide by the number of texture fetches

		// -- Calculate the sharpening --
		float3 sharp = ori - blur_ori;  //Subtracting the blurred image from the original image

		// -- Adjust strength of the sharpening and clamp it--
		float4 sharp_strength_luma_clamp = float4(sharp_strength_luma * (0.5 / sharp_clamp), 0.5); //Roll part of the clamp into the dot

		float sharp_luma = clamp((dot(float4(sharp, 1.0), sharp_strength_luma_clamp)), 0.0, 1.0); //Calculate the luma, adjust the strength, scale up and clamp
		sharp_luma = (sharp_clamp * 2.0) * sharp_luma - sharp_clamp; //scale down

		// -- Combining the values to get the final sharpened pixel	--
		colorInput.rgb = colorInput.rgb + sharp_luma;    // Add the sharpening to the input color.
		return clamp(colorInput, 0.0, 1000.0);
	}


	half4 fragMedMediumSharpen(float2 uv, float2 _MainTex_TexelSize, float _SharpenAmount)
	{
		float2 ooRes = _MainTex_TexelSize.xy;//_ScreenParams.w;


		float3 sharp_strength_luma = (lumacoeff * _SharpenAmount); //I'll be combining even more multipliers with it later on

		//This doesn't work on SM2
		float3 ofs = _MainTex_TexelSize.xyx * float3(1, 1, 0);

#if DEPTH_SHARPEN
		float midDepth = LinearEyeDepth(SampleSceneDepth(uv));
#endif

		//
		float3 v[5];

		float4 midCol = SampleMainTexture( uv);

		v[0] = midCol.rgb;
		v[1] = SampleMainTexture(uv - ofs.xz).rgb;
		v[2] = SampleMainTexture(uv + ofs.xz).rgb;
		v[3] = SampleMainTexture(uv - ofs.zy).rgb;
		v[4] = SampleMainTexture(uv + ofs.zy).rgb;

#if DEPTH_SHARPEN
		//Then sample depth at each point. If the depth gap between the point and the middle is too large, we just use the "real" (middle) color for it.
//Depth Threshold: > 0.1units: discard
//1
		if (abs(LinearEyeDepth(SampleSceneDepth(uv - ofs.xz)) - midDepth) > 0.04)
		{
			v[1] = v[0];
		}
		//2
		if (abs(LinearEyeDepth(SampleSceneDepth(uv + ofs.xz)) - midDepth) > 0.04)
		{
			v[2] = v[0];
		}

		//3
		if (abs(LinearEyeDepth(SampleSceneDepth(uv - ofs.zy)) - midDepth) > 0.04)
		{
			v[3] = v[0];
		}

		//4
		if (abs(LinearEyeDepth(SampleSceneDepth(uv + ofs.zy)) - midDepth) > 0.04)
		{
			v[4] = v[0];
		}
#endif

		float3 temp;
		mnmx5(v[0], v[1], v[2], v[3], v[4]);
		mnmx3(v[1], v[2], v[3]);

		//return float4(midCol.rgb, 1.0);

		float3 sharp = midCol.rgb - v[2].rgb;


		// -- Adjust strength of the sharpening and clamp it--
		float4 sharp_strength_luma_clamp = float4(sharp_strength_luma * (0.5 / sharp_clamp), 0.5); //Roll part of the clamp into the dot

		float sharp_luma = clamp((dot(float4(sharp, 1.0), sharp_strength_luma_clamp)), 0.0, 1.0); //Calculate the luma, adjust the strength, scale up and clamp
		sharp_luma = (sharp_clamp * 2.0) * sharp_luma - sharp_clamp; //scale down

		// -- Combining the values to get the final sharpened pixel	--
		midCol.rgb = midCol.rgb + sharp_luma;    // Add the sharpening to the input color.
		return clamp(midCol, 0.0, 1000.0);

		//midCol.rgb = lerp(midCol.rgb, SafeHDR(v[2].rgb), -_SharpenAmount);

		return midCol;
	}

	float4 FullScreenPassSharpen(PostProcessVaryings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

		half4 color = (half4)0.0;
#if MEDIAN_SHARPEN
		color = fragMedMediumSharpen(uv, _MainTex_TexelSize.xy, _Intensity);
#else
		color = sharpen(uv, _Intensity, _MainTex_TexelSize.xy);
#endif

		return color;
	}


		
		ENDHLSL

	SubShader
	{
		Name "PRISM HDRP Sharpen"
			ZWrite Off
			ZTest Always
			Cull Off

			Pass{

			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment FullScreenPassSharpen
			ENDHLSL

		}
	}
	Fallback Off
}