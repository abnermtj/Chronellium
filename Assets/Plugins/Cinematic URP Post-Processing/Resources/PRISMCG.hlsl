// Disable warnings we aren't interested in
#pragma warning (disable : 3205) // conversion of larger type to smaller
#pragma warning (disable : 3568) // unknown pragma ignored
#pragma warning (disable : 3571) // "pow(f,e) will not work for negative f"; however in majority of our calls to pow we know f is not negative
#pragma warning (disable : 3206) // implicit truncation of vector type

// Encoding/decoding [0..1) floats into 8 bit/channel RG. Note that 1.0 will not be encoded properly.
#define UNITY_PI 3.14159265359f
#define prismEpsilon = 1e-4;

	// Mean of Rec. 709 & 601 luma coefficients
#define lumacoeff        float3(0.2558, 0.6511, 0.0931)

// Max RGB components
//#define max3(RGB)      ( max((RGB).r, max((RGB).g, (RGB).b)) )
//#define min3(RGB)      ( min((RGB).r, min((RGB).g, (RGB).b)) )

// Macros to declare textures and samplers, possibly separately. For platforms
// that have separate samplers & textures (like DX11), and we'd want to conserve
// the samplers.
//  - UNITY_DECLARE_TEX*_NOSAMPLER declares a texture, without a sampler.
//  - UNITY_SAMPLE_TEX*_SAMPLER samples a texture, using sampler from another texture.
//      That another texture must also be actually used in the current shader, otherwise
//      the correct sampler will not be set.
#if defined(SHADER_API_D3D11) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))
#define UNITY_SEPARATE_TEXTURE_SAMPLER

// 2D textures
#define UNITY_DECLARE_TEX2D(tex) Texture2D tex; SamplerState sampler##tex
#define UNITY_DECLARE_TEX2D_NOSAMPLER(tex) Texture2D tex
#define UNITY_DECLARE_TEX2D_NOSAMPLER_INT(tex) Texture2D<int4> tex
#define UNITY_DECLARE_TEX2D_NOSAMPLER_UINT(tex) Texture2D<uint4> tex
#define UNITY_SAMPLE_TEX2D(tex,coord) tex.Sample (sampler##tex,coord)
#define UNITY_SAMPLE_TEX2D_LOD(tex,coord,lod) tex.SampleLevel (sampler##tex,coord, lod)
#define UNITY_SAMPLE_TEX2D_SAMPLER(tex,samplertex,coord) tex.Sample (sampler##samplertex,coord)
#define UNITY_SAMPLE_TEX2D_SAMPLER_LOD(tex, samplertex, coord, lod) tex.SampleLevel (sampler##samplertex, coord, lod)

#if defined(UNITY_COMPILER_HLSLCC) && (!defined(SHADER_API_GLCORE) || defined(SHADER_API_SWITCH)) // GL Core doesn't have the _half mangling, the rest of them do. Workaround for Nintendo Switch.
#define UNITY_DECLARE_TEX2D_HALF(tex) Texture2D_half tex; SamplerState sampler##tex
#define UNITY_DECLARE_TEX2D_FLOAT(tex) Texture2D_float tex; SamplerState sampler##tex
#define UNITY_DECLARE_TEX2D_NOSAMPLER_HALF(tex) Texture2D_half tex
#define UNITY_DECLARE_TEX2D_NOSAMPLER_FLOAT(tex) Texture2D_float tex
#else
#define UNITY_DECLARE_TEX2D_HALF(tex) Texture2D tex; SamplerState sampler##tex
#define UNITY_DECLARE_TEX2D_FLOAT(tex) Texture2D tex; SamplerState sampler##tex
#define UNITY_DECLARE_TEX2D_NOSAMPLER_HALF(tex) Texture2D tex
#define UNITY_DECLARE_TEX2D_NOSAMPLER_FLOAT(tex) Texture2D tex
#endif

	// Cubemaps
