#include "CUPPCore.hlsl"

static const float pi = 3.14159265358979323846;
static const float epsilon = 1e-6;

static const float fringeExp = 2.3;
static const float fringeScale = 0.02;
static const float distortionExp = 0.1;
static const float distortionScale = 0.65;

static const float startAngle = 1.23456 + 3.14159265358979323846;	// tweak to get different fringe colouration
static const float angleStep = 3.14159265358979323846 * 2.0 / 3.0;	// space samples every 120 degrees
static const float _LensStrength = 1.0;

float2 ChromaticAberrationFringeUV(inout float2 uv, float4 mt_TexelSize)
{
	float2 fromCentre = uv - float2(0.5, 0.5);

	// correct for aspect ratio
	fromCentre.y *= mt_TexelSize.w / mt_TexelSize.z;

	float radius = length(fromCentre);

	fromCentre = radius > epsilon
		? (fromCentre * (1.0 / radius))
		: (float2)0.0;

	return fromCentre;
}

//uvR = centre UV that gets converted into UV for the Red sample. returns float4(uvG, uvG, uvB, uvB)
float4 ChromaticAberrationGetThreeUVs(inout float2 uvR, float2 fromCentre, float fringeStr, float4 mt_TexelSize, float _GlassAngle)
{
	//SHOULD ACTUALLY BE CONTRAST
	float radius = length(fromCentre);

	float fringing = fringeScale * pow(radius, fringeExp) * fringeStr;

	float distortion = 0.0;

	float2 distortUV = uvR - fromCentre * distortion;

	float angle;
	float2 dir;
	float4 gbUVs = (float4)0.0;

	float rotation = 1.0 + mt_TexelSize.w * 2.0 * pi;
	//Red
	float sAngle = _GlassAngle + pi;
	angle = sAngle + rotation;// +rotation;
	dir = float2(sin(angle), cos(angle));
	uvR = distortUV + fringing * dir;

	//Green
	angle += angleStep;
	dir = float2(sin(angle), cos(angle));
	gbUVs.rg = distortUV + fringing * dir;

	//Blue
	angle += angleStep;
	dir = float2(sin(angle), cos(angle));
	gbUVs.ba = distortUV + fringing * dir;

	return gbUVs;
}

//DistortAmount = 0.2
float2 LensDistortUV(float2 uv, float distortAmount)
{
	// Lens distortion
	float2 dir = uv - float2(0.5,0.5);
	uv += dir * dot(dir, dir) * distortAmount;
}

float2x2 rotate(float angle)
{
	float s = sin(angle);
	float c = cos(angle);
	return float2x2(c, -s, s, c);
}

float dist(float3 p) {
	p.xz = mul(rotate(p.y), p.xz);
	p.yz = mul(rotate(3.1415 / 2.), p.yz);
	return length(p) - 0.01;
}

float3 norm(float3 p) {
	float2 e = float2(0.1, 0.0);
	return normalize(float3(
		dist(p + e.xyy) - dist(p - e.xyy),
		dist(p + e.yxx) - dist(p - e.yxx),
		dist(p + e.yxx) - dist(p - e.yxx)
	));
}

float3 PetzvalDistortMask(float2 uv)
{
	uv = (uv*2.0 - 1) / 1;
	float amount, distance = 10.0;
	float3 petzval, rd = float3(uv, 0.0);
	petzval = rd * distance;
	distance += amount = dist(petzval);
	float3 col = norm(petzval)*0.5 + 0.5;
	return col;
}

//Amount should be 0 coming in
float2 LensDistortFisheye(float2 uv, float _GlassAngle, float _NegativeBarrel, inout float amount)
{
	const float2 ctr = float2(0.5, 0.5);
	float2 ctrvec = ctr - uv;
	float ctrdist = length(ctrvec);

	amount = max(0.0, pow(ctrdist, _GlassAngle) - 0.0025);

	if (_NegativeBarrel > 0.5)
	{
		amount = _NegativeBarrel + ctrdist * _GlassAngle;
	}

	uv = amount * (uv - 0.5) + 0.5;
	return uv;
}

#define scale 0.0
float2 LensDistortBiotar(float2 uv, float disp)
{
	//disp = .76 
	uv = uv * 2.0 - 1.0;
	float scaleDisp = scale - disp;
	return(uv * (pow(length(uv) - scaleDisp, .6) * 0.76) + 0.5);
}


float3 PrismVignette(float2 uv, float3 col, float vigStrength)
{
	// Vignette
	float2 d = abs(uv - (float2)0.5) * vigStrength;
	d = pow(d, (float2)2.0);
	col *= pow(saturate(1.0 - dot(d, d)), 3.0);

	return col;
}

float PrismVignetteMultiplier(float2 uv, float vigStrength)
{
	if (vigStrength == 0.0) return 1.0;

	// Vignette
	float2 d = abs(uv - (float2)0.5) * vigStrength;
	d = pow(d, (float2)2.0);
	return pow(saturate(1.0 - dot(d, d)), 3.0);
}


/*// Chromatic aberration
vec2 offset = vec2(1.0, 1.0) * 0.005;
col.r = texture(iChannel1, uv - offset).r;
col.g = texture(iChannel1, uv).g;
col.b = texture(iChannel1, uv + offset).b;*/
