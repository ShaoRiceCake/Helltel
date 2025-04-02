Shader "Hidden/AccumulateTransmissionURP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        Blend DstColor Zero, One One

        Pass
        {

            HLSLPROGRAM
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            float _Thickness;

            half4 frag (Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 

                half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, i.texcoord);

                // clamp negative thicknesses to zero, this prevents artifacts when backfaces are in front.
                col.a = max(0, col.a); 
                
                float3 transmission = exp(- _Thickness * col.a * (1 - col.rgb));
                return float4(transmission, col.a);
            }
            ENDHLSL
        }
    }
}
