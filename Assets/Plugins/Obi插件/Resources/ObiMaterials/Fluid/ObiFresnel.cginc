#ifndef OBIFRESNEL_INCLUDED
#define OBIFRESNEL_INCLUDED

// https://viclw17.github.io/2018/08/05/raytracing-dielectric-materials
float3 RefractVector(float cosi, float cost2, float3 normal, float3 incident, float n1_over_n2)
{
    return n1_over_n2 * incident + ((n1_over_n2 * cosi - sqrt(cost2)) * normal);
}

float SchlickFresnel(float cosi, float cost2, float ior, out bool TIR) 
{
   TIR = false;
   float r0 = (ior - 1) / (ior + 1);
   r0 *= r0;

   // handle total internal reflection
   if (ior > 1)
   {
      if (cost2 <= 0)
      {
         TIR = true;
         return 1.0f;
      }
      cosi = sqrt(cost2);
   }

   return lerp(pow(1 - cosi,5), 1, r0);
}

void FresnelReflectAmount_float (float ior, float3 normal, float3 incident, out float3 refracted, out float3 reflected, out float reflect_prob)
{
    float3 N = normalize(normal);
    float3 I = normalize(incident);
    float cosi = dot(I, N);
   
    reflected = reflect(I,N);
    
    float n1_over_n2;

    // when ray shoot through object back into vacuum,
    // ni_over_nt = ior, surface normal has to be inverted.
    if (cosi > 0)
    {
        n1_over_n2 = ior;
        N = -N;
    }
    // when ray shoots into object,
    // ni_over_nt = 1 / ior.
    else
    {
        n1_over_n2 = 1.0 / ior;
        cosi = -cosi;
    }

    float cost2 = 1.0 - n1_over_n2 * n1_over_n2 * (1 - cosi * cosi);

    bool TIR = false;
    reflect_prob = SchlickFresnel(cosi, cost2, n1_over_n2, TIR);
    refracted = TIR ? I : RefractVector(cosi, cost2, N, I, n1_over_n2);
}

// Use in underwater shader to read scene depth, and compare it with fluid depth to determine whether we should refract.
void Z2EyeDepth_float(float z, out float depth) 
{
    if (unity_OrthoParams.w < 0.5)
        depth = 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);//LinearEyeDepth(z); // Unity's LinearEyeDepth only works for perspective cameras.
    else{

        // since we're not using LinearEyeDepth in orthographic, we must reverse depth direction ourselves:
        #if UNITY_REVERSED_Z 
            z = 1-z;
        #endif

        depth = ((_ProjectionParams.z - _ProjectionParams.y) * z + _ProjectionParams.y);
    }
}

#endif