#define UNITY_DECLARE_TEXCUBE(tex) TextureCube tex; SamplerState sampler##tex
#define UNITY_ARGS_TEXCUBE(tex) TextureCube tex, SamplerState sampler##tex
#define UNITY_PASS_TEXCUBE(tex) tex, sampler##tex
#define UNITY_PASS_TEXCUBE_SAMPLER(tex,samplertex) tex, sampler##samplertex
#define UNITY_PASS_TEXCUBE_SAMPLER_LOD(tex, samplertex, lod) tex, sampler##samplertex, lod
#define UNITY_DECLARE_TEXCUBE_NOSAMPLER(tex) TextureCube tex
#define UNITY_SAMPLE_TEXCUBE(tex,coord) tex.Sample (sampler##tex,coord)
#define UNITY_SAMPLE_TEXCUBE_LOD(tex,coord,lod) tex.SampleLevel (sampler##tex,coord, lod)
#define UNITY_SAMPLE_TEXCUBE_SAMPLER(tex,samplertex,coord) tex.Sample (sampler##samplertex,coord)
#define UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(tex, samplertex, coord, lod) tex.SampleLevel (sampler##samplertex, coord, lod)
// 3D textures
#define UNITY_DECLARE_TEX3D(tex) Texture3D tex; SamplerState sampler##tex
#define UNITY_DECLARE_TEX3D_NOSAMPLER(tex) Texture3D tex
#define UNITY_SAMPLE_TEX3D(tex,coord) tex.Sample (sampler##tex,coord)
#define UNITY_SAMPLE_TEX3D_LOD(tex,coord,lod) tex.SampleLevel (sampler##tex,coord, lod)
#define UNITY_SAMPLE_TEX3D_SAMPLER(tex,samplertex,coord) tex.Sample (sampler##samplertex,coord)
#define UNITY_SAMPLE_TEX3D_SAMPLER_LOD(tex, samplertex, coord, lod) tex.SampleLevel(sampler##samplertex, coord, lod)

#if defined(UNITY_COMPILER_HLSLCC) && !defined(SHADER_API_GLCORE) // GL Core doesn't have the _half mangling, the rest of them do.
#define UNITY_DECLARE_TEX3D_FLOAT(tex) Texture3D_float tex; SamplerState sampler##tex
#define UNITY_DECLARE_TEX3D_HALF(tex) Texture3D_half tex; SamplerState sampler##tex
#else
#define UNITY_DECLARE_TEX3D_FLOAT(tex) Texture3D tex; SamplerState sampler##tex
#define UNITY_DECLARE_TEX3D_HALF(tex) Texture3D tex; SamplerState sampler##tex
#endif

	// 2D arrays
#define UNITY_DECLARE_TEX2DARRAY_MS(tex) Texture2DMSArray<float> tex; SamplerState sampler##tex
#define UNITY_DECLARE_TEX2DARRAY_MS_NOSAMPLER(tex) Texture2DArray<float> tex
#define UNITY_DECLARE_TEX2DARRAY(tex) Texture2DArray tex; SamplerState sampler##tex
#define UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(tex) Texture2DArray tex
#define UNITY_ARGS_TEX2DARRAY(tex) Texture2DArray tex, SamplerState sampler##tex
#define UNITY_PASS_TEX2DARRAY(tex) tex, sampler##tex
#define UNITY_SAMPLE_TEX2DARRAY(tex,coord) tex.Sample (sampler##tex,coord)
#define UNITY_SAMPLE_TEX2DARRAY_LOD(tex,coord,lod) tex.SampleLevel (sampler##tex,coord, lod)
#define UNITY_SAMPLE_TEX2DARRAY_SAMPLER(tex,samplertex,coord) tex.Sample (sampler##samplertex,coord)
#define UNITY_SAMPLE_TEX2DARRAY_SAMPLER_LOD(tex,samplertex,coord,lod) tex.SampleLevel (sampler##samplertex,coord,lod)

