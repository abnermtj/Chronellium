Shader "Hidden/PIDI Shaders Collection/Planar Reflections 4/Blur Pass"
{
	Properties
	{
		[Enum(Low,4,Normal,8,High,16)]_KernelSize("Blur Quality", Float) = 16
		[PerRendererData]_Radius("Blur Radius", Range( 1, 32 )) = 1
		[PerRendererData]_MainTex ("Texture", 2D) = "white" {}
		[PerRendererData]_ReflectionDepth ("Texture", 2D) = "black" {}
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Opaque" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" }
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
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			int _KernelSize;
			float _Radius;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float2 _MainTex_TexelSize;

			static const float TWO_PI = 6.28319;
			static const float E = 2.71828;

			float gaussian(int x, int y)
			{
				float sigmaSqu = _Radius * _Radius;
				return (1 / sqrt(TWO_PI * sigmaSqu)) * pow(E, -((x * x) + (y * y)) / (2 * sigmaSqu));
			}

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
			
				fixed4 col = fixed4(0,0,0,1);

				int upper = ((_KernelSize - 1) / 2);
				int lower = -upper;
				float kernelSum;

				for (int x = lower; x <= upper; ++x)
				{
					for (int y = lower; y <= upper; ++y)
					{
						float gauss = gaussian(x, y);
						kernelSum += gauss;

						fixed2 offset = fixed2(_MainTex_TexelSize.x * x, _MainTex_TexelSize.y * y);
						col += gauss * tex2D(_MainTex, i.uv + offset);
					}
				}

				col /= kernelSum;

				return col;
			}
			ENDCG
		}
		

	}
}
