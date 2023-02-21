Shader "PRISM/PRISMDebug"
{
	HLSLINCLUDE
#include "CUPPCore.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "PRISMColorSpaces.hlsl"
#include "PRISMCG.hlsl"

		UNITY_DECLARE_TEX2D(_MainTex);
	UNITY_DECLARE_TEX2D(_SecondaryTex);
	
	//SHARPEN AREA===========================================================================================================================
#define offset_bias 1.0
#define sharp_clamp 1.0
//End sharpen variables

// Mean of Rec. 709 & 601 luma coefficients
#define lumacoeff        float3(0.2558, 0.6511, 0.0931)
#define HALF_MAX 65504.0

	float _Intensity;

	inline float4 SampleMainTexture(float2 uv)
	{
		return UNITY_SAMPLE_TEX2D(_MainTex, uv);
	}


	float4 FullscreenDebug(PostProcessVaryings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

		half4 color;

		// calculate the intensity bucket for this pixel based on column height (padded at the top)
		const float max_value = 1.0;
		const float buckets = 256.0;
		float bucket_min = log(max_value * floor(uv.y * buckets) / buckets);
		float bucket_max = log(max_value * floor((uv.y * buckets) + 1.0) / buckets);

		// count the count the r,g,b and luma in this column that match the bucket
		float4 count = float4(0.0, 0.0, 0.0, 0.0);
		for (int i = 0; i < 256; ++i) {
			float j = float(i) / buckets;
			float3 pixel = SampleMainTexture(float2(uv.x, j));

			float3 labPixel = RGBtoLab(pixel.rgb);

			float3 logpixel = log(labPixel);
			if (logpixel.r >= bucket_min && logpixel.r < bucket_max) count.r += 1.0;
			if (logpixel.g >= bucket_min && logpixel.g < bucket_max) count.g += 1.0;
			if (logpixel.b >= bucket_min && logpixel.b < bucket_max) count.b += 1.0;
		}

		// sum luma into RGB, tweak log intensity for readability
		const float gain = 1.0;
		const float blend = 0.6;
		//count.gb = log(lerp(count.gb, count.rr, blend)) * gain;

		// output
		color = count;

		return color;
	}


	float4 FullscreenDebugCombine(PostProcessVaryings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

		half4 color;

		color = SampleMainTexture(uv);

		if (uv.x < 0.4 && uv.y > 0.6)
		{
			color += UNITY_SAMPLE_TEX2D(_SecondaryTex, uv);
		}

		return color;
	}
		ENDHLSL

	SubShader
	{
		Name "PRISM Debug"
			ZWrite Off
			ZTest Always
			Cull Off

			Pass{
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment FullscreenDebug
			ENDHLSL
		}

			Pass{
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment FullscreenDebugCombine
			ENDHLSL
		}
	}
	Fallback Off
}

		/*
    float4 FullScreenPassSharpen(Varyings varyings) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
        float depth = LoadCameraDepth(varyings.positionCS.xy);
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
        float3 viewDirection = GetWorldSpaceNormalizeViewDir(posInput.positionWS);
        float4 color = float4(0.0, 0.0, 0.0, 0.0);

        // Load the camera color buffer at the mip 0 if we're not at the before rendering injection point
        if (_CustomPassInjectionPoint != CUSTOMPASSINJECTIONPOINT_BEFORE_RENDERING)
            color = float4(CustomPassLoadCameraColor(varyings.positionCS.xy, 0), 1);

        // Add your custom pass code here
		float2 uv = posInput.positionNDC.xy * _RTHandleScale.xy;
		color = sharpen(uv, _Intensity, _ScreenSize.zw);

		return float4(color);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "PRISM HDRP Sharpen"

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
                #pragma fragment FullScreenPassSharpen
            ENDHLSL
        }
    }

    Fallback Off
}*/
