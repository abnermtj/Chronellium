#include "Assets/PRISMv3URP/Resources/Core.hlsl"
#include "HLSLSupport.cginc"
#include "PRISMCG.hlsl"


#define HALF_MAX 65504.0

#define ZEROES float4(0.0,0.0,0.0,0.0)

//FS Tex
UNITY_DECLARE_TEX2D(_FSOverlayTex);
//TEXTURE2D_SAMPLER2D(_FSOverlayTex, sampler_FSOverlayTex);
float4 _FSOverlayTex_TexelSize;

float3 highlights(float3 col, float thres)
{
	//Gradual
	col *= saturate(col.r / thres);
	return col;
}

float3 highlightsKeijiro(float3 col, float thres)
{
	// Pixel brightness
	half br = LuminanceSimple(col);

	// Under-threshold part: quadratic curve
	half rq = clamp(br - 5., 0.0, 65472.0);
	rq = 0.5 * rq * rq;

	// Combine and apply the brightness response curve.
	col *= max(rq, br - thres) / max(br, 1e-4);

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
	half2 PixelSize = _MainTex_TexelSize.y;

	// -- Gaussian filter --
	//   [ .25, .50, .25]     [ 1 , 2 , 1 ]
	//   [ .50,   1, .50]  =  [ 2 , 4 , 2 ]
	//   [ .25, .50, .25]     [ 1 , 2 , 1 ]
	float px = PixelSize.x;//1.0/
	float py = PixelSize.y;

	float4 blur_ori = UNITY_SAMPLE_TEX2D(_MainTex, uv + float2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * 0.5 ); // South East
	float4 blur_tw = UNITY_SAMPLE_TEX2D(_MainTex, uv + float2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * 0.5 );  // South West
	float4 blur_th = UNITY_SAMPLE_TEX2D(_MainTex, uv + float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y) * 0.5 ); // North East
	float4 blur_fr = UNITY_SAMPLE_TEX2D(_MainTex, uv + float2(-_MainTex_TexelSize.x, _MainTex_TexelSize.y) * 0.5); // North West

	blur_ori = blur_ori + blur_tw + blur_th + blur_fr;

	return blur_ori * 0.25;
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

	return float4(SafeHDR(v[2].rgb), midCol.a);
}

float4 fragMedLarge(float2 uv, float _BloomThreshold)
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

	return float4(SafeHDR(v[4].rgb), midCol.a);
}

float4 fragBloomMedFast(float2 uv, float4 _MainTex_TexelSize, float _BloomThreshold)//, float _Exposure)
{
	float2 ooRes = _MainTex_TexelSize.xy;//_ScreenParams.w;

	float4 midCol = UNITY_SAMPLE_TEX2D(_MainTex, uv);
	//midCol.rgb = exp2(_Exposure)*midCol.rgb;

#if GRADUALH
	midCol.rgb = highlights(midCol.rgb, _BloomThreshold);//highlights(v[2].rgb, _BloomThreshold);// saturate(SCurve(Luminance(v[4].rgb)) / _BloomThreshold);

#else
	midCol.rgb = highlightsKeijiro(midCol.rgb, _BloomThreshold);//highlights(v[2].rgb, _BloomThreshold);// saturate(SCurve(Luminance(v[4].rgb)) / _BloomThreshold);
#endif

	//return float4(v[2].rgb, midCol.a);
	return float4(SafeHDR3(midCol.rgb), midCol.a);
}

