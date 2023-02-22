//
// Kino/Motion - Motion blur effect
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//



Shader "PRISM/PRISMLongExposure"
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

	UNITY_DECLARE_TEX2D(_LongExpTex);
	float4 _LongExpTex_TexelSize;

	UNITY_DECLARE_TEX2D(_AccumMotion);
	float4 _AccumMotion_TexelSize;
	

	UNITY_DECLARE_TEX2D(_MotionVectorTexture);
	float4 _MotionVectorTexture_TexelSize;

	UNITY_DECLARE_TEX2D(_HistoryOne);
	UNITY_DECLARE_TEX2D(_HistoryTwo);
	UNITY_DECLARE_TEX2D(_HistoryThree);
	UNITY_DECLARE_TEX2D(_HistoryFour);

	uniform float individualExposureWeight;// 2.
	uniform float _Intensity;
	uniform float _ExposureTime;
	uniform float _ExposureTimestamp;
	uniform half4 _LastExposedHistory;
	uniform float4 _HistoryTimestampWeights;
	
	uniform float _HistoryTexExposureTimestampOne;
	uniform float _HistoryTexExposureTimestampTwo;
	uniform float _HistoryTexExposureTimestampThree;
	uniform float _HistoryTexExposureTimestampFour;

	float CUPPfrac(float x)
	{
		return x - floor(x);
	}

	float interleavedGradientNoise(float2 n) {
		n = floor((n + _Time.y) * _ScreenParams.xy);
		float f = 0.06711056 * n.x + 0.00583715 * n.y;
		return CUPPfrac(52.9829189 * CUPPfrac(f));
	}

	inline float4 SampleMainTexture(float2 uv)
	{
		return UNITY_SAMPLE_TEX2D(_MainTex, uv);
	}

	inline float4 SampleLongExpTexture(float2 uv)
	{
		return UNITY_SAMPLE_TEX2D(_LongExpTex, uv);
	}

	inline half2 SampleMotionTexture(float2 uv)
	{
		return UNITY_SAMPLE_TEX2D(_MotionVectorTexture, uv).rg;
	}

	inline float ClampMotionVector(inout float2 moVec)
	{
		const float _TempMaxBlurRadius = 16.0;
		moVec /= max(1, length(moVec) * _TempMaxBlurRadius);
		return moVec;
	}

	// Returns the largest vector of v1 and v2.
	float2 VMax(float2 v1, float2 v2)
	{
		return dot(v1, v1) < dot(v2, v2) ? v2 : v1;
	}

	// Fragment shader: TileMax filter (2 pixel width with normalization)
	half2 frag_TileMax1(PostProcessVaryings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

		float4 d = _MotionVectorTexture_TexelSize.xyxy * float4(-0.5, -0.5, 0.5, 0.5);

		half2 v1 = SampleMotionTexture(uv + d.xy);
		half2 v2 = SampleMotionTexture(uv + d.zy);
		half2 v3 = SampleMotionTexture(uv + d.xw);
		half2 v4 = SampleMotionTexture(uv + d.zw);

		return half2(VMax(VMax(VMax(v1, v2), v3), v4));
	}

	// Fragment shader: TileMax filter (2 pixel width)
	half2 frag_TileMax2(PostProcessVaryings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

		float4 d = _MotionVectorTexture_TexelSize.xyxy * float4(-0.5, -0.5, 0.5, 0.5);

		half2 v1 = SampleMotionTexture(uv + d.xy);
		half2 v2 = SampleMotionTexture(uv + d.zy);
		half2 v3 = SampleMotionTexture(uv + d.xw);
		half2 v4 = SampleMotionTexture(uv + d.zw);

		return half2(VMax(VMax(VMax(v1, v2), v3), v4));
	}

	// Returns the largest vector of v1 and v2.
	half2 VMax(half2 v1, half2 v2)
	{
		return dot(v1, v1) < dot(v2, v2) ? v2 : v1;
	}

	// Fragment shader: NeighborMax filter
	half2 frag_NeighborMax(PostProcessVaryings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

		const half cw = 1.01f; // center weight tweak

		float4 d = _MotionVectorTexture_TexelSize.xyxy * float4(1, 1, -1, 0);

		half2 v1 = SampleMotionTexture(uv - d.xy);
		half2 v2 = SampleMotionTexture(uv - d.wy);
		half2 v3 = SampleMotionTexture(uv - d.zy);

		half2 v4 = SampleMotionTexture(uv - d.xw);
		half2 v5 = SampleMotionTexture(uv) * cw;
		half2 v6 = SampleMotionTexture(uv + d.xw);

		half2 v7 = SampleMotionTexture(uv + d.zy);
		half2 v8 = SampleMotionTexture(uv + d.wy);
		half2 v9 = SampleMotionTexture(uv + d.xy);

		half2 va = VMax(v1, VMax(v2, v3));
		half2 vb = VMax(v4, VMax(v5, v6));
		half2 vc = VMax(v7, VMax(v8, v9));

		return half2(VMax(va, VMax(vb, vc)) / cw);
	}


	// Fragment shader: TileMax filter (2 pixel width with normalization)
	half2 frag_AccumMotion(PostProcessVaryings input) : SV_Target
	{

		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

		float2 newMotion = SampleMotionTexture(uv);
		float2 oldMotion = UNITY_SAMPLE_TEX2D(_AccumMotion, uv);

		return lerp(oldMotion, newMotion, 0.1);
	}


	// Fragment shader: TileMax filter (2 pixel width with normalization)
	float4 frag_AccumMotionDebug(PostProcessVaryings input) : SV_Target
	{

		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

		float2 oldMotion = UNITY_SAMPLE_TEX2D(_AccumMotion, uv);

		oldMotion = abs(oldMotion);
		oldMotion *= 10.0;

		return float4(oldMotion.r, oldMotion.g, 0.0, 1.0);
	}

	// Fragment shader: TileMax filter (2 pixel width with normalization)
	float4 fragCombineFrames(PostProcessVaryings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

		float4 currentFrame = UNITY_SAMPLE_TEX2D(_MainTex, uv);
		float4 oldMotion = SampleLongExpTexture(uv);
		//oldMotion *= 1111110.1;
		//currentFrame *= 111111.0;
		return max(currentFrame, oldMotion);
		return lerp(currentFrame, oldMotion, 0.5);
	}

	float4 fragAddNewFrameToLongExp(PostProcessVaryings input) : SV_Target
	{
		float4 col = 0.0;

		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

		float4 tex = SampleMainTexture(uv);

		float4 lexptex = SampleLongExpTexture(uv);

		float2 oldMotion = UNITY_SAMPLE_TEX2D(_AccumMotion, uv);

		// NeighborMax vector sample at the center point
		half l_v_max = length(oldMotion);
		l_v_max *= 1000000.0;
		const half rcp_l_v_max = 1 / l_v_max;

		// Escape early if the NeighborMax vector is small enough.
		//if (l_v_max < 0.000005) return tex;

		//Can be negative
		float2 motion_middle = SampleMotionTexture(uv).rg;
		const half l_v_p = length(motion_middle);
		float2 moVecWeight = VMax(motion_middle, oldMotion);
		
		ClampMotionVector(moVecWeight);

		//Get a pixel space vector for motion weight
		moVecWeight *= _Intensity * _MotionVectorTexture_TexelSize.xy;
		
		//if (!any(moVecWeight))
		//{
			//DEBUG COMMENT NEXT LINE OUT
			//return tex;
			
			//moVecWeight = moVecWeight;
		//}

		float2 deltaTexCoord = moVecWeight;

		float rand = interleavedGradientNoise(uv / _MainTex_TexelSize.xy) - 0.5;

		float randMult = 1.0;
		if (!any(moVecWeight))
		{
			randMult = 0.0;
		}

		//Get middle depth
		float d_mid = SampleSceneDepth(uv);
		d_mid = Linear01Depth(d_mid);
		const half reciprocal_depth_mid = 1 / d_mid;

		float4 historyTexWeights = float4(1.0, 1.0, 1.0, 1.0);
		historyTexWeights *= _HistoryTimestampWeights;

		float4 histOneCol = 0.0;
		float4 histTwoCol = 0.0;
		float4 histThreeCol = 0.0;
		float4 histFourCol = 0.0;

		float2 tempUV = uv;

		//TODO: Need to make areas that should be blurred, not take blur colour from areas that are stationary. Similar to DoF problem. Potentially depth check?
		for (int i = 0; i < 8; i++)
		{
			rand *= randMult;
			tempUV = (tempUV - deltaTexCoord);
			tempUV += rand * _MainTex_TexelSize.xy;
			rand = interleavedGradientNoise(tempUV / _MainTex_TexelSize.xy) - 0.5;

			// Background/Foreground separation      //      1/z
			float d_sample = SampleSceneDepth(tempUV);
			d_sample = Linear01Depth(d_sample);
			//half fg = saturate((d - vd) * 1 * reciprocal_depth_mid);
			half fg = (d_sample - d_mid);// *reciprocal_depth_mid;

			float2 newVeloc = 
				//SampleMotionTexture(tempUV).rg;
				UNITY_SAMPLE_TEX2D(_AccumMotion, tempUV);
			//fg += 0.000000001;
			
			fg += length(newVeloc);
			fg *= 20.0;

			//Now we have a mask. Sign(fg) will return 1.0 for anything that we should blur, and 0.0 for anything we shouldn't.
			//tempUV = lerp(uv, tempUV, sign(fg));
			
			tempUV = lerp(uv, tempUV, sign(fg));
			
			//return lerp((float4)0.0, (float4)(1.0), fg); // sign(fg)

			//float2 newVeloc = UNITY_SAMPLE_TEX2D(_AccumMotion, tempUV);

			// Length of the velocity vector
			//const half l_v = lerp(l_v_p, max(length(newVeloc), 1.0), fg);

			// Sample weight
			// (Distance test) * (Spreading out by motion) * (Triangular window)//
			//const half w = 1.0 / l_v * (1.2);

			//return lerp((float4)0.0, (float4)(1.0), w);

			float4 mainTexCol = SampleMainTexture(tempUV) * 0.2;
			col += mainTexCol;

			histOneCol += UNITY_SAMPLE_TEX2D(_HistoryOne, tempUV) * historyTexWeights.x;

			histTwoCol += UNITY_SAMPLE_TEX2D(_HistoryTwo, tempUV) * historyTexWeights.y;

			histThreeCol += UNITY_SAMPLE_TEX2D(_HistoryThree, tempUV) * historyTexWeights.z;

			histFourCol += UNITY_SAMPLE_TEX2D(_HistoryFour, tempUV) * historyTexWeights.w;
		}

		histOneCol /= 8;
		histTwoCol /= 8;
		histThreeCol /= 8;
		histFourCol /= 8;

		float sumWeights = historyTexWeights.x + historyTexWeights.y + historyTexWeights.z + historyTexWeights.w;

		col += histOneCol;
		col += histTwoCol;
		col += histThreeCol;
		col += histFourCol;
		col /= (1.6 + sumWeights);

		float4 outColor = (float4)0.0;

		outColor = UNITY_SAMPLE_TEX2D(_HistoryOne, uv);

		//outColor = UNITY_SAMPLE_TEX2D(_HistoryTwo, uv);

		//outColor = UNITY_SAMPLE_TEX2D(_HistoryThree, uv);
		
		//outColor = UNITY_SAMPLE_TEX2D(_HistoryFour, uv);

		//return outColor;

		//Depth check
		//float d = SampleSceneDepth(uv);
		//d = Linear01Depth(d);
		//float d = UNITY_SAMPLE_TEX2D(_MotionVectorTexture, uv).b;
		//float4 ret = float4(1.0, 0.0, 0.0, 1.0);
		//float4 zeros = (float4)0.0;
		//col = lerp(ret, zeros, d);

		return col;

		/*float4 outColor = lerp(lexptex, tex, individualExposureWeight * moVecWeight);
			//(tex + lexptex) * 0.5;

		if (uv.y < 0.1) {
			outColor = UNITY_SAMPLE_TEX2D(_HistoryOne, uv);
		}

		if (uv.y > 0.1 && uv.y < 0.2) {
			outColor = UNITY_SAMPLE_TEX2D(_HistoryTwo, uv);
		}

		if (uv.y > 0.8 && uv.y < 0.9) {
			outColor = UNITY_SAMPLE_TEX2D(_HistoryThree, uv);
		}

		if (uv.y > 0.9 && uv.y < 1.0) {
			outColor = UNITY_SAMPLE_TEX2D(_HistoryThree, uv);
		}

		return outColor;*/
	}


		
		ENDHLSL

	SubShader
	{

		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "0Motion Vector Max Prepass "
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment frag_TileMax1
			ENDHLSL
		}

		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "1Initial LongExposure"
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragAddNewFrameToLongExp
			ENDHLSL
		}

		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "2MRT blit normal and MoVec"
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragAddNewFrameToLongExp
			ENDHLSL
		}

		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "3Accum Motion"
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment frag_AccumMotion
			ENDHLSL
		}

		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "4Accum Motion Debug"
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment frag_AccumMotionDebug
			ENDHLSL
		}


		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "5Combine frames"
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragCombineFrames
			ENDHLSL
		}

		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "6 tilemax2 "
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment frag_TileMax2
			ENDHLSL
		}

		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "7 neighbourmax"
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment frag_NeighborMax
			ENDHLSL
		}

			
	}
	Fallback Off
}