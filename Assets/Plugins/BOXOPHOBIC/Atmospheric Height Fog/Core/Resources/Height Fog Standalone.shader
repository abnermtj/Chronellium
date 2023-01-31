// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "BOXOPHOBIC/Atmospherics/Height Fog Standalone"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[StyledBanner(Height Fog Standalone)]_Banner("Banner", Float) = 0
		[StyledCategory(Fog)]_FogCat("[ Fog Cat]", Float) = 1
		_FogIntensity("Fog Intensity", Range( 0 , 1)) = 1
		[Enum(X Axis,0,Y Axis,1,Z Axis,2)][Space(10)]_FogAxisMode("Fog Axis Mode", Float) = 1
		[Enum(Multiply Distance and Height,0,Additive Distance and Height,1)]_FogLayersMode("Fog Layers Mode", Float) = 0
		[HDR][Space(10)]_FogColorStart("Fog Color Start", Color) = (0.4411765,0.722515,1,0)
		[HDR]_FogColorEnd("Fog Color End", Color) = (0.4411765,0.722515,1,0)
		_FogColorDuo("Fog Color Duo", Range( 0 , 1)) = 1
		[Space(10)]_FogDistanceStart("Fog Distance Start", Float) = -200
		_FogDistanceEnd("Fog Distance End", Float) = 200
		_FogDistanceFalloff("Fog Distance Falloff", Range( 1 , 8)) = 2
		[Space(10)]_FogHeightStart("Fog Height Start", Float) = 0
		_FogHeightEnd("Fog Height End", Float) = 200
		_FogHeightFalloff("Fog Height Falloff", Range( 1 , 8)) = 2
		[StyledCategory(Skybox)]_SkyboxCat("[ Skybox Cat ]", Float) = 1
		_SkyboxFogIntensity("Skybox Fog Intensity", Range( 0 , 1)) = 0
		_SkyboxFogHeight("Skybox Fog Height", Range( 0 , 1)) = 1
		_SkyboxFogFalloff("Skybox Fog Falloff", Range( 1 , 8)) = 2
		_SkyboxFogOffset("Skybox Fog Offset", Range( -1 , 1)) = 0
		_SkyboxFogBottom("Skybox Fog Bottom", Range( 0 , 1)) = 0
		_SkyboxFogFill("Skybox Fog Fill", Range( 0 , 1)) = 0
		[StyledCategory(Directional)]_DirectionalCat("[ Directional Cat ]", Float) = 1
		[HDR]_DirectionalColor("Directional Color", Color) = (1,0.8280286,0.6084906,0)
		_DirectionalIntensity("Directional Intensity", Range( 0 , 1)) = 1
		_DirectionalFalloff("Directional Falloff", Range( 1 , 8)) = 2
		_DirectionalDir("Directional Dir", Vector) = (1,1,1,0)
		[StyledCategory(Noise)]_NoiseCat("[ Noise Cat ]", Float) = 1
		_NoiseIntensity("Noise Intensity", Range( 0 , 1)) = 1
		_NoiseDistanceEnd("Noise Distance End", Float) = 10
		_NoiseScale("Noise Scale", Float) = 30
		_NoiseSpeed("Noise Speed", Vector) = (0.5,0.5,0,0)
		[ASEEnd][StyledCategory(Advanced)]_AdvancedCat("[ Advanced Cat ]", Float) = 1
		[HideInInspector]_FogAxisOption("_FogAxisOption", Vector) = (0,0,0,0)
		[HideInInspector]_HeightFogStandalone("_HeightFogStandalone", Float) = 1
		[HideInInspector]_IsHeightFogShader("_IsHeightFogShader", Float) = 1

		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25
	}

	SubShader
	{
		LOD 0

		
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
		
		Cull Front
		AlphaToMask Off
		
		HLSLINCLUDE
		#pragma target 3.0

		#pragma prefer_hlslcc gles
		#pragma exclude_renderers d3d11_9x 

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}
		
		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS

		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			ZTest Always
			Offset 0 , 0
			ColorMask RGBA
			ZCLip False

			HLSLPROGRAM
			
			#define _RECEIVE_SHADOWS_OFF 1
			#define ASE_SRP_VERSION 100202
			#define REQUIRE_DEPTH_TEXTURE 1

			
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if ASE_SRP_VERSION <= 70108
			#define REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
			#endif

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			//Atmospheric Height Fog Defines
			//#define AHF_DISABLE_NOISE3D
			//#define AHF_DISABLE_DIRECTIONAL
			//#define AHF_DISABLE_SKYBOXFOG
			//#define AHF_DISABLE_FALLOFF
			//#define AHF_DEBUG_WORLDPOS


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				#ifdef ASE_FOG
				float fogFactor : TEXCOORD2;
				#endif
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			half4 _FogColorStart;
			half4 _DirectionalColor;
			half4 _FogColorEnd;
			half3 _DirectionalDir;
			half3 _NoiseSpeed;
			half3 _FogAxisOption;
			half _NoiseIntensity;
			half _NoiseDistanceEnd;
			half _SkyboxFogOffset;
			half _FogHeightEnd;
			half _SkyboxFogHeight;
			half _NoiseScale;
			half _FogLayersMode;
			half _FogHeightFalloff;
			half _FogHeightStart;
			half _SkyboxFogFalloff;
			half _SkyboxFogBottom;
			half _FogIntensity;
			float _Banner;
			half _DirectionalIntensity;
			half _SkyboxFogFill;
			half _FogColorDuo;
			half _FogDistanceFalloff;
			half _FogDistanceEnd;
			half _FogDistanceStart;
			half _AdvancedCat;
			half _SkyboxCat;
			half _NoiseCat;
			half _FogAxisMode;
			half _DirectionalCat;
			half _FogCat;
			half _IsHeightFogShader;
			half _HeightFogStandalone;
			half _DirectionalFalloff;
			half _SkyboxFogIntensity;
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
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
			
			float3 DepthByVersion226_g994( float3 _731, float3 _741 )
			{
				float3 result = _731;
				#if ASE_SRP_VERSION > 70301
				result = _741;
				#endif
				return result;
			}
			
			
			VertexOutput VertexFunction ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord3 = screenPos;
				
				o.ase_texcoord4 = v.vertex;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				#ifdef ASE_FOG
				o.fogFactor = ComputeFogFactor( positionCS.z );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif
				float4 screenPos = IN.ase_texcoord3;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float2 break248_g994 = (ase_screenPosNorm).xy;
				float clampDepth227_g994 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch250_g994 = ( 1.0 - clampDepth227_g994 );
				#else
				float staticSwitch250_g994 = clampDepth227_g994;
				#endif
				float3 appendResult244_g994 = (float3(break248_g994.x , break248_g994.y , staticSwitch250_g994));
				float4 appendResult220_g994 = (float4((appendResult244_g994*2.0 + -1.0) , 1.0));
				float4 break229_g994 = mul( unity_CameraInvProjection, appendResult220_g994 );
				float3 appendResult237_g994 = (float3(break229_g994.x , break229_g994.y , break229_g994.z));
				float4 appendResult233_g994 = (float4(( ( appendResult237_g994 / break229_g994.w ) * half3(1,1,-1) ) , 1.0));
				float4 break245_g994 = mul( unity_CameraToWorld, appendResult233_g994 );
				float3 appendResult239_g994 = (float3(break245_g994.x , break245_g994.y , break245_g994.z));
				float3 _731226_g994 = appendResult239_g994;
				float eyeDepth218_g994 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float4 unityObjectToClipPos224_g994 = TransformWorldToHClip(TransformObjectToWorld(IN.ase_texcoord4.xyz));
				float4 computeScreenPos225_g994 = ComputeScreenPos( unityObjectToClipPos224_g994 );
				float3 temp_output_216_0_g994 = ( _WorldSpaceCameraPos - ( eyeDepth218_g994 * ( ( _WorldSpaceCameraPos - WorldPosition ) / computeScreenPos225_g994.w ) ) );
				float3 _741226_g994 = temp_output_216_0_g994;
				float3 localDepthByVersion226_g994 = DepthByVersion226_g994( _731226_g994 , _741226_g994 );
				float3 WorldPositionFromDepth253_g994 = localDepthByVersion226_g994;
				float3 WorldPosition2_g994 = WorldPositionFromDepth253_g994;
				float temp_output_7_0_g996 = _FogDistanceStart;
				float temp_output_155_0_g994 = saturate( ( ( distance( WorldPosition2_g994 , _WorldSpaceCameraPos ) - temp_output_7_0_g996 ) / ( _FogDistanceEnd - temp_output_7_0_g996 ) ) );
				#ifdef AHF_DISABLE_FALLOFF
				float staticSwitch467_g994 = temp_output_155_0_g994;
				#else
				float staticSwitch467_g994 = pow( abs( temp_output_155_0_g994 ) , _FogDistanceFalloff );
				#endif
				half FogDistanceMask12_g994 = staticSwitch467_g994;
				float3 lerpResult258_g994 = lerp( (_FogColorStart).rgb , (_FogColorEnd).rgb , ( saturate( ( FogDistanceMask12_g994 - 0.5 ) ) * _FogColorDuo ));
				float3 normalizeResult318_g994 = normalize( ( WorldPosition2_g994 - _WorldSpaceCameraPos ) );
				float dotResult145_g994 = dot( normalizeResult318_g994 , _DirectionalDir );
				float temp_output_140_0_g994 = ( (dotResult145_g994*0.5 + 0.5) * _DirectionalIntensity );
				#ifdef AHF_DISABLE_FALLOFF
				float staticSwitch470_g994 = temp_output_140_0_g994;
				#else
				float staticSwitch470_g994 = pow( abs( temp_output_140_0_g994 ) , _DirectionalFalloff );
				#endif
				float DirectionalMask30_g994 = staticSwitch470_g994;
				float3 lerpResult40_g994 = lerp( lerpResult258_g994 , (_DirectionalColor).rgb , DirectionalMask30_g994);
				#ifdef AHF_DISABLE_DIRECTIONAL
				float3 staticSwitch442_g994 = lerpResult258_g994;
				#else
				float3 staticSwitch442_g994 = lerpResult40_g994;
				#endif
				float3 temp_output_2_0_g995 = staticSwitch442_g994;
				float3 gammaToLinear3_g995 = FastSRGBToLinear( temp_output_2_0_g995 );
				#ifdef UNITY_COLORSPACE_GAMMA
				float3 staticSwitch1_g995 = temp_output_2_0_g995;
				#else
				float3 staticSwitch1_g995 = gammaToLinear3_g995;
				#endif
				half3 Final_Color462_g994 = staticSwitch1_g995;
				half3 AHF_FogAxisOption181_g994 = _FogAxisOption;
				float3 break159_g994 = ( WorldPosition2_g994 * AHF_FogAxisOption181_g994 );
				float temp_output_7_0_g997 = _FogHeightEnd;
				float temp_output_167_0_g994 = saturate( ( ( ( break159_g994.x + break159_g994.y + break159_g994.z ) - temp_output_7_0_g997 ) / ( _FogHeightStart - temp_output_7_0_g997 ) ) );
				#ifdef AHF_DISABLE_FALLOFF
				float staticSwitch468_g994 = temp_output_167_0_g994;
				#else
				float staticSwitch468_g994 = pow( abs( temp_output_167_0_g994 ) , _FogHeightFalloff );
				#endif
				half FogHeightMask16_g994 = staticSwitch468_g994;
				float lerpResult328_g994 = lerp( ( FogDistanceMask12_g994 * FogHeightMask16_g994 ) , saturate( ( FogDistanceMask12_g994 + FogHeightMask16_g994 ) ) , _FogLayersMode);
				float mulTime204_g994 = _TimeParameters.x * 2.0;
				float3 temp_output_197_0_g994 = ( ( WorldPosition2_g994 * ( 1.0 / _NoiseScale ) ) + ( -_NoiseSpeed * mulTime204_g994 ) );
				float3 p1_g998 = temp_output_197_0_g994;
				float localSimpleNoise3D1_g998 = SimpleNoise3D( p1_g998 );
				float temp_output_7_0_g1000 = _NoiseDistanceEnd;
				half NoiseDistanceMask7_g994 = saturate( ( ( distance( WorldPosition2_g994 , _WorldSpaceCameraPos ) - temp_output_7_0_g1000 ) / ( 0.0 - temp_output_7_0_g1000 ) ) );
				float lerpResult198_g994 = lerp( 1.0 , (localSimpleNoise3D1_g998*0.5 + 0.5) , ( NoiseDistanceMask7_g994 * _NoiseIntensity ));
				half NoiseSimplex3D24_g994 = lerpResult198_g994;
				#ifdef AHF_DISABLE_NOISE3D
				float staticSwitch42_g994 = lerpResult328_g994;
				#else
				float staticSwitch42_g994 = ( lerpResult328_g994 * NoiseSimplex3D24_g994 );
				#endif
				float temp_output_454_0_g994 = ( staticSwitch42_g994 * _FogIntensity );
				float3 normalizeResult169_g994 = normalize( ( WorldPosition2_g994 - _WorldSpaceCameraPos ) );
				float3 break170_g994 = ( normalizeResult169_g994 * AHF_FogAxisOption181_g994 );
				float temp_output_449_0_g994 = ( ( break170_g994.x + break170_g994.y + break170_g994.z ) + -_SkyboxFogOffset );
				float temp_output_7_0_g999 = _SkyboxFogHeight;
				float temp_output_176_0_g994 = saturate( ( ( abs( temp_output_449_0_g994 ) - temp_output_7_0_g999 ) / ( 0.0 - temp_output_7_0_g999 ) ) );
				float saferPower309_g994 = abs( temp_output_176_0_g994 );
				#ifdef AHF_DISABLE_FALLOFF
				float staticSwitch469_g994 = temp_output_176_0_g994;
				#else
				float staticSwitch469_g994 = pow( saferPower309_g994 , _SkyboxFogFalloff );
				#endif
				float lerpResult179_g994 = lerp( saturate( ( staticSwitch469_g994 + ( _SkyboxFogBottom * step( temp_output_449_0_g994 , 0.0 ) ) ) ) , 1.0 , _SkyboxFogFill);
				half SkyboxFogHeightMask108_g994 = ( lerpResult179_g994 * _SkyboxFogIntensity );
				float clampDepth118_g994 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch123_g994 = clampDepth118_g994;
				#else
				float staticSwitch123_g994 = ( 1.0 - clampDepth118_g994 );
				#endif
				half SkyboxFogMask95_g994 = ( 1.0 - ceil( staticSwitch123_g994 ) );
				float lerpResult112_g994 = lerp( temp_output_454_0_g994 , SkyboxFogHeightMask108_g994 , SkyboxFogMask95_g994);
				#ifdef AHF_DISABLE_SKYBOXFOG
				float staticSwitch455_g994 = temp_output_454_0_g994;
				#else
				float staticSwitch455_g994 = lerpResult112_g994;
				#endif
				half Final_Alpha463_g994 = staticSwitch455_g994;
				float4 appendResult114_g994 = (float4(Final_Color462_g994 , Final_Alpha463_g994));
				float4 appendResult457_g994 = (float4(WorldPosition2_g994 , 1.0));
				#ifdef AHF_DEBUG_WORLDPOS
				float4 staticSwitch456_g994 = appendResult457_g994;
				#else
				float4 staticSwitch456_g994 = appendResult114_g994;
				#endif
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = (staticSwitch456_g994).xyz;
				float Alpha = (staticSwitch456_g994).w;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#ifdef ASE_FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				return half4( Color, Alpha );
			}

			ENDHLSL
		}

	
	}
	
	CustomEditor "HeightFogShaderGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=18934
