#ifndef __RELISTIC_RAIN_DROP_UTILS__
#define __RELISTIC_RAIN_DROP_UTILS__

#include "UnityCG.cginc"

struct appdata
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
};

struct v2f
{
	float4 pos : SV_POSITION;
	float4 scrPos : TEXCOORD0;
	float2 uv : TEXCOORD1;
};

sampler2D _MainTex;
sampler2D _SrcTex;
half4 _Params;

#define SCREEN_RAIN_DROP_FADE _Params.x
#define SCREEN_RAIN_DROP_INTENSITY _Params.y

float3 NOT(float p) {
	float3 p3 = frac(float3(p, p, p) * float3(.1031, .11369, .13787));
	p3 += dot(p3, p3.yzx + 19.19);
	return frac(float3((p3.x + p3.y) * p3.z, (p3.x + p3.z) * p3.y, (p3.y + p3.z) * p3.x));
}
float4 NOF(float t) {
	return frac(sin(t * float4(123., 1024., 1456., 264.)) * float4(6547., 345., 8799., 1564.));
}
float N(float t) {
	return frac(sin(t * 12345.564) * 7658.76);
}
float S(float b, float t) {
	return smoothstep(0., b, t) * smoothstep(1., b, t);
}
float2 DL2(float2 uv, float t) {
	float2 UV = uv;

	uv.y += t * 0.75;
	float2 a = float2(6., 1.);
	float2 grid = a * 2.;
	float2 id = floor(uv * grid);

	float colShift = N(id.x);
	uv.y += colShift;

	id = floor(uv * grid);
	float3 n = NOT(id.x * 35.2 + id.y * 2376.1);
	float2 st = frac(uv * grid) - float2(.5, 0);

	float x = n.x - .5;

	float y = UV.y * 20.;
	float wiggle = sin(y + sin(y));
	x += wiggle * (.5 - abs(x)) * (n.z - .5);
	x *= .7;
	float ti = frac(t + n.z);
	y = (S(.85, ti) - .5) * .9 + .5;
	float2 p = float2(x, y);

	float d = length((st - p) * a.yx);

	float mainDrop = smoothstep(.4, .0, d);

	float r = sqrt(smoothstep(1., y, st.y));
	float cd = abs(st.x - x);
	float trail = smoothstep(.23 * r, .15 * r * r, cd);
	float trailFront = smoothstep(-.02, .02, st.y - y);
	trail *= trailFront * r * r;

	y = UV.y;
	float trail2 = smoothstep(.2 * r, .0, cd);
	float droplets = max(0., (sin(y * (1. - y) * 120.) - st.y)) * trail2 * trailFront * n.z;
	y = frac(y * 10.) + (st.y - .5);
	float dd = length(st - float2(x, y));
	droplets = smoothstep(.3, 0., dd);
	float m = mainDrop + droplets * r * trailFront;

	return float2(m, trail);
}
float SDS(float2 uv, float t) {
	uv *= 40.;

	float2 id = floor(uv);
	uv = frac(uv) - .5;
	float3 n = NOT(id.x * 107.45 + id.y * 3543.654);
	float2 p = (n.xy - .5) * .7;
	float d = length(uv - p);

	float fade = S(.025, frac(t + n.z));
	float c = smoothstep(.3, 0., d) * frac(n.z * 10.) * fade;
	return c;
}
float2 DS(float2 uv, float t, float l0, float l1, float l2) {
	float s = SDS(uv, t) * l0;
	float2 m1 = DL2(uv, t) * l1;
	float2 m2 = DL2(uv * 1.85, t) * l2;

	float c = s + m1.x + m2.x;
	c = smoothstep(.3, 1., c);

	return float2(c, max(m1.y * l0, m2.y * l1));
}

v2f vert_rain_drop(appdata v)
{
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o)
		o.pos = UnityObjectToClipPos(v.vertex);
	o.scrPos = ComputeScreenPos(o.pos);
	return o;
}

fixed4 frag_rain_drop(v2f i) : SV_Target
{
	float2 fragCoord = (i.scrPos.xy / i.scrPos.w) * _ScreenParams.xy;
	float4 fragColor = 0;

	float2 uv = (fragCoord.xy - 2 * _ScreenParams.xy) / _ScreenParams.y;
	float2 UV = fragCoord.xy / _ScreenParams.xy;
	float3 M = 2;
	float T = _Time.y + M.x * 2.;

	float t = T * .2;

	float rainAmount = M.y;

	float maxBlur = lerp(3., 6., rainAmount);
	float minBlur = 2.;

	uv *= 0.5;

	float staticDrops = smoothstep(-.5, 1., rainAmount / 4000) * 2.;
	float layer1 = smoothstep(.25, .75, rainAmount / 200);
	float layer2 = smoothstep(.0, .5, rainAmount / 10);

	float2 c = DS(uv, t, staticDrops, layer1, layer2);
	float2 e = float2(.001, 0.);
	float cx = DS(uv + e, t, staticDrops, layer1, layer2).x;
	float cy = DS(uv + e.yx, t, staticDrops, layer1, layer2).x;
	float2 n = float2(cx - c.x, cy - c.x);

	float blend = (n.x + n.y) * SCREEN_RAIN_DROP_INTENSITY;
	float3 col = tex2D(_MainTex, UV + n).rgb;

	fragColor = float4(col, blend);

	return fragColor;
}

v2f vert_combined(appdata v)
{
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o)
		o.pos = UnityObjectToClipPos(v.vertex);
	o.uv = v.uv;
	return o;
}

fixed4 frag_combined(v2f i) : SV_Target
{
	fixed4 rainC = tex2D(_MainTex, i.uv);
	fixed4 mainC = tex2D(_SrcTex, i.uv);
	fixed blend = rainC.a * SCREEN_RAIN_DROP_FADE;
	mainC.rgb = rainC.rgb * blend + mainC.rgb * (1 - blend);
	return mainC;
}

#endif