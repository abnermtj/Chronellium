// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


// Originall from Unity Pro PostFX
// modified and adapted DLAA code based on Dmitry Andreev's
// Directionally Localized Anti-Aliasing (DLAA)
//
// as seen in "The Force Unleashed 2
//

Shader "PRISM/PRISMDOF" {

	HLSLINCLUDE
#pragma target 5.0
#pragma warning( disable:4005 )
#include "CUPPCore.hlsl"
#include "PRISMDoF.hlsl"
#include "PRISMCG.hlsl"
#include "PRISMGlobals.hlsl"
#include "PRISMEasing.hlsl"
#include "PRISMColorSpaces.hlsl"
#pragma exclude_renderers gles gles3
#pragma multi_compile __ DOFAF
#pragma multi_compile __ DOF_FRONTBLUR

		// DoF parameters.
		int DoFSamplesPerRing; //4
		int DoFRings; //5
		int BokehDoFEdgeCount;// 5
		int BokehDoFSamplePerEdge;// 2
	uniform float DoFMaxBlur;// 2.
	uniform float BokehDoFIntensity;// 1.
	uniform float BokehDoFTreshold;// 0.7
	uniform float fAperture;// = 2.0;
	uniform float fFocalLength;// = 2.0;
	uniform float DofFocusDist;// = 2.0;
	uniform float DofSensorSize;// = 2.0;

		UNITY_DECLARE_TEX2D(_MainTex);
		//UNITY_DECLARE_TEX2D(samplerDepth);

		
		UNITY_DECLARE_TEX2D(_SecondaryTex);

		float4 _MainTex_TexelSize;
		float4 _SecondaryTex_TexelSize;

		float LinearizeDepth(float inDepth)
		{
			return inDepth;
		}

		// Martinsh LensBlur implementation.
		float3 LensBlur(Texture2D inTex, SamplerState Tex, float2 texCoord, float max_radius, float2 texelSize) {
			float3 outColor = inTex.SampleLevel(Tex, texCoord, 0.0).xyz;
			float weight = 1.0;

			int ringsamples;
			float w = texelSize.x * max_radius;
			float h = texelSize.y * max_radius;
			//[unroll]
			for (int i = 1; i <= DoFRings; i++) {
				ringsamples = i * DoFSamplesPerRing;
				for (int j = 0; j < ringsamples; j++) {
					float fStep = 3.14*2.0 / float(ringsamples);
					float pw = (cos(float(j)*fStep)*float(i));
					float ph = (sin(float(j)*fStep)*float(i));
					outColor += inTex.SampleLevel(Tex, 
						texCoord + float2(pw*w, ph*h), 0.0).rgb
						*lerp(1.0, (float(i)) / ((float)DoFRings), 0.3);
					//outColor += UNITY_SAMPLE_TEX2D(_MainTex, texCoord + float2(pw*w, ph*h)).rgb*lerp(1.0, (float(i)) / ((float)DoFRings), 0.3);
					weight += 1.0*lerp(1.0, (float(i)) / ((float)DoFRings), 0.3);
				}
			}
			return outColor / weight;
		}

		half3 LensBlurMobile(float2 texCoord, float max_radius, float2 texelSize) {
			half3 outColor = UNITY_SAMPLE_TEX2D(_MainTex, texCoord).xyz;
			half weight = 1.0;

			int ringsamples;
			half w = texelSize.x * max_radius;
			half h = texelSize.y * max_radius;
			//[unroll]
			for (int i = 1; i <= DoFRings; i++) {
				ringsamples = i * DoFSamplesPerRing;
				for (int j = 0; j < ringsamples; j++) {
					half fStep = 3.14*2.0 / half(ringsamples);
					half pw = (cos(half(j)*fStep)*half(i));
					half ph = (sin(half(j)*fStep)*half(i));
					outColor += UNITY_SAMPLE_TEX2D(_MainTex, texCoord + float2(pw*w, ph*h)).rgb*lerp(1.0, (float(i)) / ((float)DoFRings), 0.3);
					weight += 1.0*lerp(1.0, (half(i)) / ((half)DoFRings), 0.3);
				}
			}
			return outColor / weight;
		}

		// Petr Georgievsky N-Edge Shaped Blur implementation.(used for bokeh)
		float3 N_EdgeShapedBlur(Texture2D inTex, SamplerState sTex, float2 vTexCoord, int iEdgeCount, int iBlurSamples, float fBlurRadius, float2 texelSize) 
		{
			//return (float3)0.0;
			float3 oColor;
			float2 mBlurOffsets[64], vBlur;
			float fWeight, fWidth, fHeight, fAngle;
			int iCurrentEdge, iCurrBlurSample;

			// Initialization Step
			oColor = inTex.SampleLevel(sTex, vTexCoord, 0.0).rgb;					// First init first sample.
			fWeight = 1.0;											// Set our weight to 1 to avoid division by zero.
			fWidth = (texelSize.x / 0.25)*fBlurRadius;		// Calculate Blur Factor by X.
			fHeight = (texelSize.y / 0.25)*fBlurRadius;		// Calculate Blur Factor by Y.

			// Blur Offsets Calculation Step.
			//[unroll]
			for (iCurrentEdge = 0; iCurrentEdge < iEdgeCount; iCurrentEdge++) {
				fAngle = ((PI*2.0) / (float)iEdgeCount)*(float)iCurrentEdge;	// Compute our angle in radians using that equalation - 2PI*I/N, where I - edge number, N - edge count.
				mBlurOffsets[iCurrentEdge] = float2(cos(fAngle), sin(fAngle));	// Compute Blur Offsets just by converting that angle to cartesian coordinate system.
			}

			// Shaped Blur Step.
			for (iCurrentEdge = 1; iCurrentEdge < iEdgeCount; iCurrentEdge++) {
				for (iCurrBlurSample = 0; iCurrBlurSample < iBlurSamples; iCurrBlurSample++) {
					vBlur = lerp(mBlurOffsets[iCurrentEdge - 1], mBlurOffsets[iCurrentEdge], ((1.0 / (float)iBlurSamples)*(float)iCurrBlurSample));	// Calculate Blur Point to blur our shape.
					oColor += //_SecondaryTex.SampleLevel(sTex,
						inTex.SampleLevel(sTex, 
						float2(vTexCoord + float2(vBlur.x*fWidth, vBlur.y*fHeight)), 
						(float2)0.0
					).rgb;									// Sample our texture with blur offset(I use that tex2Dlod function for preformance reasons and to avoid unroll).
					fWeight++;
				}
			}
			return oColor / fWeight;
		}

		float GetAutoFocus(float2 uv) {

#if 0
			if(uv.x < 0.99)
			{
				return _MyCustomBuffer[0].something.b;
			}

			float af = LinearEyeDepth(SampleSceneDepth((float2)0.5));// LinearizeDepth(UNITY_SAMPLE_TEX2D(samplerDepth, float2(0.5, 0.5)).r);

			float distance = lerp(_MyCustomBuffer[0].something.b, af, unity_DeltaTime.r * BokehDoFTreshold);

			_MyCustomBuffer[0].something.b = distance; 

			return distance;
#else
			return DofFocusDist;
#endif
		}

		float GetDoFCoCNew(float depth_o, float focusDist)
		{//
			//fAperture *= 0.01;
			float s1 = max(focusDist, fFocalLength);

			float lensCoef = (fFocalLength * fFocalLength) / (fAperture * (s1 - fFocalLength) * DofSensorSize * 2.0);
			float cocs = (depth_o - s1) * lensCoef / depth_o;

			return cocs;
		}

		float GetDoFCoCFront(float depth_o, float focusDist)
		{//
			//fAperture *= 0.01;
			fFocalLength = min(fFocalLength, focusDist - 0.01);
			float s1 = max(focusDist, fFocalLength);

			float lensCoef = (fFocalLength * fFocalLength) / (fAperture * (fFocalLength - s1) * DofSensorSize * 2.0);
			float cocs = (depth_o - s1) * lensCoef / depth_o;

			return cocs;
		}


		float4 DoFBokehTresholdPass(float2 uv)
		{
			float fDepth = LinearEyeDepth(SampleSceneDepth(uv)); // SampleSceneDepth(uv);// LinearizeDepth(UNITY_SAMPLE_TEX2D(samplerDepth, uv).r);
			float fAF = GetAutoFocus(uv);
			//float Blur = fAperture * abs(fDepth - fAF) / (fDepth + 0.000001);
			//Blur = clamp(max(0.0, Blur - fFocalLength), 0.0, 1.0) * sign(fDepth - fAF);

			float Blur = GetDoFCoCFront(fDepth, fAF);
			Blur = clamp(max(0.0, Blur), 0.0, 1.0);
			//Blur = abs(Blur);

#if SHADER_API_VULKAN || SHADER_API_GLES3 || SHADER_API_METAL
			float3 edgeBlur = LensBlurMobile(uv, Blur*DoFMaxBlur, _MainTex_TexelSize.xy);
#else
			float3 edgeBlur = LensBlur(_MainTex, sampler_MainTex, uv, Blur*DoFMaxBlur, _MainTex_TexelSize.xy);
#endif

			float4 color = float4(edgeBlur.rgb 
				* Blur, Blur);
			return color;
		}

		half4 DoFBokehTresholdMobilePass(float2 uv)
		{
			float fDepth = LinearEyeDepth(SampleSceneDepth(uv));
			float fAF = GetAutoFocus(uv);

			half Blur = GetDoFCoCNew(fDepth, fAF);
			Blur = clamp(max(0.0, Blur), 0.0, 1.0);

			half3 edgeBlur = LensBlurMobile(uv, Blur*DoFMaxBlur, _MainTex_TexelSize.xy);

			half4 color = half4(edgeBlur.rgb
				, Blur);
			return color;
		}

		float4 DoFPass(float2 uv)
		{
			float fDepth = LinearEyeDepth(SampleSceneDepth(uv));
			float fAF = GetAutoFocus(uv);

			float Blur = GetDoFCoCNew(fDepth, fAF);
			Blur = clamp(max(0.0, Blur), 0.0, 1.0);

#if !DOF_FRONTBLUR
#if SHADER_API_VULKAN || SHADER_API_GLES3 || SHADER_API_METAL
			return float4(LensBlurMobile(uv, Blur*DoFMaxBlur, _MainTex_TexelSize.xy), 1.0);
#else
			return float4(LensBlur(_MainTex, sampler_MainTex, uv, Blur*DoFMaxBlur, _MainTex_TexelSize.xy), 1.0);
#endif
#endif

			float FrontBlur = GetDoFCoCFront(fDepth, fAF);
			FrontBlur = clamp(max(0.0, FrontBlur), 0.0, 1.0);

			float3 blurCol = (float3)0.0;
#if SHADER_API_VULKAN || SHADER_API_GLES3 || SHADER_API_METAL
			blurCol = LensBlurMobile(uv, Blur*DoFMaxBlur, _MainTex_TexelSize.xy);
#else
			blurCol = LensBlur(_MainTex, sampler_MainTex, uv, Blur*DoFMaxBlur, _MainTex_TexelSize.xy);
#endif
			float4 edgeBlur = UNITY_SAMPLE_TEX2D(_SecondaryTex, uv);

			if (Blur <= 0.0) {
				if (FrontBlur < 1.0) {
					blurCol *= (1.0 - FrontBlur);
				} else {
					blurCol = min(Blur, FrontBlur);
				}
			}

			return float4(blurCol + edgeBlur, 1);
		}

		half4 fragPRISMDoFPre(PostProcessVaryings i) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

			float2 uv = UnityStereoTransformScreenSpaceTex(i.texcoord);
#if !DOF_FRONTBLUR
			return UNITY_SAMPLE_TEX2D(_MainTex, uv);
			//return (float4)0.0;
#endif
			return DoFBokehTresholdPass(uv);
		}

		half4 fragPRISMDoFPreMobile(PostProcessVaryings i) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

			float2 uv = UnityStereoTransformScreenSpaceTex(i.texcoord);

			return DoFBokehTresholdMobilePass(uv);
		}
			
		half4 fragPRISMDoFCombine(PostProcessVaryings i) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

			float2 uv = UnityStereoTransformScreenSpaceTex(i.texcoord);
			//return UNITY_SAMPLE_TEX2D(_MainTex, uv);

			return DoFPass(uv);
		}

		half4 fragPRISMDoFMobileCombine(PostProcessVaryings i) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

			half2 uv = UnityStereoTransformScreenSpaceTex(i.texcoord);

			float4 edgeBlur = UNITY_SAMPLE_TEX2D(_SecondaryTex, uv);
			float4 blurCol = UNITY_SAMPLE_TEX2D(_MainTex, uv);

			//Normalise brightness
			float3 labBlur = RGBtoLab(blurCol.rgb);

			float4 targetFinalCol = lerp(blurCol, edgeBlur, edgeBlur.a);
			float diff = 1.0;
			float3 labFinal = RGBtoLab(targetFinalCol.rgb);
			diff = labBlur.r - labFinal.r;
			diff += 1.0;

			targetFinalCol.rgb = LabtoRGB(labFinal);
			targetFinalCol.rgb *= diff;

			return targetFinalCol;
		}

			
		ENDHLSL

		SubShader {

		//Downsample - 0
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "PRISMDoF0"
			HLSLPROGRAM
			#pragma target 4.0
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragPRISMDoFPre
			ENDHLSL
		}

		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "PRISMDoF1"
			HLSLPROGRAM
			#pragma target 4.0
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragPRISMDoFCombine
			ENDHLSL
		}

		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "PRISMDoFMobileCombine"
			HLSLPROGRAM
			#pragma target 4.0
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragPRISMDoFMobileCombine
			ENDHLSL
		}


			Cull Off ZWrite Off ZTest Always
			Pass
		{
			Name "PRISMDoFMobilePre"
			HLSLPROGRAM
			#pragma target 4.0
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragPRISMDoFPreMobile
			ENDHLSL
		}
	}

	Fallback off

}