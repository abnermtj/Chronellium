#ifndef YETMAN_POSTPROCESS_CORE_INCLUDED
#define YETMAN_POSTPROCESS_CORE_INCLUDED


#pragma warning( disable:4005 )
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//6 - (VIRTUAL_TAPS - 1) / 2	// 4, 3, 2

shared float2 texelSize;
float2 DIRECTION;
shared float centerTapWeight;
shared float4 tapWeights, tapOffsets;
//float4 _MainTex_TexelSize;

// Instead of recieving the vertex position, we just receive a vertex id (0,1,2)
// and convert it to a clip-space postion in the vertex shader
struct FullScreenTrianglePostProcessAttributes
{
    uint vertexID : SV_VertexID;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// This is what the fragment program should recieve if you use "FullScreenTrianglePostProcessVertexProgram" as the vertex shader
struct PostProcessVaryings
{
    float4 positionCS : SV_POSITION;
    float2 texcoord   : TEXCOORD0;
    UNITY_VERTEX_OUTPUT_STEREO
};

struct VS_OUTPUT
{
	float4 positionCS : SV_POSITION;
	float2 centerTap : TEXCOORD0;
	float4 positiveTaps[2] : TEXCOORD1;
	float4 negativeTaps[2] : TEXCOORD3;
};

PostProcessVaryings FullScreenTrianglePostProcessVertexProgram(FullScreenTrianglePostProcessAttributes input)
{
    PostProcessVaryings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
    output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
    return output;
}


#endif