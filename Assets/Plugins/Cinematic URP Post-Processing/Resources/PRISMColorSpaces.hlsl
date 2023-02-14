// -- Color space options --
#define GammaCurve Fast
#define gamma 2.2
#pragma warning (disable : 3206)

// -- Misc --
sampler s0 : register(s0);
float4 p0 :  register(c0);
float2 p1 :  register(c1);

#define width  (p0[0])
#define height (p0[1])

#define px (p1[0])
#define py (p1[1])

// -- Option values --
#define None  1
#define sRGB  2
#define Power 3
#define Fast  4
#define true  5
#define false 6

// -- Gamma processing --
#define A (0.272433)

#if GammaCurve == sRGB
float3 Gamma(float3 x) { return x < (0.0392857 / 12.9232102) ? x * 12.9232102 : 1.055*pow(x, 1 / gamma) - 0.055; }
float3 GammaInv(float3 x) { return x < 0.0392857 ? x / 12.9232102 : pow((x + 0.055) / 1.055, gamma); }
#elif GammaCurve == Power
float3 Gamma(float3 x) { return pow(saturate(x), 1 / gamma); }
float3 GammaInv(float3 x) { return pow(saturate(x), gamma); }
#elif GammaCurve == Fast
float3 Gamma(float3 x) { return saturate(x)*rsqrt(saturate(x)); }
float3 GammaInv(float3 x) { return x * x; }
#elif GammaCurve == None
float3 Gamma(float3 x) { return x; }
float3 GammaInv(float3 x) { return x; }
#endif

#define HALF_MAX 65504.0

inline half3 SafeHDRTwo(half3 c) { return min(c, HALF_MAX); }

// -- Colour space Processing --
#define Kb 0.114
#define Kr 0.299
#define RGBtoYUV float3x3(float3(Kr, 1 - Kr - Kb, Kb), float3(-Kr, Kr + Kb - 1, 1 - Kb) / (2*(1 - Kb)), float3(1 - Kr, Kr + Kb - 1, -Kb) / (2*(1 - Kr)))
#define YUVtoRGB float3x3(float3(1, 0, 2*(1 - Kr)), float3(Kb + Kr - 1, 2*(1 - Kb)*Kb, 2*Kr*(1 - Kr)) / (Kb + Kr - 1), float3(1, 2*(1 - Kb),0))
#define RGBtoXYZ float3x3(float3(0.4124,0.3576,0.1805),float3(0.2126,0.7152,0.0722),float3(0.0193,0.1192,0.9502))
#define XYZtoRGB (625.0*float3x3(float3(67097680, -31827592, -10327488), float3(-20061906, 38837883, 859902), float3(1153856, -4225640, 21892272))/12940760409.0)
#define YUVtoXYZ mul(RGBtoXYZ,YUVtoRGB)
#define XYZtoYUV mul(RGBtoYUV,XYZtoRGB)

float3 Labf(float3 x) { return x < (6.0*6.0*6.0) / (29.0*29.0*29.0) ? (x * (29.0 * 29.0) / (3.0 * 6.0 * 6.0)) + (4.0 / 29.0) : pow(abs(x), 1.0 / 3.0); }
float3 Labfinv(float3 x) { return x < (6.0 / 29.0) ? (x - (4.0 / 29.0)) * (3.0 * 6.0 * 6.0) / (29.0 * 29.0) : x * x*x; }

float3 DLabf(float3 x) { return min((29.0 * 29.0) / (3.0 * 6.0 * 6.0), (1.0 / 3.0) / pow(x, (2.0 / 3.0))); }
float3 DLabfinv(float3 x) { return max((3.0 * 6.0 * 6.0) / (29.0 * 29.0), 3.0*x*x); }
float3 RGBtoLab(float3 rgb) {
	float3 xyz = mul(RGBtoXYZ, rgb);
	xyz = Labf(xyz);
	return float3(1.16*xyz.y - 0.16, 5.0*(xyz.x - xyz.y), 2.0*(xyz.y - xyz.z));
}

float3 LabtoRGB(float3 lab) {
	float3 xyz = (lab.x + 0.16) / 1.16 + float3(lab.y / 5.0, 0, -lab.z / 2.0);
	return saturate(mul(XYZtoRGB, Labfinv(xyz)));
}

float3 LabtoRGBHDR(float3 lab) {
	float3 xyz = (lab.x + 0.16) / 1.16 + float3(lab.y / 5.0, 0, -lab.z / 2.0);
	return SafeHDRTwo(mul(XYZtoRGB, Labfinv(xyz)));
}

float3x3 DRGBtoLab(float3 rgb) {
	float3 xyz = mul(RGBtoXYZ, rgb);
	xyz = DLabf(xyz);
	float3x3 D = { { xyz.x, 0, 0 }, { 0, xyz.y, 0 }, { 0, 0, xyz.z } };
	return mul(D, RGBtoXYZ);
}

float3x3 DLabtoRGB(float3 lab) {
	float3 xyz = (lab.x + 0.16) / 1.16 + float3(lab.y / 5.0, 0, -lab.z / 2.0);
	xyz = DLabfinv(xyz);
	float3x3 D = { { xyz.x, 0, 0 }, { 0, xyz.y, 0 }, { 0, 0, xyz.z } };
	return mul(XYZtoRGB, D);
}

float PRISMLuma(float3 rgb) {
	return dot(RGBtoYUV[0], rgb);
}