// Cube arrays
#define UNITY_DECLARE_TEXCUBEARRAY(tex) TextureCubeArray tex; SamplerState sampler##tex
#define UNITY_DECLARE_TEXCUBEARRAY_NOSAMPLER(tex) TextureCubeArray tex
#define UNITY_ARGS_TEXCUBEARRAY(tex) TextureCubeArray tex, SamplerState sampler##tex
#define UNITY_PASS_TEXCUBEARRAY(tex) tex, sampler##tex
#if defined(SHADER_API_PSSL)
	// round the layer index to get DX11-like behaviour (otherwise fractional indices result in mixed up cubemap faces)
#define UNITY_SAMPLE_TEXCUBEARRAY(tex,coord) tex.Sample (sampler##tex,float4((coord).xyz, round((coord).w)))
#define UNITY_SAMPLE_TEXCUBEARRAY_LOD(tex,coord,lod) tex.SampleLevel (sampler##tex,float4((coord).xyz, round((coord).w)), lod)
#define UNITY_SAMPLE_TEXCUBEARRAY_SAMPLER(tex,samplertex,coord) tex.Sample (sampler##samplertex,float4((coord).xyz, round((coord).w)))
#define UNITY_SAMPLE_TEXCUBEARRAY_SAMPLER_LOD(tex,samplertex,coord,lod) tex.SampleLevel (sampler##samplertex,float4((coord).xyz, round((coord).w)), lod)
#else
#define UNITY_SAMPLE_TEXCUBEARRAY(tex,coord) tex.Sample (sampler##tex,coord)
#define UNITY_SAMPLE_TEXCUBEARRAY_LOD(tex,coord,lod) tex.SampleLevel (sampler##tex,coord, lod)
#define UNITY_SAMPLE_TEXCUBEARRAY_SAMPLER(tex,samplertex,coord) tex.Sample (sampler##samplertex,coord)
#define UNITY_SAMPLE_TEXCUBEARRAY_SAMPLER_LOD(tex,samplertex,coord,lod) tex.SampleLevel (sampler##samplertex,coord,lod)
#endif


#else
	// DX9 style HLSL syntax; same object for texture+sampler
	// 2D textures
#define UNITY_DECLARE_TEX2D(tex) sampler2D tex
#define UNITY_DECLARE_TEX2D_HALF(tex) sampler2D_half tex
#define UNITY_DECLARE_TEX2D_FLOAT(tex) sampler2D_float tex

#define UNITY_DECLARE_TEX2D_NOSAMPLER(tex) sampler2D tex
#define UNITY_DECLARE_TEX2D_NOSAMPLER_HALF(tex) sampler2D_half tex
#define UNITY_DECLARE_TEX2D_NOSAMPLER_FLOAT(tex) sampler2D_float tex

#define UNITY_SAMPLE_TEX2D(tex,coord) tex2D (tex,coord)
#define UNITY_SAMPLE_TEX2D_SAMPLER(tex,samplertex,coord) tex2D (tex,coord)
// Cubemaps
#define UNITY_DECLARE_TEXCUBE(tex) samplerCUBE tex
#define UNITY_ARGS_TEXCUBE(tex) samplerCUBE tex
#define UNITY_PASS_TEXCUBE(tex) tex
#define UNITY_PASS_TEXCUBE_SAMPLER(tex,samplertex) tex
#define UNITY_DECLARE_TEXCUBE_NOSAMPLER(tex) samplerCUBE tex
#define UNITY_SAMPLE_TEXCUBE(tex,coord) texCUBE (tex,coord)

#define UNITY_SAMPLE_TEXCUBE_LOD(tex,coord,lod) texCUBElod (tex, half4(coord, lod))
#define UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(tex,samplertex,coord,lod) UNITY_SAMPLE_TEXCUBE_LOD(tex,coord,lod)
#define UNITY_SAMPLE_TEXCUBE_SAMPLER(tex,samplertex,coord) texCUBE (tex,coord)

