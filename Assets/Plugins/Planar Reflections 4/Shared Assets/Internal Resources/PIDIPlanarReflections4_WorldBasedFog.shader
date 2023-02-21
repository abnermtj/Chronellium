Shader "Hidden/PIDI SHaders Collection/Planar Reflections 4/World Based Fog"
{
    Properties
    {
        _P4FogCameraPosition("Camera position", Vector) = (0,0,0,0)
        _FogDensity("Fog Density", Float) = 0.03
        _FogStart("Fog Start", Float) = 0
        _FogEnd("Fog End", Float) = 300
        [Enum(Linear,0,Exp,1)]_FogMode("Fog Mode", Float ) = 0
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
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 worldFog : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float3 _P4FogCameraPosition;
            float _FogEnd;
            float _FogStart;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float worldDist = distance(worldPos, _WorldSpaceCameraPos);
                UNITY_CALC_FOG_FACTOR_RAW(worldDist);
                o.worldFog = unityFogFactor;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half4 col = lerp( float4(0,0,0,0), unity_FogColor, 1-i.worldFog.x);
                col.a = 1 - i.worldFog.x;
                
                return col;
            }
            ENDCG
        }
    }
}