//PRISM.COLOR
//Inspired by typical highlight & shadow algorithms, https://mouaif.wordpress.com/2009/01/28/levels-control-shader/
//Col.a = lightness
//float3 col, float2 shadowsPaddingCutoff, float2 highlightsPaddingCutoff, float luma, float3 shadows, float4 midtones, float3 highlights, float3 saturations)
//float3 HighlightsShadows(float3 col, float4 grayVal, float4 shadowVal, float4 midVal, float4 highVal, float4 gainVal)
float3 SMH(float3 col, float luma, float4 shadows, float4 midtones, float4 highlights, float3 saturations)
{

	float luminance = luma;// dot(col, float3(0.2126, 0.7152, 0.0722));

	//Histogram split
	float4 FShadows = (0.5 + (cos(3.14159 * min(luminance, 0.5) * 2.0)) * 0.5) * shadows;
	float4 FMid = (0.5 - (cos(3.14159 * luminance * 2.0)) * 0.5) * midtones;
	float4 FHigh = (0.5 + (cos(3.14159 * max(luminance, 0.5) * 2.0)) * 0.5) * highlights;

	//Color Correct
	col = lerp(col, (float3)(luminance), (saturations)); //Desaturation
	col = col + (FShadows * FShadows.a) + (FMid * FMid.a) + (FHigh * FHigh.a); //Color Balance
	col = col * ((float3)(1.0) + FShadows.rgb) * ((float3)(1.0) + FMid.rgb) * ((float3)(1.0) + FHigh.rgb); //Color lift
	return col; //ColorGain todo exposure
}

// TANH Function (Hyperbolic Tangent)
inline float glslTanh(float val)
{
	float tmp = exp(val);
	float tanH = (tmp - 1.0 / tmp) / (tmp + 1.0 / tmp);
	return tanH;
}

inline float3 glslTanh(float3 val)
{
	float3 tmp = exp(val);
	float3 tanH = (tmp - 1.0 / tmp) / (tmp + 1.0 / tmp);
	return tanH;
}

// Copyright (c) 2016, bacondither
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer
//    in this position and unchanged.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE AUTHORS ``AS IS'' AND ANY EXPRESS OR
// IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
// OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
// IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
// NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// EXPECTS FULL RANGE GAMMA LIGHT
inline float3 ApplyColorify(float3 col, float _Colourfulness, float luma)
{
	//col = saturate(col);
	//float luma = dot(col, lumacoeff);//Luminance(col);//to_gamma( max(dot(to_linear(col), lumacoeff), 0) );

	float3 colour = luma + (col - luma)*(max(_Colourfulness, -1) + 1);

	float3 diff = colour - col;

	//#if defined( HD_COLORIFY )
	#if SHADER_API_GLCORE || SHADER_API_OPENGL || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_GLES2 || SHADER_API_WIIU
		diff = glslTanh(diff);
	#else
	//Or not, since that's slow
		diff = tanh(diff);
	#endif

	return float3(col + diff);
}

float ApplyLiftInvGammaGain(const float lift, const float invGamma, const float gain, float v)
{
	// lerp gain
	float lerpV = saturate(pow(v, invGamma));
	float dst = gain * lerpV + lift * (1.0 - lerpV);
	return dst;
}

//=================================================================================================
// EXPOSURE CODE
//  Baking Lab
//  by MJP and David Neubelt
//  http://mynameismjp.wordpress.com/
//
//  All code licensed under the MIT license
//
//=================================================================================================

// The two functions below were based on code and explanations provided by Padraic Hennessy (@PadraicHennessy).
// See this for more info: https://placeholderart.wordpress.com/2014/11/21/implementing-a-physically-based-camera-manual-exposure/

uniform float ApertureFNumber; uniform float ISO; uniform float ShutterSpeedValue; uniform float maxLuminance;

float SaturationBasedExposure()
{
	float maxLuminance = (7800.0f / 65.0f) * (ApertureFNumber * ApertureFNumber) / (ISO * ShutterSpeedValue);
	return log2(1.0f / maxLuminance);
}

float StandardOutputBasedExposure(float middleGrey = 0.18f)
{
	float lAvg = (1000.0f / 65.0f) * (ApertureFNumber * ApertureFNumber) / (ISO * ShutterSpeedValue);
	return log2(middleGrey / lAvg);
}

float Log2Exposure(in float avgLuminance, float ManualExposure)
{
	float exposure = 0.0f;

	/*if (ExposureMode == ExposureModes_Automatic)
	{
		avgLuminance = max(avgLuminance, 0.00001f);
		float linearExposure = (KeyValue / avgLuminance);
		exposure = log2(max(linearExposure, 0.00001f));
	}
	else if (ExposureMode == ExposureModes_Manual_SBS)
	{
		exposure = SaturationBasedExposure();
		exposure -= log2(FP16Scale);
	}
	else if (ExposureMode == ExposureModes_Manual_SOS)
	{
		exposure = StandardOutputBasedExposure();
		exposure -= log2(FP16Scale);
	}
	else
	{*/
		exposure = ManualExposure;
	//}

	return exposure;
}

float LinearExposure(in float avgLuminance, float ManualExposure)
{
	return exp2(Log2Exposure(avgLuminance, ManualExposure));
}

// Determines the color based on exposure settings
float3 CalcExposedColor(in float3 color, in float avgLuminance, in float offset, in float ManualExposure, out float exposure)
{
	exposure = Log2Exposure(avgLuminance, ManualExposure);
	exposure += offset;
	return exp2(exposure) * color;
}