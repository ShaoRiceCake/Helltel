#ifndef OBILIGHTINGBUILTURP_INCLUDED
#define OBILIGHTINGBUILTURP_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
    #undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
#endif

//https://learnopengl.com/PBR/Theory
float DistributionGGX(float nh, float a)
{
    float a2     = a*a;
    float NdotH2 = nh * nh;
    
    float nom    = a2;
    float denom  = (NdotH2 * (a2 - 1) + 1);
    denom        = 3.1415 * denom * denom;
    
    return nom / denom;
}

float GeometrySchlickGGX(float NdotV, float k)
{
    return 1 / ( NdotV * (1.0 - k) + k);
}
  
float GeometrySmith(float nv, float nl, float k)
{
    float ggx1 = GeometrySchlickGGX(nv, k);
    float ggx2 = GeometrySchlickGGX(nl, k);
    
    return ggx1 * ggx2;
}

float Schlick2(float cosine, float r0) 
{
    return lerp(r0,1,pow(1 - cosine,5));
}

void MainLight_float(float3 WorldPos, out float3 Direction, out float3 color, out float DistanceAtten, out float ShadowAtten)
{
#ifdef SHADERGRAPH_PREVIEW
   Direction = float3(0.5, 0.5, 0);
   color = 1;
   DistanceAtten = 1;
   ShadowAtten = 1;
#else
    #ifdef UNIVERSAL_LIGHTING_INCLUDED

        #if SHADOWS_SCREEN
           half4 clipPos = TransformWorldToHClip(WorldPos);
           half4 shadowCoord = ComputeScreenPos(clipPos);
        #else
           half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
        #endif
           Light mainLight = GetMainLight(shadowCoord);
           Direction = mainLight.direction;
           color = mainLight.color;

           // attentuation should only be zero if light culling mask doesn't intersect 
           // render params layer, but for some reason it's always zero on newer URP versions :(.
           DistanceAtten = 1;//mainLight.distanceAttenuation;
           ShadowAtten = mainLight.shadowAttenuation;
    #else
       Direction = _WorldSpaceLightPos0;
       color = _LightColor0;
       DistanceAtten = 1;
       ShadowAtten = 1;
    #endif
#endif
}

void Ambient_float(float3 WorldPos, float3 WorldNormal, out float3 ambient)
{
#ifdef SHADERGRAPH_PREVIEW
ambient = float3(0,0,0);
#else
    #ifdef UNIVERSAL_LIGHTING_INCLUDED
        // Samples spherical harmonics, which encode light probe data
        float3 vertexSH;
        OUTPUT_SH(WorldNormal, vertexSH);

        float2 lightmapUV = float2(0,0);
        // This function calculates the final baked lighting from light maps or probes
        ambient = SAMPLE_GI(lightmapUV, vertexSH, WorldNormal);
    #else
        #if UNITY_SHOULD_SAMPLE_SH
        ambient = ShadeSHPerPixel(float4(WorldNormal, 1.0),float3(0,0,0),WorldPos);
        #else
        ambient = UNITY_LIGHTMODEL_AMBIENT;
        #endif

    #endif
#endif
}


void DirectScatter_float(float3 WorldNormal, float3 WorldView, float3 Direction, float3 color, float radius, float thickness, float subsurfaceAmount, out float3 diffuse)
{
#ifdef SHADERGRAPH_PREVIEW
   diffuse = float3(0,0,0);
#else

    float dotLN = dot(Direction, WorldNormal);

    float3 vLTLight = Direction + WorldNormal * (1 - radius);
    float3 volumeScatter = (pow(saturate(dot(WorldView, -vLTLight)),2) + 0.2) * thickness;
   
    //https://www.cim.mcgill.ca/~derek/files/jgt_wrap.pdf
    float theta_m = acos(-radius); // boundary of the lighting function
    float theta = max(0, dotLN + radius) - radius;
    float normalization_jgt = (2 + radius) / (2 * (1 + radius));
    float subsurfaceScatter = (pow(((theta + radius) / (1 + radius)), 1 + radius)) * normalization_jgt * subsurfaceAmount;
    
    diffuse = color * (subsurfaceScatter * (1 - volumeScatter) + volumeScatter);

#endif
}

void DirectSpecular_float(float3 specular, float Smoothness, float IOR, float3 Direction, float3 color, float3 WorldNormal, float3 WorldView, out float3 Out)
{
#if SHADERGRAPH_PREVIEW
   Out = 0;
#else
   WorldNormal = normalize(WorldNormal);
   WorldView = SafeNormalize(WorldView);
   float3 WorldHalf = normalize(WorldView + Direction);

   float nv = saturate(dot(WorldNormal,WorldView));
   float nl = saturate(dot(WorldNormal,Direction));
   float nh = saturate(dot(WorldNormal,WorldHalf));
   float vh = saturate(dot(WorldView,WorldHalf));

   float perceptualRoughness = 1 - Smoothness;
   float alpha = max(perceptualRoughness * perceptualRoughness, 0.002);

   float D = DistributionGGX(nh, alpha);
   float G = GeometrySmith(nv, nl, pow(alpha + 1,2)/8.0 );

   float r0s = (IOR - 1) / (IOR + 1);
   float F = Schlick2(vh, r0s * r0s);

    // nl and nv terms in the denominator cancel out with G term's.
   float spec = D * F * G / 4 * nl;

   // gamma
   #ifdef UNITY_COLORSPACE_GAMMA
   spec = pow (spec, 1/2.2f);
   #endif
  
   Out = color * spec;
#endif
}

/*#ifndef UNIVERSAL_LIGHTING_INCLUDED
    sampler2D _CameraDepthTexture;

    void SceneEyeDepth_float(float4 clipPos, out float depth) 
    {
        float z = tex2D(_CameraDepthTexture,clipPos).r;

        if (unity_OrthoParams.w < 0.5)
            depth = 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w); //LinearEyeDepth only works for perspective cameras.
        else{

            // since we're not using LinearEyeDepth in orthographic, we must reverse depth direction ourselves:
            #if UNITY_REVERSED_Z 
                z = 1-z;
            #endif

            depth = ((_ProjectionParams.z - _ProjectionParams.y) * z + _ProjectionParams.y);
        }
    }
#endif*/

#endif