float4 fragBloomMedRGB(float2 uv, float4 _MainTex_TexelSize, float _BloomThreshold)//, float _Exposure)
{
	float2 ooRes = _MainTex_TexelSize.xy;//_ScreenParams.w;

	float3 ofs = _MainTex_TexelSize.xyx * float3(1, 1, 0);
	//
	float3 v[5];
	
	float4 midCol = UNITY_SAMPLE_TEX2D(_MainTex, uv);

	v[0] = midCol.rgb;
	v[1] = UNITY_SAMPLE_TEX2D(_MainTex, uv - ofs.xz).rgb;
	v[2] = UNITY_SAMPLE_TEX2D(_MainTex, uv - ofs.xz).rgb;
	v[3] = UNITY_SAMPLE_TEX2D(_MainTex, uv - ofs.zy).rgb;
	v[4] = UNITY_SAMPLE_TEX2D(_MainTex, uv + ofs.zy).rgb;

	float3 temp;
	mnmx5(v[0], v[1], v[2], v[3], v[4]);
	mnmx3(v[1], v[2], v[3]);

	//v[2].rgb = exp2(_Exposure)*v[2].rgb;

#if GRADUALH
	v[2].rgb = highlights(v[2].rgb, _BloomThreshold);//highlights(v[2].rgb, _BloomThreshold);// saturate(SCurve(Luminance(v[4].rgb)) / _BloomThreshold);
#else
	v[2].rgb = highlightsKeijiro(v[2].rgb, _BloomThreshold);//highlights(v[2].rgb, _BloomThreshold);// saturate(SCurve(Luminance(v[4].rgb)) / _BloomThreshold);
#endif
	//return float4(v[2].rgb, midCol.a);
	return float4(SafeHDR3(v[2].rgb), midCol.a);
}

static const half coefficients[5] = { 0.0625, 0.25, 0.375, 0.25, 0.0625 };
uniform float2 offsets[5];

//https://d3cw3dd2w32x2b.cloudfront.net/wp-content/uploads/2012/06/faster_filters.pdf
float4 fragBlurHorizontal(float2 uv, float4 _MainTex_TexelSize)
{
	float2 ps = _MainTex_TexelSize.xy;
	float4 c = ZEROES;

	/*4.30908 * 055028
	-2.37532 * 0.244038
	-0.50000 * 401870
	1.37532 * 244038
	3.30908 * 055028*/

	//float4 fastOffsets = float4(-3.5, -1.5, 1.5, 3.5) * ps.xxxx;
	//Faster - can just add - infront to make -3.5 and 1.5
	float offsOne = 1.5 * ps.x;
	float offsTwo = 3.5 * ps.x;
	float4 c1 = UNITY_SAMPLE_TEX2D(_MainTex, uv - float2(offsTwo, 0.0));
	float4 c2 = UNITY_SAMPLE_TEX2D(_MainTex, uv - float2(offsOne, 0.0));
	float4 c3 = UNITY_SAMPLE_TEX2D(_MainTex, uv);
	float4 c4 = UNITY_SAMPLE_TEX2D(_MainTex, uv + float2(offsOne, 0.0));
	float4 c5 = UNITY_SAMPLE_TEX2D(_MainTex, uv + float2(offsTwo, 0.0));

	c1 *= coefficients[0];
	c2 *= coefficients[1];
	c3 *= coefficients[2];
	c4 *= coefficients[3];
	c5 *= coefficients[4];

	return c1 + c2 + c3 + c4 + c5;
}

float4 fragBlurVertical(float2 uv, float4 _MainTex_TexelSize)
{
	float2 ps = _MainTex_TexelSize.xy;
	float4 c = ZEROES;

	/*4.30908 * 055028
	-2.37532 * 0.244038
	-0.50000 * 401870
	1.37532 * 244038
	3.30908 * 055028*/

	//float4 fastOffsets = float4(-3.5, -1.5, 1.5, 3.5) * ps.xxxx;
	//Faster - can just add - infront to make -3.5 and 1.5
	float offsOne = 1.5 * ps.x;
	float offsTwo = 3.5 * ps.x;
	float4 c1 = UNITY_SAMPLE_TEX2D(_MainTex, float2(0.5, -4.30908)*ps + uv);
	float4 c2 = UNITY_SAMPLE_TEX2D(_MainTex, float2(0.5, -2.37532)*ps + uv);
	float4 c3 = UNITY_SAMPLE_TEX2D(_MainTex, float2(0.5, -0.5)*ps + uv);
	float4 c4 = UNITY_SAMPLE_TEX2D(_MainTex, float2(0.5, 1.37532)*ps + uv);
	float4 c5 = UNITY_SAMPLE_TEX2D(_MainTex, float2(0.5, 3.30908)*ps + uv);

	c1 *= 0.055028;
	c2 *= 0.244038;
	c3 *= 0.401870;
	c4 *= 0.244038;
	c5 *= 0.055028;

	return c1 + c2 + c3 + c4 + c5;
}
