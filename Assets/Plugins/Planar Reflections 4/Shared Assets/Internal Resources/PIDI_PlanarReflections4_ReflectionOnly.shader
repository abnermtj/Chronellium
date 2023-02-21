/*
 * PIDI Planar Reflections 4
 * Developed  by : Jorge Pinal Negrete.
 * Copyright(c) 2017-2021, Jorge Pinal Negrete.  All Rights Reserved.
 *
*/


Shader "PIDI Shaders Collection/Planar Reflections 4/Unlit/Reflection Only"
{
	Properties
	{
		[PerRendererData]_ReflectionTex ("Reflection Texture", 2D) = "black" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 screenPos : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _ReflectionTex;
			float4 _ReflectionTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeGrabScreenPos(o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{

				i.screenPos.xy /= i.screenPos.w;
				i.screenPos.x = 1-i.screenPos.x;
				//i.screenPos.y = 1-i.screenPos.y;

				// sample the texture
				fixed4 col = tex2D(_ReflectionTex, i.screenPos.xy);
				return col;
			}
			ENDCG
		}
	}
}
