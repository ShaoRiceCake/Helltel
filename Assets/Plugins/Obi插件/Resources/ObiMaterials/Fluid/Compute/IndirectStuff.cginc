#ifndef INDIRECTSTUFF_INCLUDED
#define INDIRECTSTUFF_INCLUDED

#define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
#include "UnityIndirect.cginc"

void DecodeNormal_float( float k, out float3 normal)
{
    uint d = asuint(k);
    float2 f = float2((d >> 16) / 65535.0, (d & 0xffff) / 65535.0) * 2.0-1.0;
     
    // https://twitter.com/Stubbesaurus/status/937994790553227264
    float3 n = float3( f.x, f.y, 1.0 - abs( f.x ) - abs( f.y ) );
    float t = saturate( -n.z );
    n.xy += n.xy >= 0.0 ? -t : t;
    normal = normalize( n );
}

StructuredBuffer<float4> _Vertices;
StructuredBuffer<float4> _Colors;
StructuredBuffer<float4> _Velocities;
uniform float4x4 _ObjectToWorld;

inline float3 projectOnPlane( float3 vec, float3 normal )
{
    return dot(normal, vec) * normal;
}

void GetIndirectVertex_float(uint svVertexID, uint svInstanceID,
					   out float3 worldPos, out float3 worldNormal, out float4 worldVelocity, out float3 worldView, out float4 color)
{
    InitIndirectDrawArgs(0);
    
    float4 v = _Vertices[GetIndirectVertexID(svVertexID)];
    float3 nrm;
    DecodeNormal_float(v.w,nrm);

    float4 vel = _Velocities[GetIndirectVertexID(svVertexID)];
    
    worldPos = mul(_ObjectToWorld, float4(v.xyz, 1.0f)).xyz;
    worldVelocity = float4(mul(_ObjectToWorld, float4(vel.xyz, 0.0f)).xyz, vel.w);
    worldNormal = mul((float3x3)_ObjectToWorld, nrm); // assume uniform scale.
    color = _Colors[GetIndirectVertexID(svVertexID)];
    
    if (unity_OrthoParams.w < 0.5)
        worldView = _WorldSpaceCameraPos.xyz - worldPos.xyz;
    else
        worldView = projectOnPlane(_WorldSpaceCameraPos.xyz - worldPos.xyz, -UNITY_MATRIX_V[2].xyz);
}

#endif