1920;0;1920;1029;4150.203;5034.952;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;1093;-3328,-4736;Inherit;False;Property;_Banner;Banner;0;0;Create;True;0;0;0;True;1;StyledBanner(Height Fog Standalone);False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1107;-3136,-4736;Half;False;Property;_HeightFogStandalone;_HeightFogStandalone;34;1;[HideInInspector];Create;False;0;0;0;True;0;False;1;1;1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1106;-2880,-4736;Half;False;Property;_IsHeightFogShader;_IsHeightFogShader;35;1;[HideInInspector];Create;False;0;0;0;True;0;False;1;1;1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;1127;-3328,-4608;Inherit;False;Base;1;;994;13c50910e5b86de4097e1181ba121e0e;28,347,1,386,1,370,1,384,1,378,1,388,1,343,1,376,1,339,1,382,1,380,1,355,1,392,1,366,1,116,1,349,1,354,1,99,1,360,1,450,1,364,1,368,1,476,1,361,1,345,1,351,1,374,1,372,1;0;3;FLOAT4;113;FLOAT3;86;FLOAT;87
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1120;-3072,-4608;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1121;-3072,-4608;Float;False;True;-1;2;HeightFogShaderGUI;0;3;BOXOPHOBIC/Atmospherics/Height Fog Standalone;2992e84f91cbeb14eab234972e07ea9d;True;Forward;0;1;Forward;8;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;1;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;5;False;-1;10;False;-1;1;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;7;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;False;0;Hidden/InternalErrorShader;0;0;Standard;22;Surface;1;0;  Blend;0;0;Two Sided;2;0;Cast Shadows;0;0;  Use Shadow Threshold;0;0;Receive Shadows;0;0;GPU Instancing;0;0;LOD CrossFade;0;0;Built-in Fog;0;0;DOTS Instancing;0;0;Meta Pass;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,-1;0;  Type;0;0;  Tess;16,False,-1;0;  Min;10,False,-1;0;  Max;25,False,-1;0;  Edge Length;16,False,-1;0;  Max Displacement;25,False,-1;0;Vertex Position,InvertActionOnDeselection;1;0;0;5;False;True;False;False;False;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1122;-3072,-4608;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1123;-3072,-4608;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1124;-3072,-4608;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.CommentaryNode;1105;-3328,-4864;Inherit;False;919.8825;100;Drawers;0;;1,0.475862,0,1;0;0
WireConnection;1121;2;1127;86
WireConnection;1121;3;1127;87
ASEEND*/
//CHKSM=CA93F68B7350F2D582193C2B654F86968ACD4A32