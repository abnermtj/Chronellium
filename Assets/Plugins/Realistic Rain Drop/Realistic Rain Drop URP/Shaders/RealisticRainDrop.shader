Shader "Hidden/RealisticRainDrop"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_rain_drop
			#pragma fragment frag_rain_drop
			#pragma target 3.0
			#include "RealisticRainDropUtils.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_combined
			#pragma fragment frag_combined
			#pragma target 3.0
			#include "RealisticRainDropUtils.cginc"
			ENDCG
		}
	}
}
