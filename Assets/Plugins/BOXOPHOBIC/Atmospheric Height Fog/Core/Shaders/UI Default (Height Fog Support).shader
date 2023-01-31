// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "UI/Default (Height Fog Support)"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		[StyledCategory(Fog)]_FogCat("[ Fog Cat]", Float) = 1
		[Enum(X Axis,0,Y Axis,1,Z Axis,2)][Space(10)]_FogAxisMode("Fog Axis Mode", Float) = 1
		[StyledCategory(Skybox)]_SkyboxCat("[ Skybox Cat ]", Float) = 1
		[StyledCategory(Directional)]_DirectionalCat("[ Directional Cat ]", Float) = 1
		[StyledCategory(Noise)]_NoiseCat("[ Noise Cat ]", Float) = 1
		[ASEEnd][StyledCategory(Advanced)]_AdvancedCat("[ Advanced Cat ]", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}

	SubShader
	{
		LOD 0

		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
		
		Stencil
		{
			Ref [_Stencil]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
			CompFront [_StencilComp]
			PassFront [_StencilOp]
			FailFront Keep
			ZFailFront Keep
			CompBack Always
			PassBack Keep
			FailBack Keep
			ZFailBack Keep
		}


		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		
		Pass
		{
			Name "Default"
		CGPROGRAM
			
			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_CLIP_RECT
			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
			#include "UnityShaderVariables.cginc"
			#define ASE_NEEDS_FRAG_COLOR
			//Atmospheric Height Fog Defines
			//#define AHF_DISABLE_NOISE3D
			//#define AHF_DISABLE_DIRECTIONAL
			//#define AHF_DISABLE_SKYBOXFOG
			//#define AHF_DISABLE_FALLOFF
			//#define AHF_DEBUG_WORLDPOS

			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
			};
			
			uniform fixed4 _Color;
			uniform fixed4 _TextureSampleAdd;
			uniform float4 _ClipRect;
			uniform sampler2D _MainTex;
			uniform half _DirectionalCat;
			uniform half _NoiseCat;
			uniform half _SkyboxCat;
			uniform half _FogAxisMode;
			uniform half _FogCat;
			uniform half _AdvancedCat;
			uniform float4 _MainTex_ST;
			uniform half4 AHF_FogColorStart;
			uniform half4 AHF_FogColorEnd;
			uniform half AHF_FogDistanceStart;
			uniform half AHF_FogDistanceEnd;
			uniform half AHF_FogDistanceFalloff;
			uniform half AHF_FogColorDuo;
			uniform half4 AHF_DirectionalColor;
			uniform half3 AHF_DirectionalDir;
			uniform half AHF_DirectionalIntensity;
			uniform half AHF_DirectionalFalloff;
			uniform half3 AHF_FogAxisOption;
			uniform half AHF_FogHeightEnd;
			uniform half AHF_FogHeightStart;
			uniform half AHF_FogHeightFalloff;
			uniform half AHF_FogLayersMode;
			uniform half AHF_NoiseScale;
			uniform half3 AHF_NoiseSpeed;
			uniform half AHF_NoiseDistanceEnd;
			uniform half AHF_NoiseIntensity;
			uniform half AHF_FogIntensity;
			UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
			uniform float4 _CameraDepthTexture_TexelSize;
			float4 mod289( float4 x )
			{
				return x - floor(x * (1.0 / 289.0)) * 289.0;
			}
			
			float4 perm( float4 x )
			{
				return mod289(((x * 34.0) + 1.0) * x);
			}
			
			float SimpleNoise3D( float3 p )
			{
				    float3 a = floor(p);
				    float3 d = p - a;
				    d = d * d * (3.0 - 2.0 * d);
				    float4 b = a.xxyy + float4(0.0, 1.0, 0.0, 1.0);
				    float4 k1 = perm(b.xyxy);
				    float4 k2 = perm(k1.xyxy + b.zzww);
				    float4 c = k2 + a.zzzz;
				    float4 k3 = perm(c);
				    float4 k4 = perm(c + 1.0);
				    float4 o1 = frac(k3 * (1.0 / 41.0));
				    float4 o2 = frac(k4 * (1.0 / 41.0));
				    float4 o3 = o2 * d.z + o1 * (1.0 - d.z);
				    float2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);
				    return o4.y * d.y + o4.x * (1.0 - d.y);
			}
			
			float2 UnStereo( float2 UV )
			{
				#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[ unity_StereoEyeIndex];
				UV.xy = (UV.xy - scaleOffset.zw) / scaleOffset.xy;
				#endif
				return UV;
			}
			

			
			v2f vert( appdata_t IN  )
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID( IN );
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
				OUT.worldPosition = IN.vertex;
				float3 ase_worldPos = mul(unity_ObjectToWorld, IN.vertex).xyz;
				OUT.ase_texcoord2.xyz = ase_worldPos;
				float4 ase_clipPos = UnityObjectToClipPos(IN.vertex);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				OUT.ase_texcoord3 = screenPos;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				OUT.ase_texcoord2.w = 0;
				
				OUT.worldPosition.xyz +=  float3( 0, 0, 0 ) ;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = IN.texcoord;
				
				OUT.color = IN.color * _Color;
				return OUT;
			}

			fixed4 frag(v2f IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float2 uv_MainTex = IN.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 temp_output_4_0 = ( IN.color * ( tex2D( _MainTex, uv_MainTex ) + _TextureSampleAdd ) );
				float3 ase_worldPos = IN.ase_texcoord2.xyz;
				float3 WorldPosition2_g931 = ase_worldPos;
				float temp_output_7_0_g975 = AHF_FogDistanceStart;
				float temp_output_155_0_g931 = saturate( ( ( distance( WorldPosition2_g931 , _WorldSpaceCameraPos ) - temp_output_7_0_g975 ) / ( AHF_FogDistanceEnd - temp_output_7_0_g975 ) ) );
				#ifdef AHF_DISABLE_FALLOFF
				float staticSwitch467_g931 = temp_output_155_0_g931;
				#else
				float staticSwitch467_g931 = pow( abs( temp_output_155_0_g931 ) , AHF_FogDistanceFalloff );
				#endif
				half FogDistanceMask12_g931 = staticSwitch467_g931;
				float3 lerpResult258_g931 = lerp( (AHF_FogColorStart).rgb , (AHF_FogColorEnd).rgb , ( saturate( ( FogDistanceMask12_g931 - 0.5 ) ) * AHF_FogColorDuo ));
				float3 normalizeResult318_g931 = normalize( ( WorldPosition2_g931 - _WorldSpaceCameraPos ) );
				float dotResult145_g931 = dot( normalizeResult318_g931 , AHF_DirectionalDir );
				float temp_output_140_0_g931 = ( (dotResult145_g931*0.5 + 0.5) * AHF_DirectionalIntensity );
				#ifdef AHF_DISABLE_FALLOFF
				float staticSwitch470_g931 = temp_output_140_0_g931;
				#else
				float staticSwitch470_g931 = pow( abs( temp_output_140_0_g931 ) , AHF_DirectionalFalloff );
				#endif
				float DirectionalMask30_g931 = staticSwitch470_g931;
				float3 lerpResult40_g931 = lerp( lerpResult258_g931 , (AHF_DirectionalColor).rgb , DirectionalMask30_g931);
				#ifdef AHF_DISABLE_DIRECTIONAL
				float3 staticSwitch442_g931 = lerpResult258_g931;
				#else
				float3 staticSwitch442_g931 = lerpResult40_g931;
				#endif
				float3 temp_output_2_0_g974 = staticSwitch442_g931;
				float3 gammaToLinear3_g974 = GammaToLinearSpace( temp_output_2_0_g974 );
				#ifdef UNITY_COLORSPACE_GAMMA
				float3 staticSwitch1_g974 = temp_output_2_0_g974;
				#else
				float3 staticSwitch1_g974 = gammaToLinear3_g974;
				#endif
				half3 Final_Color462_g931 = staticSwitch1_g974;
				half3 AHF_FogAxisOption181_g931 = AHF_FogAxisOption;
				float3 break159_g931 = ( WorldPosition2_g931 * AHF_FogAxisOption181_g931 );
				float temp_output_7_0_g970 = AHF_FogHeightEnd;
				float temp_output_167_0_g931 = saturate( ( ( ( break159_g931.x + break159_g931.y + break159_g931.z ) - temp_output_7_0_g970 ) / ( AHF_FogHeightStart - temp_output_7_0_g970 ) ) );
				#ifdef AHF_DISABLE_FALLOFF
				float staticSwitch468_g931 = temp_output_167_0_g931;
				#else
				float staticSwitch468_g931 = pow( abs( temp_output_167_0_g931 ) , AHF_FogHeightFalloff );
				#endif
				half FogHeightMask16_g931 = staticSwitch468_g931;
				float lerpResult328_g931 = lerp( ( FogDistanceMask12_g931 * FogHeightMask16_g931 ) , saturate( ( FogDistanceMask12_g931 + FogHeightMask16_g931 ) ) , AHF_FogLayersMode);
				float mulTime204_g931 = _Time.y * 2.0;
				float3 temp_output_197_0_g931 = ( ( WorldPosition2_g931 * ( 1.0 / AHF_NoiseScale ) ) + ( -AHF_NoiseSpeed * mulTime204_g931 ) );
				float3 p1_g973 = temp_output_197_0_g931;
				float localSimpleNoise3D1_g973 = SimpleNoise3D( p1_g973 );
				float temp_output_7_0_g972 = AHF_NoiseDistanceEnd;
				half NoiseDistanceMask7_g931 = saturate( ( ( distance( WorldPosition2_g931 , _WorldSpaceCameraPos ) - temp_output_7_0_g972 ) / ( 0.0 - temp_output_7_0_g972 ) ) );
				float lerpResult198_g931 = lerp( 1.0 , (localSimpleNoise3D1_g973*0.5 + 0.5) , ( NoiseDistanceMask7_g931 * AHF_NoiseIntensity ));
				half NoiseSimplex3D24_g931 = lerpResult198_g931;
				#ifdef AHF_DISABLE_NOISE3D
				float staticSwitch42_g931 = lerpResult328_g931;
				#else
				float staticSwitch42_g931 = ( lerpResult328_g931 * NoiseSimplex3D24_g931 );
				#endif
				float temp_output_454_0_g931 = ( staticSwitch42_g931 * AHF_FogIntensity );
				half Final_Alpha463_g931 = temp_output_454_0_g931;
				float4 appendResult114_g931 = (float4(Final_Color462_g931 , Final_Alpha463_g931));
				float4 screenPos = IN.ase_texcoord3;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float2 UV235_g931 = ase_screenPosNorm.xy;
				float2 localUnStereo235_g931 = UnStereo( UV235_g931 );
				float2 break248_g931 = localUnStereo235_g931;
				float clampDepth227_g931 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch250_g931 = ( 1.0 - clampDepth227_g931 );
				#else
				float staticSwitch250_g931 = clampDepth227_g931;
				#endif
				float3 appendResult244_g931 = (float3(break248_g931.x , break248_g931.y , staticSwitch250_g931));
				float4 appendResult220_g931 = (float4((appendResult244_g931*2.0 + -1.0) , 1.0));
				float4 break229_g931 = mul( unity_CameraInvProjection, appendResult220_g931 );
				float3 appendResult237_g931 = (float3(break229_g931.x , break229_g931.y , break229_g931.z));
				float4 appendResult233_g931 = (float4(( ( appendResult237_g931 / break229_g931.w ) * half3(1,1,-1) ) , 1.0));
				float4 break245_g931 = mul( unity_CameraToWorld, appendResult233_g931 );
				float3 appendResult239_g931 = (float3(break245_g931.x , break245_g931.y , break245_g931.z));
				float3 WorldPositionFromDepth253_g931 = appendResult239_g931;
				float4 appendResult457_g931 = (float4(WorldPositionFromDepth253_g931 , 1.0));
				#ifdef AHF_DEBUG_WORLDPOS
				float4 staticSwitch456_g931 = appendResult457_g931;
				#else
				float4 staticSwitch456_g931 = appendResult114_g931;
				#endif
				float3 temp_output_95_86_g930 = (staticSwitch456_g931).xyz;
				float temp_output_95_87_g930 = (staticSwitch456_g931).w;
				float3 lerpResult82_g930 = lerp( (temp_output_4_0).rgb , temp_output_95_86_g930 , temp_output_95_87_g930);
				float4 appendResult9 = (float4(lerpResult82_g930 , (temp_output_4_0).a));
				
				half4 color = appendResult9;
				
				#ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				return color;
			}
		ENDCG
		}
	}
	
	
	
}
/*ASEBEGIN
Version=18934
1920;0;1920;1029;287.3055;790.6581;1;True;False
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;2;-512,0;Inherit;False;0;0;_MainTex;Shader;False;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-320,0;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;11;-320,192;Inherit;False;0;0;_TextureSampleAdd;Pass;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;12;-512,-256;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;10;64,64;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;256,-256;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SwizzleNode;6;448,-256;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;7;448,-160;Inherit;False;FLOAT;3;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;26;640,-256;Inherit;False;Apply Height Fog;0;;930;950890317d4f36a48a68d150cdab0168;0;1;81;FLOAT3;0,0,0;False;3;FLOAT3;85;FLOAT3;86;FLOAT;87
Node;AmplifyShaderEditor.DynamicAppendNode;9;896,-256;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;1088,-256;Float;False;True;-1;2;;0;4;UI/Default (Height Fog Support);5056123faa0c79b47ab6ad7e8bf059a4;True;Default;0;0;Default;2;False;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;True;True;True;True;True;0;True;-9;False;False;False;False;False;False;False;True;True;0;True;-5;255;True;-8;255;True;-7;0;True;-4;0;True;-6;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;0;True;-11;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;3;0;2;0
WireConnection;10;0;3;0
WireConnection;10;1;11;0
WireConnection;4;0;12;0
WireConnection;4;1;10;0
WireConnection;6;0;4;0
WireConnection;7;0;4;0
WireConnection;26;81;6;0
WireConnection;9;0;26;85
WireConnection;9;3;7;0
WireConnection;1;0;9;0
ASEEND*/
//CHKSM=6778C1A702309BA0803BB631C1CC8C00AD30C328