Shader "Hidden/PRISMStack"
{
	HLSLINCLUDE
	#pragma exclude_renderers gles
	#pragma target 3.5

	#include "CUPPCore.hlsl"
	#include "PRISMColorSpaces.hlsl"
	#include "PRISMColorTemperature.hlsl"
	#include "PRISMTonemapping.hlsl"
	#include "PRISMLensDefects.hlsl"
	#include "PRISMNoiseAndGrain.hlsl"
	#include "PRISMCurves.hlsl"
	#include "PRISMCG.hlsl"
	#define PRISM_LINEAR_LOOKUP 1

	UNITY_DECLARE_TEX2D(_MainTex);

	UNITY_DECLARE_TEX2D(_SecondaryTex);

	float4 _MainTex_TexelSize;
	float _ColorTemperature;
	float _ColorTint;
	float _a_LabValue;
	float _b_LabValue;
	int _Tonemap;
	float _Gamma;

	float _GlassDispersion;
	float _PetzvalDistortion;
	float _GlassAngle;
	float _NegativeBarrel;
	float3 _ColorDistort;

	float _Exposure;
	float _Lightness;
	float _LightnessSlope;

	//Grain
	float _GrainScale;
	float _GrainIntensity;
	float4 _GrainSaturations;
	float4 _GrainTVValues;

	float _DitherBitDepth;
	float _UseDither;

	//Bloom
	//TODO
	UNITY_DECLARE_TEX2D(_BloomTex);
	UNITY_DECLARE_TEX2D(_BloomTexQuarter);
	UNITY_DECLARE_TEX2D(_BloomTexEighth);
	UNITY_DECLARE_TEX2D(_BloomTexSixteenth);
	UNITY_DECLARE_TEX2D(_BloomTexThirtySecond);
	UNITY_DECLARE_TEX2D(_BloomTexSixtyFourth);
	UNITY_DECLARE_TEX2D(_BloomTexLast);


	float4 _BloomTex_TexelSize;
	float4 _BloomTexQuarter_TexelSize;
	float4 _BloomTexEighth_TexelSize;
	float4 _BloomTexSixteenth_TexelSize;
	float4 _BloomTexThirtySecond_TexelSize;
	float4 _BloomTexSixtyFourth_TexelSize;

	UNITY_DECLARE_TEX2D(_BloomDirtTex);
	
	float4 _BloomTexLast_TexelSize;

	float4 _BloomTex_ST;
	float4 _BloomTexQuarter_ST;
	float4 _BloomTexEighth_ST;
	float4 _BloomTexSixteenth_ST;

#define DIRECTIONONE float2(0.0, 1.25)
#define DIRECTIONTWO float2(1.25, 0.0)
#define DIRECTIONTHREE float2(0.0, 1.35)
#define DIRECTIONFOUR float2(1.35, 0.0)
#define DIRECTIONFIVE float2(0.0, 1.5)
#define DIRECTIONSIX float2(1.5, 0.0)

#if !BLOOMCLARITY
#define bloomMults float4(0.3, 1.5, 1.8, 3.1)
#define bloomMultsMid float2(1.0, 6.0)
#else
#define bloomMults2 float4(0.5, 1.0, 1.4, 1.7)
#define bloomMultsMid2 float2(0.85, 2.0)
#define bloomMults float4(0.05, 0.1, 0.14, 0.17)
#define bloomMultsMid float2(0.085, 0.2)
#endif

	float4 _BloomDirtTex_TexelSize;
	float _LensDirtExposure;
	float _BloomStrength;
	float _BloomThreshold;
	float _BloomSaturation;
	float _BloomRadius;

	//Vig
	float _VignetteStrength;

	//Rays
	//TODO
	//TEXTURE2D_SAMPLER2D(_RaysTex, sampler_RaysTex);
	float4 _RaysTex_TexelSize;

	//White point
	float4 _ColorRangeShift;
	float _ColorVibrance;
	float4 _ShadowValue;
	float4 _MiddleValue;
	float4 _GainValue;
	float4 _HighlightsValue;
	float4 _GraysValue;
	float _Contrast;
	float3 _SaturationValues;

	uniform float _SunWeight;
	uniform float _SunExposure;
	uniform int _DownsampleAmount;

	uniform float _LutScale;
	uniform float _LutOffset;
	uniform float _LutAmount;

	UNITY_DECLARE_TEX3D(_LutTex);
	UNITY_DECLARE_TEX3D(_SecondLutTex);
	uniform float _SecondLutScale;
	uniform float _SecondLutOffset;
	uniform float _SecondLutAmount;
	//End LUT variables

	#define HALF_MAX 65504.0
	#define ZEROES float4(0.0,0.0,0.0,0.0)

#define TAPS 17
#define VIRTUAL_TAPS	1 + (TAPS - 1) / 2
#define LOOPS		6 - (VIRTUAL_TAPS - 1) / 2	// 4, 3, 2

	//FS Tex
	UNITY_DECLARE_TEX2D(_FSOverlayTex);
	//TEXTURE2D_SAMPLER2D(_FSOverlayTex, sampler_FSOverlayTex);
	float4 _FSOverlayTex_TexelSize;


	VS_OUTPUT FsTrianglePostProcessVertexProgramBloom(FullScreenTrianglePostProcessAttributes input)
	{
		VS_OUTPUT output;
		UNITY_SETUP_INSTANCE_ID(input);
		output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);

		texelSize = _MainTex_TexelSize.xy;

		float2 centerTap = GetFullScreenTriangleTexCoord(input.vertexID);// +texelSize / 2;
		output.centerTap = centerTap;

		for (int i = 0; i < LOOPS; i++)
		{
			float tapOffset = i * 4 + 1.5 + tapOffsets[i * 2]; // { 1.5, 5.5 }
			output.positiveTaps[i].xy = centerTap + tapOffset * DIRECTIONONE * texelSize;
			output.negativeTaps[i].xy = centerTap - tapOffset * DIRECTIONONE * texelSize;
			tapOffset = (i * 2 + 1) * 2 + 1.5 + tapOffsets[i * 2 + 1]; // { 3.5, 7.5 }
			output.positiveTaps[i].zw = centerTap + tapOffset * DIRECTIONONE * texelSize;
			output.negativeTaps[i].zw = centerTap - tapOffset * DIRECTIONONE * texelSize;
		}

		return output;
	}


	VS_OUTPUT FsTrianglePostProcessVertexProgramBloomTwo(FullScreenTrianglePostProcessAttributes input)
	{
		VS_OUTPUT output;
		UNITY_SETUP_INSTANCE_ID(input);
		output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);

		texelSize = _MainTex_TexelSize.xy;

		float2 centerTap = GetFullScreenTriangleTexCoord(input.vertexID);// + texelSize / 2;
		output.centerTap = centerTap;

		for (int i = 0; i < LOOPS; i++)
		{
			float tapOffset = i * 4 + 1.5 + tapOffsets[i * 2]; // { 1.5, 5.5 }
			output.positiveTaps[i].xy = centerTap + tapOffset * DIRECTIONTWO * texelSize;
			output.negativeTaps[i].xy = centerTap - tapOffset * DIRECTIONTWO * texelSize;
			tapOffset = (i * 2 + 1) * 2 + 1.5 + tapOffsets[i * 2 + 1]; // { 3.5, 7.5 }
			output.positiveTaps[i].zw = centerTap + tapOffset * DIRECTIONTWO * texelSize;
			output.negativeTaps[i].zw = centerTap - tapOffset * DIRECTIONTWO * texelSize;
		}

		return output;
	}


	VS_OUTPUT FsTrianglePostProcessVertexProgramBloomThree(FullScreenTrianglePostProcessAttributes input)
	{
		VS_OUTPUT output;
		UNITY_SETUP_INSTANCE_ID(input);
		output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);

		texelSize = _MainTex_TexelSize.xy;

		float2 centerTap = GetFullScreenTriangleTexCoord(input.vertexID);// + texelSize / 2;
		output.centerTap = centerTap;

		for (int i = 0; i < LOOPS; i++)
		{
			float tapOffset = i * 4 + 1.5 + tapOffsets[i * 2]; // { 1.5, 5.5 }
			output.positiveTaps[i].xy = centerTap + tapOffset * DIRECTIONTHREE * texelSize;
			output.negativeTaps[i].xy = centerTap - tapOffset * DIRECTIONTHREE * texelSize;
			tapOffset = (i * 2 + 1) * 2 + 1.5 + tapOffsets[i * 2 + 1]; // { 3.5, 7.5 }
			output.positiveTaps[i].zw = centerTap + tapOffset * DIRECTIONTHREE * texelSize;
			output.negativeTaps[i].zw = centerTap - tapOffset * DIRECTIONTHREE * texelSize;
		}

		return output;
	}


	VS_OUTPUT FsTrianglePostProcessVertexProgramBloomFour(FullScreenTrianglePostProcessAttributes input)
	{
		VS_OUTPUT output;
		UNITY_SETUP_INSTANCE_ID(input);
		output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);

		texelSize = _MainTex_TexelSize.xy;

		float2 centerTap = GetFullScreenTriangleTexCoord(input.vertexID);// + texelSize / 2;
		output.centerTap = centerTap;

		for (int i = 0; i < LOOPS; i++)
		{
			float tapOffset = i * 4 + 1.5 + tapOffsets[i * 2]; // { 1.5, 5.5 }
			output.positiveTaps[i].xy = centerTap + tapOffset * DIRECTIONFOUR * texelSize;
			output.negativeTaps[i].xy = centerTap - tapOffset * DIRECTIONFOUR * texelSize;
			tapOffset = (i * 2 + 1) * 2 + 1.5 + tapOffsets[i * 2 + 1]; // { 3.5, 7.5 }
			output.positiveTaps[i].zw = centerTap + tapOffset * DIRECTIONFOUR * texelSize;
			output.negativeTaps[i].zw = centerTap - tapOffset * DIRECTIONFOUR * texelSize;
		}

		return output;
	}



	VS_OUTPUT FsTrianglePostProcessVertexProgramBloomFive(FullScreenTrianglePostProcessAttributes input)
	{
		VS_OUTPUT output;
		UNITY_SETUP_INSTANCE_ID(input);
		output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);

		texelSize = _MainTex_TexelSize.xy;

		float2 centerTap = GetFullScreenTriangleTexCoord(input.vertexID);// + texelSize / 2;
		output.centerTap = centerTap;

		for (int i = 0; i < LOOPS; i++)
		{
			float tapOffset = i * 4 + 1.5 + tapOffsets[i * 2]; // { 1.5, 5.5 }
			output.positiveTaps[i].xy = centerTap + tapOffset * DIRECTIONFIVE * texelSize;
			output.negativeTaps[i].xy = centerTap - tapOffset * DIRECTIONFIVE * texelSize;
			tapOffset = (i * 2 + 1) * 2 + 1.5 + tapOffsets[i * 2 + 1]; // { 3.5, 7.5 }
			output.positiveTaps[i].zw = centerTap + tapOffset * DIRECTIONFIVE * texelSize;
			output.negativeTaps[i].zw = centerTap - tapOffset * DIRECTIONFIVE * texelSize;
		}

		return output;
	}



	VS_OUTPUT FsTrianglePostProcessVertexProgramBloomSix(FullScreenTrianglePostProcessAttributes input)
	{
		VS_OUTPUT output;
		UNITY_SETUP_INSTANCE_ID(input);
		output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);

		texelSize = _MainTex_TexelSize.xy;

		float2 centerTap = GetFullScreenTriangleTexCoord(input.vertexID);// + texelSize / 2;
		output.centerTap = centerTap;

		for (int i = 0; i < LOOPS; i++)
		{
			float tapOffset = i * 4 + 1.5 + tapOffsets[i * 2]; // { 1.5, 5.5 }
			output.positiveTaps[i].xy = centerTap + tapOffset * DIRECTIONSIX * texelSize;
			output.negativeTaps[i].xy = centerTap - tapOffset * DIRECTIONSIX * texelSize;
			tapOffset = (i * 2 + 1) * 2 + 1.5 + tapOffsets[i * 2 + 1]; // { 3.5, 7.5 }
			output.positiveTaps[i].zw = centerTap + tapOffset * DIRECTIONSIX * texelSize;
			output.negativeTaps[i].zw = centerTap - tapOffset * DIRECTIONSIX * texelSize;
		}

		return output;
	}



	inline float4 lookupLinear(int texRef, float4 c, half2 uv)
	{
		//float4 c = tex2D(_MainTex, uv);
#if !UNITY_COLORSPACE_GAMMA
		c.rgb = sqrt(c.rgb);
#endif
		if (texRef == 1)
		{
			c.rgb = UNITY_SAMPLE_TEX3D(_LutTex, c.rgb * _LutScale + _LutOffset).rgb;
		}
		else {
			c.rgb = UNITY_SAMPLE_TEX3D(_SecondLutTex, c.rgb * _LutScale + _LutOffset).rgb;
		}
#if !UNITY_COLORSPACE_GAMMA
		c.rgb = c.rgb*c.rgb;
#endif
		return c;
	}

	float GetChromaticFringeMainTex(float2 uv, float gA, float2 fromCentre)
	{
		float2 uvR = uv;
		float4 gbUVs = ChromaticAberrationGetThreeUVs(uvR, fromCentre, _GlassDispersion, _MainTex_TexelSize, gA);

		//Just Blue
		float mainColFringeBlue = UNITY_SAMPLE_TEX2D(_MainTex, uvR).b +
			UNITY_SAMPLE_TEX2D(_MainTex, gbUVs.rg).b +
			UNITY_SAMPLE_TEX2D(_MainTex, gbUVs.ba).b;
		//No integer divide coz GCN fucks it and is v slow
		mainColFringeBlue *= 0.33333;

		float softBlueCol = 1.0 - exp(-mainColFringeBlue);
		softBlueCol = pow(softBlueCol, 4);
		mainColFringeBlue *= softBlueCol;

		return mainColFringeBlue;
	}

	float GetChromaticFringeMainTexR(float2 uv, float gA, float redDispersion, float2 fromCentre, float inR)
	{
#if FRINGING
		return inR;
#else
		float2 uvR = uv;
		float4 gbUVs = ChromaticAberrationGetThreeUVs(uvR, fromCentre, redDispersion, _MainTex_TexelSize, gA);

		//Just Blue
		float mainColFringeBlue = UNITY_SAMPLE_TEX2D(_MainTex, uvR).r +
			UNITY_SAMPLE_TEX2D(_MainTex, gbUVs.rg).r +
			UNITY_SAMPLE_TEX2D(_MainTex, gbUVs.ba).r;
		//No integer divide coz GCN fucks it and is v slow
		mainColFringeBlue *= 0.3333;

		return mainColFringeBlue;
#endif
	}

	float GetChromaticFringeMainTexG(float2 uv, float gA, float redDispersion, float2 fromCentre, float inG)
	{
#if FRINGING
		return inG;
#else
		float2 uvR = uv;
		float4 gbUVs = ChromaticAberrationGetThreeUVs(uvR, fromCentre, redDispersion, _MainTex_TexelSize, gA);

		//Just Blue
		float mainColFringeBlue = UNITY_SAMPLE_TEX2D(_MainTex, uvR).g +
			UNITY_SAMPLE_TEX2D(_MainTex, gbUVs.rg).g +
			UNITY_SAMPLE_TEX2D(_MainTex, gbUVs.ba).g;
		//No integer divide coz GCN fucks it and is v slow
		mainColFringeBlue *= 0.3333;

		return mainColFringeBlue;
#endif
	}

	float3 highlights(float3 col, float thres)
	{
		//Gradual
		col *= saturate(col.r / thres);
		return col;
	}

	float3 highlightsKeijiro(float3 col, float thres)
	{
#if BLOOMCLARITY
		// Pixel brightness
		half br = PRISMLuma(col);

		// Under-threshold part: quadratic curve
		half rq = clamp(br - 5., 0.0, 65472.0);
		rq = 0.5 * rq * rq;

		// Combine and apply the brightness response curve.
		col *= max(rq, br - thres) / max(br, 1e-4);
#else
		return ThresholdColor(col, thres);
#endif
		return col;
	}

	float3 BloomHighlightsLAB(float3 col, float thresh)
	{
		float3 labConvertedMainColF = RGBtoLab(col.rgb);
		float3 ogCol = labConvertedMainColF;

		labConvertedMainColF = CurvesPassThresh(labConvertedMainColF, thresh);

		col = LabtoRGB(float3(labConvertedMainColF.r, ogCol.g, ogCol.b));

		return col;
	}


	half4 fragDownsampleMobile(float2 uv)
	{
		half2 ofs = _MainTex_TexelSize.xy * half2(1.0, -1.0); 

		half4 vTexels = UNITY_SAMPLE_TEX2D(_MainTex, uv);
		vTexels = max(vTexels, UNITY_SAMPLE_TEX2D(_MainTex, uv - ofs));
		return max(vTexels, UNITY_SAMPLE_TEX2D(_MainTex, uv + ofs));
	}



	float4 fragDownsampleGauss(float2 uv)
	{
#if SHADER_API_VULKAN || SHADER_API_GLES3 || SHADER_API_METAL
		return KawaseBlurMobile(_MainTex, uv, 1, _MainTex_TexelSize.xy);
#else
		return KawaseBlur(_MainTex, uv, _DownsampleAmount, sampler_MainTex, _MainTex_TexelSize.xy);
#endif
	}

	float4 fragMedMedium(float2 uv)
	{
		float2 ooRes = _MainTex_TexelSize.xy;//_ScreenParams.w;

		//This doesn't work on SM2
		float3 ofs = _MainTex_TexelSize.xyx * float3(1.0, 1.0, 0.0);
		//
		float3 v[5];

		float4 midCol = UNITY_SAMPLE_TEX2D(_MainTex, uv);

		v[0] = midCol.rgb;
		v[1] = UNITY_SAMPLE_TEX2D(_MainTex, uv - ofs.xz).rgb;
		v[2] = UNITY_SAMPLE_TEX2D(_MainTex, uv + ofs.xz).rgb;
		v[3] = UNITY_SAMPLE_TEX2D(_MainTex, uv - ofs.zy).rgb;
		v[4] = UNITY_SAMPLE_TEX2D(_MainTex, uv + ofs.zy).rgb;

		float3 temp;
		mnmx5(v[0], v[1], v[2], v[3], v[4]);
		mnmx3(v[1], v[2], v[3]);

		return float4(SafeHDR3(v[2].rgb), midCol.a);
	}

	float4 fragMedLarge(float2 uv)
	{
		float2 ooRes = _MainTex_TexelSize.xy;//_ScreenParams.w;

		float4 midCol = UNITY_SAMPLE_TEX2D(_MainTex, uv);

		// -1  1  1
		// -1  0  1 
		// -1 -1  1  //3x3=9	
		float3 v[9];

		// Add the pixels which make up our window to the pixel array.
		[unroll]
		for (int dX = -1; dX <= 1; ++dX)
		{
			[unroll]
			for (int dY = -1; dY <= 1; ++dY)
			{
				float2 ofst = float2(float(dX), float(dY));

				// If a pixel in the window is located at (x+dX, y+dY), put it at index (dX + R)(2R + 1) + (dY + R) of the
				// pixel array. This will fill the pixel array, with the top left pixel of the window at pixel[0] and the
				// bottom right pixel of the window at pixel[N-1].
				v[(dX + 1) * 3 + (dY + 1)] = UNITY_SAMPLE_TEX2D(_MainTex, ofst * ooRes + uv).rgb;
			}
		}

		float3 temp;

		// Starting with a subset of size 6, remove the min and max each time
		mnmx6(v[0], v[1], v[2], v[3], v[4], v[5]);
		mnmx5(v[1], v[2], v[3], v[4], v[6]);
		mnmx4(v[2], v[3], v[4], v[7]);
		mnmx3(v[3], v[4], v[8]);

		return float4(SafeHDR3(v[4].rgb), midCol.a);
	}

	float4 fragBloomMedFast(float2 uv)//, float _Exposure)
	{
		float2 ooRes = _MainTex_TexelSize.xy;//_ScreenParams.w;

		float4 midCol = fragDownsampleMobile(uv);
		midCol.rgb = exp2(_Exposure)*midCol.rgb;

		midCol.rgb = highlights(midCol.rgb, _BloomThreshold);

		return float4(midCol.rgb, midCol.a);
	}

	float4 bloomMedRGB(float2 uv)//, float _Exposure)
	{
		float2 ooRes = _MainTex_TexelSize.xy;//_ScreenParams.w;

		float3 ofs = _MainTex_TexelSize.xyx * float3(1, 1, 0);
		//
		float3 v[5];

		float4 midCol = UNITY_SAMPLE_TEX2D(_MainTex, uv);

#if BLOOMSTAB
		v[0] = midCol.rgb;
		v[1] = UNITY_SAMPLE_TEX2D(_MainTex, uv - ofs.xz).rgb;
		v[2] = UNITY_SAMPLE_TEX2D(_MainTex, uv - ofs.xz).rgb;
		v[3] = UNITY_SAMPLE_TEX2D(_MainTex, uv - ofs.zy).rgb;
		v[4] = UNITY_SAMPLE_TEX2D(_MainTex, uv + ofs.zy).rgb;

		float3 temp;
		mnmx5(v[0], v[1], v[2], v[3], v[4]);
		mnmx3(v[1], v[2], v[3]);

		v[2].rgb = exp2(_Exposure)*v[2].rgb;

		v[2].rgb = highlightsKeijiro(v[2].rgb, _BloomThreshold);


		return float4(min(v[2].rgb, 65504.0), midCol.a);
#endif

		midCol.rgb = exp2(_Exposure)*midCol.rgb;

		midCol.rgb = highlightsKeijiro(midCol.rgb, _BloomThreshold);

		//midCol.rgb = ThresholdSmooth(midCol.rgb.rgb, 1.0);
		return float4(min(midCol.rgb, 65504.0), midCol.a);
	}

	float4 bloomMedBigRGB(float2 uv)
	{
		float4 col = fragMedLarge(uv);
		col.rgb = ThresholdColor(col.rgb, _BloomThreshold);
		//col.rgb = ThresholdSmooth(col.rgb, _BloomSaturation);
		return float4(SafeHDR3(col.rgb), col.a);
	}
	

	float4 fragBloomPrepassMobile(PostProcessVaryings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		float2 uv = UnityStereoTransformScreenSpaceTex(i.texcoord);

		//return bloomMedBigRGB(uv);
		return fragBloomMedFast(uv) * _BloomStrength;
	}

	float4 fragBloomMedianFull(PostProcessVaryings i) : SV_Target
	{ 
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		float2 uv = UnityStereoTransformScreenSpaceTex(i.texcoord);
		
		//return bloomMedBigRGB(uv);
		return bloomMedRGB(uv);
	}

	static const half coefficients[5] = { 0.0625, 0.25, 0.375, 0.25, 0.0625 };
	uniform float2 offsets[5];

	//https://d3cw3dd2w32x2b.cloudfront.net/wp-content/uploads/2012/06/faster_filters.pdf
	float4 fragBlurHorizontalPRISM(PostProcessVaryings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		float2 uv = UnityStereoTransformScreenSpaceTex(i.texcoord);
		half offsOne = 1.5 * _MainTex_TexelSize.x;
		half offsTwo = 3.5 * _MainTex_TexelSize.x;
		half4 c1 = UNITY_SAMPLE_TEX2D(_MainTex, uv - half2(offsTwo, 0.0));
		half4 c2 = UNITY_SAMPLE_TEX2D(_MainTex, uv - half2(offsOne, 0.0));
		half4 c3 = UNITY_SAMPLE_TEX2D(_MainTex, uv);
		half4 c4 = UNITY_SAMPLE_TEX2D(_MainTex, uv + half2(offsOne, 0.0));
		half4 c5 = UNITY_SAMPLE_TEX2D(_MainTex, uv + half2(offsTwo, 0.0));

		c1 *= coefficients[0];
		c2 *= coefficients[1];
		c3 *= coefficients[2];
		c4 *= coefficients[3];
		c5 *= coefficients[4];

		return c1 + c2 + c3 + c4 + c5;
	}

	half4 fragBlurVerticalPRISM(PostProcessVaryings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		float2 uv = UnityStereoTransformScreenSpaceTex(i.texcoord);

		float2 ps = _MainTex_TexelSize.xy;
		half offsOne = 1.5 * ps.y;
		half offsTwo = 3.5 * ps.y;
		half4 c1 = UNITY_SAMPLE_TEX2D(_MainTex, uv - half2(0.0, offsTwo ));
		half4 c2 = UNITY_SAMPLE_TEX2D(_MainTex, uv - half2(0.0, offsOne));
		half4 c3 = UNITY_SAMPLE_TEX2D(_MainTex, uv);
		half4 c4 = UNITY_SAMPLE_TEX2D(_MainTex, uv + half2(0.0, offsOne));
		half4 c5 = UNITY_SAMPLE_TEX2D(_MainTex, uv + half2(0.0 , offsTwo ));

		c1 *= 0.055028;
		c2 *= 0.244038;
		c3 *= 0.401870;
		c4 *= 0.244038;
		c5 *= 0.055028;

		return c1 + c2 + c3 + c4 + c5;
	}

	float4 PS(VS_OUTPUT IN) : SV_Target
	{
		float4x4 positiveSamples;
		float4x4 negativeSamples;

		float4 centerSample = UNITY_SAMPLE_TEX2D(_MainTex, IN.centerTap) * centerTapWeight;

		UNITY_UNROLL
		for (int i = 0; i < LOOPS; i++)
		{
			positiveSamples[i * 2] = UNITY_SAMPLE_TEX2D(_MainTex, IN.positiveTaps[i].xy);
			negativeSamples[i * 2] = UNITY_SAMPLE_TEX2D(_MainTex, IN.negativeTaps[i].xy);
			positiveSamples[i * 2 + 1] = UNITY_SAMPLE_TEX2D(_MainTex, IN.positiveTaps[i].zw);
			negativeSamples[i * 2 + 1] = UNITY_SAMPLE_TEX2D(_MainTex, IN.negativeTaps[i].zw);
		}

		return
				centerSample +
				mul(tapWeights, positiveSamples) + mul(tapWeights, negativeSamples)
		;
	}

	half4 fragPRISMStack(PostProcessVaryings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		float2 uv = UnityStereoTransformScreenSpaceTex(i.texcoord);
		//float2 uv = i.texcoord;

		float4 mainCol = (float4)0.0;

#if BARRELDIST || FRINGING || CHROMAB
		float amount = 0.0;
		uv.xy = LensDistortFisheye(uv, _GlassAngle, _NegativeBarrel, amount);
		float2 uvAdd = PetzvalDistortMask(uv).rg;

		// correct for aspect ratio
		float2 d = abs(uv - float2(0.5, 0.5));
		d.x *= _MainTex_TexelSize.z / _MainTex_TexelSize.w;
		d.x *= 1.75;
		d.x = dot(d, d);

		//d = abs(uv - (float2)0.5) * 2.0f;
		//d *= 2.0;
		d = pow(d, (float2)2.0);
		d = max(0.000001, d);
		float petzvalStr = 200.0 * d;

		uvAdd -= 0.5;
		uv -= _MainTex_TexelSize.xy * uvAdd * _PetzvalDistortion * petzvalStr;
		if (_PetzvalDistortion < 0.0)
		{
			//Scale
			float normPetz = _PetzvalDistortion + 0.5;
			//Now normPetz is 0-0.5. At 0 we lerp 0. By the time it is halfway to 1.46 (=1.0), normPetz is 0
			uv = (uv - 0.5) * lerp(0.73, 1.27, normPetz) + 0.5;// abs((abs(_PetzvalDistortion) * -1.46)) + 0.5;
		}
		mainCol = UNITY_SAMPLE_TEX2D(_MainTex, uv);


		#if FRINGING || CHROMAB
		float2 fromCentre = ChromaticAberrationFringeUV(uv, _MainTex_TexelSize);

		float mainBlue = GetChromaticFringeMainTex(uv, 1.0, fromCentre);
		mainBlue = min(mainBlue, 1000.0);
		mainCol.b = max( mainCol.b, mainBlue);

		float mainRed = GetChromaticFringeMainTexR(uv, 4.0, min(d, 1.0) * _ColorDistort.r, fromCentre, mainCol.r);
		mainRed = min(mainRed, 1000.0);
		mainCol.r = mainRed;

		float mainGreen = GetChromaticFringeMainTexG(uv, 8.0, min(d, 1.0) * _ColorDistort.g, fromCentre, mainCol.g);
		mainGreen = min(mainGreen, 1000.0);
		mainCol.g = mainGreen;
		#else
		//Chomab
		float mainBlue = 0.0;
		#endif
#else
		float mainBlue = 0.0;
		mainCol = UNITY_SAMPLE_TEX2D(_MainTex, uv);
#endif

#if BLOOM || BLOOMLENSDIRT || BLOOMSTAB

		float4 bloomCol = (float4)0.0;

		#if !BLOOMSTAB


#if SHADER_API_VULKAN || SHADER_API_GLES3 || SHADER_API_METAL
				half3 bloomOne = UpsampleFilterMobile(_BloomTex, _BloomTex_ST, uv, _BloomTex_TexelSize.xy, 1.0);
				half3 bloomTwo = UpsampleFilterMobile(_BloomTexQuarter, _BloomTexQuarter_ST, uv, _BloomTexQuarter_TexelSize.xy, 1.5);
				half3 bloomThree = UpsampleFilterMobile(_BloomTexEighth, _BloomTexEighth_ST, uv, _BloomTexEighth_TexelSize.xy, 2.0);
				half3 bloomFour = UpsampleFilterMobile(_BloomTexSixteenth, _BloomTexSixteenth_ST, uv, _BloomTexSixteenth_TexelSize.xy, 3.0);
				half3 bloomFive = UpsampleFilterMobile(_BloomTexThirtySecond, _BloomTexSixteenth_ST, uv, _BloomTexThirtySecond_TexelSize.xy, 3.0);
				half3 bloomSix = UpsampleFilterMobile(_BloomTexSixtyFourth, _BloomTexSixteenth_ST, uv, _BloomTexSixtyFourth_TexelSize.xy, 3.0);
#else
				half3 bloomOne = UpsampleFilter(_BloomTex, _BloomTex_ST, uv, _BloomTex_TexelSize.xy, 1.0, sampler_BloomTex);
				half3 bloomTwo = UpsampleFilter(_BloomTexQuarter, _BloomTexQuarter_ST, uv, _BloomTexQuarter_TexelSize.xy, 1.5, sampler_BloomTexQuarter);
				half3 bloomThree = UpsampleFilter(_BloomTexEighth, _BloomTexEighth_ST, uv, _BloomTexEighth_TexelSize.xy, 2.0, sampler_BloomTexEighth);
				half3 bloomFour = UpsampleFilter(_BloomTexSixteenth, _BloomTexSixteenth_ST, uv, _BloomTexSixteenth_TexelSize.xy, 3.0, sampler_BloomTexSixteenth);
				half3 bloomFive = UpsampleFilter(_BloomTexThirtySecond, _BloomTexSixteenth_ST, uv, _BloomTexThirtySecond_TexelSize.xy, 3.0, sampler_BloomTexThirtySecond);
				half3 bloomSix = UpsampleFilter(_BloomTexSixtyFourth, _BloomTexSixteenth_ST, uv, _BloomTexSixtyFourth_TexelSize.xy, 3.0, sampler_BloomTexSixtyFourth);
#endif
				bloomCol.rgb = bloomOne * bloomMults.r;
				bloomCol.rgb += bloomTwo * bloomMultsMid.r;
				bloomCol.rgb += bloomThree * bloomMults.g;
				bloomCol.rgb += bloomFour * bloomMults.b;
				bloomCol.rgb += bloomFive * bloomMults.a;
				bloomCol.rgb += bloomSix * bloomMultsMid.g;
				bloomCol *= _BloomStrength;

				float4 softBloomCol = 1.0 - exp(-bloomCol);
				bloomCol = lerp(bloomCol, softBloomCol, 0.80);
		#endif

		#if BLOOMSTAB
			bloomCol = UNITY_SAMPLE_TEX2D(_BloomTexLast, uv);
		#endif

		#if BLOOMLENSDIRT
			float4 dirtCol = UNITY_SAMPLE_TEX2D(_BloomDirtTex, uv);
			dirtCol.rgb = ApplyColorify(dirtCol.rgb, -1., dirtCol.r);
			float bloomLuma = Luminance(bloomCol.rgb) * _LensDirtExposure;
			dirtCol = lerp(0.0, dirtCol, min(bloomLuma, 1.0));
			mainCol.rgb += dirtCol;
		#endif

		if (_Tonemap > 0.0)
		{
			bloomCol = FastToneMapPRISM(bloomCol);
		}
		mainCol += bloomCol;
#endif
		
		mainCol.rgb = PrismVignette(uv, mainCol.rgb, _VignetteStrength);

		return mainCol;
	}

	float4 fragPRISMBloomCopy(PostProcessVaryings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		
		float4 bloomCol = ZEROES;
		float2 uv = UnityStereoTransformScreenSpaceTex(i.texcoord);

#if SHADER_API_VULKAN || SHADER_API_GLES3 || SHADER_API_METAL
		half3 bloomOne = UpsampleFilterMobile(_BloomTex, _BloomTex_ST, uv, _BloomTex_TexelSize.xy, 1.0);
		half3 bloomTwo = UpsampleFilterMobile(_BloomTexQuarter, _BloomTexQuarter_ST, uv, _BloomTexQuarter_TexelSize.xy, 1.5);
		half3 bloomThree = UpsampleFilterMobile(_BloomTexEighth, _BloomTexEighth_ST, uv, _BloomTexEighth_TexelSize.xy, 2.0);
		half3 bloomFour = UpsampleFilterMobile(_BloomTexSixteenth, _BloomTexSixteenth_ST, uv, _BloomTexSixteenth_TexelSize.xy, 3.0);
		half3 bloomFive = UpsampleFilterMobile(_BloomTexThirtySecond, _BloomTexSixteenth_ST, uv, _BloomTexThirtySecond_TexelSize.xy, 3.0);
		half3 bloomSix = UpsampleFilterMobile(_BloomTexSixtyFourth, _BloomTexSixteenth_ST, uv, _BloomTexSixtyFourth_TexelSize.xy, 3.0);
#else
		half3 bloomOne = UpsampleFilter(_BloomTex, _BloomTex_ST, uv, _BloomTex_TexelSize.xy, 1.0, sampler_BloomTex);
		half3 bloomTwo = UpsampleFilter(_BloomTexQuarter, _BloomTexQuarter_ST, uv, _BloomTexQuarter_TexelSize.xy, 1.5, sampler_BloomTexQuarter);
		half3 bloomThree = UpsampleFilter(_BloomTexEighth, _BloomTexEighth_ST, uv, _BloomTexEighth_TexelSize.xy, 2.0, sampler_BloomTexEighth);
		half3 bloomFour = UpsampleFilter(_BloomTexSixteenth, _BloomTexSixteenth_ST, uv, _BloomTexSixteenth_TexelSize.xy, 3.0, sampler_BloomTexSixteenth);
		half3 bloomFive = UpsampleFilter(_BloomTexThirtySecond, _BloomTexSixteenth_ST, uv, _BloomTexThirtySecond_TexelSize.xy, 3.0, sampler_BloomTexThirtySecond);
		half3 bloomSix = UpsampleFilter(_BloomTexSixtyFourth, _BloomTexSixteenth_ST, uv, _BloomTexSixtyFourth_TexelSize.xy, 3.0, sampler_BloomTexSixtyFourth);
#endif
		bloomCol.rgb = bloomOne * bloomMults.r;
		bloomCol.rgb += bloomTwo * bloomMultsMid.r;
		bloomCol.rgb += bloomThree * bloomMults.g;
		bloomCol.rgb += bloomFour * bloomMults.b;
		bloomCol.rgb += bloomFive * bloomMults.a;
		bloomCol.rgb += bloomSix * bloomMultsMid.g;
		bloomCol *= _BloomStrength;

		float4 softBloomCol = 1.0 - exp(-bloomCol);
		bloomCol = lerp(bloomCol, softBloomCol, 0.80);

		float4 lastBloomCol = UNITY_SAMPLE_TEX2D(_MainTex, uv);
		bloomCol = lerp(lastBloomCol, bloomCol, 0.7);
		
		return bloomCol;
	}

	half4 fragPRISMDownsampleMobile(PostProcessVaryings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		float2 uv = UnityStereoTransformScreenSpaceTex(i.texcoord);

		return fragDownsampleMobile(uv);
		//float4 mainCol = UNITY_SAMPLE_TEX2D(_MainTex, uv);

		//return mainCol;
	}

	half4 fragPRISMDownsample(PostProcessVaryings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		float2 uv = UnityStereoTransformScreenSpaceTex(i.texcoord);

#if BLOOMSTAB
		return fragMedLarge(uv);
#else
		return fragDownsampleGauss(uv);

#endif
		//float4 mainCol = UNITY_SAMPLE_TEX2D(_MainTex, uv);

		//return mainCol;
	}

	half4 fragPRISMStackSensor(PostProcessVaryings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		float2 uv = UnityStereoTransformScreenSpaceTex(i.texcoord);
		//float2 uv = i.texcoord;
		float3 tempCorrectedMainCol;// = float3(0.0, 0.0, 0.0);
		float3 labConvertedMainColF;// = float3(0.0, 0.0, 0.0);
		float4 mainCol = UNITY_SAMPLE_TEX2D(_SecondaryTex, uv);

#if DEVELOP
		mainCol.rgb = exp2(_Exposure)*mainCol.rgb;

		//CC
		labConvertedMainColF = RGBtoLab(mainCol.rgb);

		//labConvertedMainColF.r *= _Lightness;
		labConvertedMainColF.r *= (pow(labConvertedMainColF.r, _Lightness) * _LightnessSlope);

		//COLOR TEMPERATURE
		tempCorrectedMainCol = mainCol.rgb * ColorTemperatureToRGB(_ColorTemperature);

		tempCorrectedMainCol.g *= _ColorTint;
#endif

#if DEVELOP
		tempCorrectedMainCol.rgb = ApplyColorify(tempCorrectedMainCol.rgb, _ColorVibrance, labConvertedMainColF.r);
#endif

#if DEVELOP
		float3 labConvertedBlue = RGBtoLab(tempCorrectedMainCol);

		//Now its ACTUALLY temp corrected
		labConvertedBlue.g *= _a_LabValue;
		labConvertedBlue.b *= _b_LabValue;
#endif

#if DEVELOP
		labConvertedMainColF.r = CurvesPassCIELAB(SafeHDR(labConvertedMainColF.r), _Contrast).r;
#endif
			
#if DEVELOP
		//Correct lighting - now convert back a "new" lab color, with lightness retained from original
		mainCol.rgb = LabtoRGB(float3(labConvertedMainColF.r, labConvertedBlue.g, labConvertedBlue.b));
		//END COLOR TEMPERATURE
#endif

		//// http://http.developer.nvidia.com/GPUGems/gpugems_ch22.html
		//Black point
#if DEVELOP
		mainCol.rgb = SMH(mainCol.rgb, labConvertedMainColF.r,
			_ShadowValue,  _MiddleValue,  _HighlightsValue, _SaturationValues);
#endif

#if LUT
		mainCol = lerp(mainCol, lookupLinear(1, mainCol, uv), _LutAmount);
#endif

#if GRAIN
		mainCol.rgb = ApplyNoise(mainCol.rgb, uv, _GrainScale, _GrainIntensity, _Time.y, _MainTex_TexelSize, _GrainSaturations, _GrainTVValues);
		//mainCol.rgb = ApplyTVNoise(mainCol.rgb, uv, _GrainIntensity, _Time.y, _MainTex_TexelSize, _GrainTVValues);//
		//float noise2 = tvNoise(uv, _Time.x);
		//noise2 = lerp(1.0, noise2, _GrainSaturations.w);
		//mainCol.rgb *= noise2;
#endif
		
#if DEVELOP
		if (_UseDither > 0.0)
		{
			mainCol.rgb = QuasiDither(mainCol.rgb, uv, _DitherBitDepth, _MainTex_TexelSize);
		}

		if (_Tonemap > 0.0)
		{
			mainCol.rgb = ACESFitted(mainCol.rgb);
			mainCol.rgb = filmicTonemapRomB(mainCol.rgb, _Gamma);
			//mainCol.rgb = ACESFitted(mainCol.rgb);
			//mainCol.rgb = filmicTonemapRomBOld(mainCol.rgb, _Gamma);
			//mainCol = FastToneMapPRISM(mainCol);
		}
#endif

#if LUT
		if (_SecondLutAmount > 0.0)
		{
			mainCol = lerp(mainCol, lookupLinear(2, mainCol, uv), _SecondLutAmount);
		}
#endif

		return mainCol;
	}
	ENDHLSL

	//LENS EFFECTS - 0
	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "PRISMStackPreSensor"
			HLSLPROGRAM
			#pragma multi_compile __ BLOOM BLOOMLENSDIRT
			#pragma multi_compile __ CHROMAB FRINGING
			#pragma multi_compile __ BARRELDIST
			#pragma multi_compile __ DEVELOP
			#pragma multi_compile __ LUT
			#pragma multi_compile __ GRAIN
			#pragma multi_compile __ BLOOMSTAB
			#pragma multi_compile __ BLOOMCLARITY
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragPRISMStack
			ENDHLSL
		}

		//SENSOR EFFECTS - 1
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "PRISMSensorEffects"
			HLSLPROGRAM
			#pragma multi_compile __ BLOOM BLOOMLENSDIRT
			#pragma multi_compile __ CHROMAB BARRELDIST
			#pragma multi_compile __ DEVELOP
			#pragma multi_compile __ LUT
			#pragma multi_compile __ GRAIN
			#pragma multi_compile __ BLOOMSTAB
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragPRISMStackSensor
			ENDHLSL
		}


		//Downsample - 2
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "PRISMDownsample"
			HLSLPROGRAM
			#pragma multi_compile __ BLOOM BLOOMLENSDIRT
			#pragma multi_compile __ CHROMAB BARRELDIST
			#pragma multi_compile __ DEVELOP
			#pragma multi_compile __ BLOOMSTAB
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragPRISMDownsample
			ENDHLSL
		}

		//Horiz - 3
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "PRISMBlurHorizontal"
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragBlurHorizontalPRISM
			ENDHLSL
		}

		//Vert - 4
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "PRISMBlurVertical"
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragBlurVerticalPRISM
			ENDHLSL
		}

		//med - 5
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "PRISMBloomMEdian"
			HLSLPROGRAM
			#pragma multi_compile __ BLOOM BLOOMLENSDIRT
			#pragma multi_compile __ CHROMAB BARRELDIST
			#pragma multi_compile __ DEVELOP
			#pragma multi_compile __ BLOOMSTAB
			#pragma multi_compile __ BLOOMCLARITY
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragBloomMedianFull
			ENDHLSL
		}

		//Crazy Blur left - 6
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "PRISMBlurLeft"
			HLSLPROGRAM
			#pragma vertex FsTrianglePostProcessVertexProgramBloom
			#pragma fragment PS
			ENDHLSL
		}

		//Crazy Blur right 7
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "PRISMBlurRight"
			HLSLPROGRAM
			#pragma vertex FsTrianglePostProcessVertexProgramBloomTwo
			#pragma fragment PS
			ENDHLSL
		}

		//Downsample Mobile 8
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "PRISM DSA Mobile"
			HLSLPROGRAM
			#pragma multi_compile __ BLOOM BLOOMLENSDIRT
			#pragma multi_compile __ CHROMAB BARRELDIST
			#pragma multi_compile __ DEVELOP
			#pragma multi_compile __ BLOOMSTAB
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragPRISMDownsampleMobile
			ENDHLSL
		}

		//Crazy Blur left - 9
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "PRISMBlurLeftThree"
			HLSLPROGRAM
			#pragma vertex FsTrianglePostProcessVertexProgramBloomThree
			#pragma fragment PS
			ENDHLSL
		}

		//Crazy Blur right 10
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "PRISMBlurRightFour"
			HLSLPROGRAM
			#pragma vertex FsTrianglePostProcessVertexProgramBloomFour
			#pragma fragment PS
			ENDHLSL
		}

		//Crazy Blur left - 11
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "PRISMBlurLeftThree"
			HLSLPROGRAM
			#pragma vertex FsTrianglePostProcessVertexProgramBloomFive
			#pragma fragment PS
			ENDHLSL
		}

		//Crazy Blur right 12
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "PRISMBlurRightFour"
			HLSLPROGRAM
			#pragma vertex FsTrianglePostProcessVertexProgramBloomSix
			#pragma fragment PS
			ENDHLSL
		}


		//Bloom copy 13
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "PRISMBloomCopy"
			HLSLPROGRAM
			#pragma multi_compile __ BLOOM BLOOMLENSDIRT
			#pragma multi_compile __ CHROMAB BARRELDIST
			#pragma multi_compile __ DEVELOP
			#pragma multi_compile __ BLOOMSTAB
			#pragma multi_compile __ BLOOMCLARITY
			#pragma vertex FsTrianglePostProcessVertexProgramBloomSix
			#pragma fragment fragPRISMBloomCopy
			ENDHLSL
		}

		//Bloom pre mobile 14
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Name "PRISMBloomMobilePre"
			HLSLPROGRAM
			#pragma multi_compile __ BLOOM BLOOMLENSDIRT
			#pragma multi_compile __ CHROMAB BARRELDIST
			#pragma multi_compile __ DEVELOP
			#pragma multi_compile __ BLOOMSTAB
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragBloomPrepassMobile
			ENDHLSL
		}


		
			
	}
}