// 3D textures
#define UNITY_DECLARE_TEX3D(tex) sampler3D tex
#define UNITY_DECLARE_TEX3D_NOSAMPLER(tex) sampler3D tex
#define UNITY_DECLARE_TEX3D_FLOAT(tex) sampler3D_float tex
#define UNITY_DECLARE_TEX3D_HALF(tex) sampler3D_float tex
#define UNITY_SAMPLE_TEX3D(tex,coord) tex3D (tex,coord)
#define UNITY_SAMPLE_TEX3D_LOD(tex,coord,lod) tex3D (tex,float4(coord,lod))
#define UNITY_SAMPLE_TEX3D_SAMPLER(tex,samplertex,coord) tex3D (tex,coord)
#define UNITY_SAMPLE_TEX3D_SAMPLER_LOD(tex,samplertex,coord,lod) tex3D (tex,float4(coord,lod))

// 2D array syntax for surface shader analysis
#if defined(SHADER_TARGET_SURFACE_ANALYSIS)
#define UNITY_DECLARE_TEX2DARRAY(tex) sampler2DArray tex
#define UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(tex) sampler2DArray tex
#define UNITY_ARGS_TEX2DARRAY(tex) sampler2DArray tex
#define UNITY_PASS_TEX2DARRAY(tex) tex
#define UNITY_SAMPLE_TEX2DARRAY(tex,coord) tex2DArray (tex,coord)
#define UNITY_SAMPLE_TEX2DARRAY_LOD(tex,coord,lod) tex2DArraylod (tex, float4(coord,lod))
#define UNITY_SAMPLE_TEX2DARRAY_SAMPLER(tex,samplertex,coord) tex2DArray (tex,coord)
#define UNITY_SAMPLE_TEX2DARRAY_SAMPLER_LOD(tex,samplertex,coord,lod) tex2DArraylod (tex, float4(coord,lod))
#endif

// surface sh ader analysis; just pretend that 2D arrays are cubemaps
#if defined(SHADER_TARGET_SURFACE_ANALYSIS)
#define sampler2DArray samplerCUBE
#define tex2DArray texCUBE
#define tex2DArraylod texCUBElod
#endif

#endif

#define MOD3 float3(443.8975, 397.2973, 491.1871)
#define s2(a, b)				temp = a; a = min(a, b); b = max(temp, b);
#define mn3(a, b, c)			s2(a, b); s2(a, c);
#define mx3(a, b, c)			s2(b, c); s2(a, c);

#define mnmx3(a, b, c)				mx3(a, b, c); s2(a, b);                                   // 3 exchanges
#define mnmx4(a, b, c, d)			s2(a, b); s2(c, d); s2(a, c); s2(b, d);                   // 4 exchanges
#define mnmx5(a, b, c, d, e)		s2(a, b); s2(c, d); mn3(a, c, e); mx3(b, d, e);           // 6 exchanges
#define mnmx6(a, b, c, d, e, f) 	s2(a, d); s2(b, e); s2(c, f); mn3(a, b, c); mx3(d, e, f); // 7 exchanges

inline half  SafeHDR(half  c) { return min(c, 65504.0); }
inline half2 SafeHDR2(half2 c) { return min(c, 65504.0); }
inline half3 SafeHDR3(half3 c) { return min(c, 65504.0); }
inline half4 SafeHDR4(half4 c) { return min(c, 65504.0); }



// https://www.shadertoy.com/view/MtjBWz - thanks iq
float2 rndC(float2 uv) // good function
{
	uv = uv * _ScreenParams.xy + 0.5;
	float2 iuv = floor(uv);
	float2 fuv = frac(uv);

	// * insert your interpolation primitive operation here *
	uv = iuv + fuv * fuv*(3.0 - 2.0*fuv); // smoothstep
	//uv = iuv + fuv*fuv*fuv*(fuv*(fuv*6.0f-15.0f)+10.0f); // quintic

	return(uv - 0.5);  // returns in same unit as input, voxels
}

