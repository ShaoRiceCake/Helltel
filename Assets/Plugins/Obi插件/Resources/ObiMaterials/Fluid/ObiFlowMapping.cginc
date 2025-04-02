#ifndef OBIFLOWMAPPING_INCLUDED
#define OBIFLOWMAPPING_INCLUDED

void FlowmapAdvect_float(in float3 position, in float3 velocity, in float4 jump, in float speedScale, in float offset, in float time, in float noise, out float3 flowUV1, out float3 flowUV2, out float2 weights)
{
    velocity *= speedScale;
    time /= speedScale;

    time += noise;

    float phase1 = frac(time);
    float phase2 = frac(time + 0.5);
    
    // Offset changes amount of distortion at phase peak, goes from -0.5 to 0.
    flowUV1 = position - velocity * (phase1 + offset);
    flowUV2 = position - velocity * (phase2 + offset);

    // jump offsets starting UV position, goes from -0.5 to 0.5.
    flowUV1 += (time - phase1) * jump.xyz; 
    flowUV2 += (time - phase2) * jump.xyz + jump.w;

    weights = 1 - abs(1 - 2 *float2(phase1, phase2));
}

#endif
