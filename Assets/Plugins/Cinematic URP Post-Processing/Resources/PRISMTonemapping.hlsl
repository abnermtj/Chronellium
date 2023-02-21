
#define s2(a, b)				temp = a; a = min(a, b); b = max(temp, b);
#define mn3(a, b, c)			s2(a, b); s2(a, c);
#define mx3(a, b, c)			s2(b, c); s2(a, c);

//=================================================================================================
//
//  ACES implementation used is from: Baking Lab by MJP and David Neubelt http://mynameismjp.wordpress.com/
//
//  Licensed under the MIT license
//
//=================================================================================================

// sRGB => XYZ => D65_2_D60 => AP1 => RRT_SAT
static const float3x3 ACESInputMat =
{
    {0.59719, 0.35458, 0.04823},
    {0.07600, 0.90834, 0.01566},
    {0.02840, 0.13383, 0.83777}
};

// ODT_SAT => XYZ => D60_2_D65 => sRGB
static const float3x3 ACESOutputMat =
{
    { 1.60475, -0.53108, -0.07367},
    {-0.10208,  1.10813, -0.00605},
    {-0.00327, -0.07276,  1.07602}
};

float3 RRTAndODTFit(float3 v)
{
    float3 a = v * (v + 0.0245786f) - 0.000090537f;
    float3 b = v * (0.983729f * v + 0.4329510f) + 0.238081f;
    return a / b;
}

float3 ACESFitted(float3 color)
{
    color = mul(ACESInputMat, color);

    // Apply RRT and ODT
    color = RRTAndODTFit(color);

    color = mul(ACESOutputMat, color);

    // Clamp to [0, 1]
    color = saturate(color);

    return color;
}


//END ACES IMPLEMENTATION =====================================================================================================

//Roman Galashov (RomBinDaHouse) operator - https://www.shadertoy.com/view/MssXz7
half3 filmicTonemapRomBOld(in float3 col, in float gammaval)
{
	//return exp(_ToneParams.x / (_ToneParams.y * col.rgb + _ToneParams.z));

	col = exp(-1.0 / (2.72*col + 0.15));
	col = pow(col, (float3)(gammaval / 1.));
	
	return col;
}

half3 filmicTonemapRomB(in float3 col, in float gammaval)
{
   return  pow(ACESFitted(col), (float3)(gammaval));// ACESFitted(col);	
}

// Compatibility function
float fastReciprocal(float value)
{
	return 1. / value;
}

// Tonemapper from http://gpuopen.com/optimized-reversible-tonemapper-for-resolve/
float4 FastToneMapPRISM(in float4 color)
{
	return color;
	float v[2];
	v[0] = color.r;
	v[1] = color.g;
	v[2] = color.b;//

	float3 temp;
	mx3(v[0], v[1], v[2]);

	return float4(color.rgb * fastReciprocal(
		v[1]
		+ 1.)
		, color.a
		);
}