/*
float3 fetch_iq(TEXTURE2D_ARGS(tex, samplerTex), float2 uv, float2 ts) {
	return UNITY_SAMPLE_TEX2D(tex, rndC(uv) / _ScreenParams.xy).rgb;
	//return UNITY_SAMPLE_TEX2D(tex, rndC(uv) / ts).rgb;
	// needs hw interpolators (good gravy)
}*/


inline float LuminanceSimple(float3 col)
{
	return dot(col, lumacoeff);
}


float smootherstep(float edge0, float edge1, float x)
{
	x = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
	return x * x*x*(x*(x*6.0 - 15.0) + 10.0);
}


half3 TransformColor(half3 skyboxValue, float Threshold) {
	return dot(max(skyboxValue.rgb - Threshold.rrr, half3(0, 0, 0)), half3(1, 1, 1)); // threshold and convert to greyscale
}

half3 ThresholdSmoothHalf(half4 col, float Threshold)
{
	//const float BEGIN_SPILL = 0.8;
	float BEGIN_SPILL = Threshold;
	const float END_SPILL = 2222.0;
	const float MAX_SPILL = 0.9; //note: <=1

	half3 mc = (float3)max(col.r, max(col.g, col.b));
	float t = MAX_SPILL * smootherstep(0.0, END_SPILL - BEGIN_SPILL, mc - BEGIN_SPILL);
	return lerp(col.rgb, mc, t);
}

float3 ThresholdSmooth(float3 col, float Threshold)
{
	//const float BEGIN_SPILL = 0.8;
	float BEGIN_SPILL = Threshold;
	const float END_SPILL = 2222.0;
	const float MAX_SPILL = 0.9; //note: <=1

	float3 mc = (float3)max(col.r, max(col.g, col.b));
	float t = MAX_SPILL * smootherstep(0.0, END_SPILL - BEGIN_SPILL, mc - BEGIN_SPILL);
	return lerp(col, mc, t);
}

inline float3 ThresholdColor(float3 col, float Threshold)
{
	float val = (col.x + col.y + col.z) / 3.0;
	return col * smootherstep(Threshold - 0.1, Threshold + 0.1, val);
}

float3 ThresholdGradual(float3 col, float Threshold)
{
	col *= saturate(LuminanceSimple(col) / Threshold);
	return col;
}

inline float2 EncodeFloatRG(float v)
{
	float2 kEncodeMul = float2(1.0, 255.0);
	float kEncodeBit = 1.0 / 255.0;
	float2 enc = kEncodeMul * v;
	enc = frac(enc);
	enc.x -= enc.y * kEncodeBit;
	return enc;
}
inline float DecodeFloatRG(float2 enc)
{
	float2 kDecodeDot = float2(1.0, 1 / 255.0);
	return dot(enc, kDecodeDot);
}

struct PRISMAttributesDefault
{
	float3 vertex : POSITION;
};

//PRISM Vertex shaders
struct PRISMVaryingsDefault
{
	float4 vertex : SV_POSITION;
	float2 texcoord : TEXCOORD0;
};


// fixed for Single Pass stereo rendering method
half3 UpsampleFilter(Texture2D tex, half4 tex_ST, float2 uv, float2 texelSize, float sampleScale, SamplerState texSampler)
{
	// 9-tap bilinear upsampler (tent filter)
	float4 d = texelSize.xyxy * float4(1.0, 1.0, -1.0, 0.0) * sampleScale;
	half3 s = (half3)0.0;

	//#if SHADER_TARGET > 30
	s = tex.Sample(texSampler, (uv - d.xy));
	s += tex.Sample(texSampler, (uv - d.wy)) * 2.0;
	s += tex.Sample(texSampler, (uv - d.zy));

	s += tex.Sample(texSampler, (uv + d.zw)) * 2.0;
	s += tex.Sample(texSampler, (uv))        * 4.0;
	s += tex.Sample(texSampler, (uv + d.xw)) * 2.0;

	s += tex.Sample(texSampler, (uv + d.zy));
	s += tex.Sample(texSampler, (uv + d.wy)) * 2.0;
	s += tex.Sample(texSampler, (uv + d.xy));
	//#endif


	return s * (1.0 / 16.0);
}

