Shader "Custom/WetRoad"
{
	Properties{
		_Color("Color",Color) = (1,1,1,1)
		_Cube("Cubemap", CUBE) = "" {}
		_Metallic("Metallic",Range(0,1)) = 1
		_Smoothness("Smoothness",Range(0,1)) = 1
		_Alpha("Alpha",Range(0,1)) = 1
	}
		SubShader{
		Tags {"RenderType" = "Transparent" "Queue" = "Transparent"}
		LOD 200
		Pass {
		ColorMask 0
		}
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask RGB

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows alpha:fade

		struct Input {
		float2 uv_MainTex;
		float3 worldRefl;
		};
		sampler2D _MainTex;
		samplerCUBE _Cube;
		float4 _Color;
		float _Metallic;
		float _Smoothness;
		float4 _EmissionColor;
		float _Alpha;
		void surf(Input IN, inout SurfaceOutputStandard o) {
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

		o.Albedo = c.rgb * 0.5 * _Color;
		o.Emission = texCUBE(_Cube, IN.worldRefl).rgb * _Color;
		o.Metallic = _Metallic;
		o.Smoothness = _Smoothness;
		o.Alpha = _Alpha;
		}
		ENDCG
		}
			Fallback "Diffuse"
}