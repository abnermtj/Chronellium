#define MOD3 float3(443.8975, 397.2973, 491.1871)
#define MAGIC float3(0.06711056, 0.00583715, 52.9829189)

float mod(float x, float y)
{
	return x - y * floor(x / y);
}

// Depth/normal sampling functions
float gnoise(float2 uv, float2 offs, float2 texelSize)
{
	uv = uv / texelSize.xy + offs;
	return frac(MAGIC.z * frac(dot(uv, MAGIC.xy)));
}

float hash12(float2 uv)
{
	float3 p3 = frac(float3(uv.xyx) * MOD3);
	p3 += dot(p3, p3.yzx + 19.19);
	return frac((p3.x + p3.y) * p3.z);
}

float tvNoise(float2 uv, float _time)
{
	return frac(sin(frac(_time) / 10.0 * dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
}


//http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/
//Super Cool!
float QuasiRandomNoise(float2 uv)
{
	const float2 magic = float2(0.75487766624669276, 0.569840290998);
	return frac(dot(uv, magic));
}

// Convert uniform distribution into triangle-shaped distribution.
float triRemap(float v)
{
	float orig = v * 2.0 - 1.0;

	//Note: if GLSL, need ABS :(
	v = orig * rsqrt(abs(orig));
	return max(-1.0, v) - sign(orig);

	//TODO investigate range -0.5, 1.5 instead of 0-1. +o.5 l33, (x +o.5) * 0.5 l34;
}

inline float3 QuasiDither(float3 col, float2 uv, float nBitDepth, float4 mt_TSize)
{
	uv = uv * mt_TSize.xy;

	float ditherVal = triRemap(QuasiRandomNoise(uv));

	col += ditherVal / nBitDepth;

	return round(col * nBitDepth) / nBitDepth;
}

float PowerNoise(float2 uv, float b, float dt, float w, float time, float speed, float angle)
{
	float y = mod(time * (dt + speed), 1.0);
	//float2 bothDir = (float2)1.0 - clamp(abs(uv - y), (float2)0.0, (float2)w);
	//float d = lerp(bothDir.x, bothDir.y, angle);
	//float d = 1.0 - clamp(abs(uv.y - y), 0.0, w) / w;
	float d1 = 1.0 - clamp(abs(uv.y - y), 0.0, w) / w;
	float d2 = 1.0 - clamp(abs(uv.x - y), 0.0, w) / w;
	float d = lerp(d1, d2, angle);

	return b * d;
	//return col *= (b * d);
	//return pow(col, float(1.0 / (1.0 + b * d)));
}

inline float3 ApplyTVNoise(float3 col, float2 uv, float _GrainIntensity, float _UnityTime, float4 mt_TSize, float4 _GrainTVValues)
{
	return col;
	//TVLines
	if (_GrainTVValues.w > 0.0)
	{
		//float tv = PowerNoise(col, uv / mt_TSize.zw, 4.0, -0.2, 0.1, _UnityTime, _GrainTVValues.w);
		//col *= tv;
	}

	return col;
}


//Actually noise
inline float3 ApplyNoise(float3 col, float2 uv, float _GrainScale, float _GrainIntensity, float _UnityTime, float4 mt_TSize, float4 sats, float4 _GrainTVValues)
{
	if (_GrainScale > 0.0)
	{
		half aspecRatioX = mt_TSize.w / mt_TSize.z;// half2(62.0, 42.0);// 12.0 / mt_TSize.xy;// (half2)_GrainScale * mt_TSize.wz;
		half2 gScaleMult = (half2)_GrainScale * half2(1., aspecRatioX);
		uv = round(uv * gScaleMult);// step(uv.x, uv.y);

		if (sats.w > 0.0)
		{
			float tv = PowerNoise(uv / mt_TSize.zw, 4.0, _GrainTVValues.g, _GrainTVValues.b, _UnityTime, sats.w, _GrainTVValues.r);
			_GrainIntensity = pow(_GrainIntensity, float(1.0 / (1.0 + tv)));
		}
	}

	float3 ditherCol;//
	float2 seed = uv;
	seed += frac(_UnityTime);
	float ditherAmount = _GrainIntensity * 0.15;

	ditherCol.r = (hash12(seed) + hash12(seed + 0.59374) - 0.5);
	seed += 0.1;
	ditherCol.g = (hash12(seed) + hash12(seed + 0.59374) - 0.5);
	seed += 0.04;
	ditherCol.b = (hash12(seed) + hash12(seed + 0.59374) - 0.5);

	ditherCol.rgb = round(ditherCol.rgb * sats.rgb);
	//ditherCol *= 0.1;


	//Should really be a value of some sort. Maybe "Sensor Lowlight Performance" and just push low values up?
	if (ditherCol.r > 0.85 && ditherCol.g > 0.85 && ditherCol.b > 0.85)
	{
		return col;
	}

	//What we could do is convert to LAB space, then apply a value to correct the L

	col += ditherCol *ditherAmount;
	return col;
}