// fixed for Single Pass stereo rendering method
half3 UpsampleFilterMobile(sampler2D tex, half4 tex_ST, float2 uv, float2 texelSize, float sampleScale)
{
	// 9-tap bilinear upsampler (tent filter)
	float4 d = texelSize.xyxy * float4(1.0, 1.0, -1.0, 0.0) * sampleScale;
	half3 s = (half3)0.0;

	//#if SHADER_TARGET > 30
	s = tex2D(tex, (uv - d.xy));
	s += tex2D(tex, (uv - d.wy)) * 2.0;
	s += tex2D(tex, (uv - d.zy));

	s += tex2D(tex, (uv + d.zw)) * 2.0;
	s += tex2D(tex, (uv))        * 4.0;
	s += tex2D(tex, (uv + d.xw)) * 2.0;

	s += tex2D(tex, (uv + d.zy));
	s += tex2D(tex, (uv + d.wy)) * 2.0;
	s += tex2D(tex, (uv + d.xy));
	//#endif


	return s * (1.0 / 16.0);
}

//From Kawase, 2003 GDC presentation
//http://developer.amd.com/wordpress/media/2012/10/Oat-ScenePostprocessing.pdf
half4 KawaseBlurMobile(sampler2D s, float2 uv, int iteration, float2 pixelSize)
{
	half2 halfPixelSize = pixelSize / 2.0;
	half2 dUV = (pixelSize.xy * float(iteration)) + halfPixelSize.xy;
	half4 cOut;
	half4 cheekySample = half4(uv.x, uv.x, uv.y, uv.y) + half4(-dUV.x, dUV.x, dUV.y, dUV.y);
	half4 cheekySample2 = half4(uv.x, uv.x, uv.y, uv.y) + half4(dUV.x, -dUV.x, -dUV.y, -dUV.y);

	cOut = tex2D(s, cheekySample.rb);
	cOut += tex2D(s, cheekySample.ga);
	cOut += tex2D(s, cheekySample.rb);
	cOut += tex2D(s, cheekySample.ga);
	cOut *= 0.25;
	return cOut;
}

//From Kawase, 2003 GDC presentation
//http://developer.amd.com/wordpress/media/2012/10/Oat-ScenePostprocessing.pdf
float4 KawaseBlur(Texture2D s, float2 uv, int iteration, SamplerState texSampler, float2 pixelSize)
{
	//Dont need anymorefloat2 texCoordSample = 0;
	//float2 pixelSize = _MainTex_TexelSize.xy * 0.5;//(float2)(1.0 / _ScreenParams.xy); //_MainTex_TexelSize.wx;
	float2 halfPixelSize = pixelSize / 2.0;
	float2 dUV = (pixelSize.xy * float(iteration)) + halfPixelSize.xy;
	float4 cOut;
	//We probably save like 1 operation from this, lol
	float4 cheekySample = float4(uv.x, uv.x, uv.y, uv.y) + float4(-dUV.x, dUV.x, dUV.y, dUV.y);
	float4 cheekySample2 = float4(uv.x, uv.x, uv.y, uv.y) + float4(dUV.x, -dUV.x, -dUV.y, -dUV.y);

	// Sample top left pixel
	cOut = s.Sample(texSampler, cheekySample.rb);
	// Sample top right pixel
	cOut += s.Sample(texSampler, cheekySample.ga);
	// Sample bottom right pixel
	cOut += s.Sample(texSampler, cheekySample2.rb);
	// Sample bottom left pixel
	cOut += s.Sample(texSampler, cheekySample2.ga);
	// Average
	cOut *= 0.25f;
	//return tex2D(s, uv);
	return cOut;
}


