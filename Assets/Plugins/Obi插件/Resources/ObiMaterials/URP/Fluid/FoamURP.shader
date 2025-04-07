Shader "Obi/FoamURP" {

Properties {

    _RadiusScale("Radius scale",float) = 1
}

    SubShader { 

        Tags {"Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Blend One OneMinusSrcAlpha  
        ZWrite Off

        Pass { 
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../../Common/ObiEllipsoids.cginc"
            #include "../../Common/ObiUtils.cginc"

            struct vin
            {
                float4 vertex   : POSITION;
                float3 corner   : NORMAL;
                half4 color    : COLOR;
                float4 velocity : TEXCOORD0; 
                float4 attributes : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4 pos   : SV_POSITION;
                half4 color    : COLOR;
                float4 mapping  : TEXCOORD0;
                float4 viewRay : TEXCOORD1;
                float3 a2 : TEXCOORD2;
                float3 a3 : TEXCOORD3;
                float4 screenPos: TEXCOORD4;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D_X(_TemporaryBuffer);
            SAMPLER(sampler_TemporaryBuffer);

            TEXTURE2D_X(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            float _FadeDepth;
            float _FadeIn;
            float _FadeOut;
            float _VelocityStretching;

            v2f vert(vin v)
            { 
                float4 worldVertex = mul(unity_ObjectToWorld,v.vertex);

                float3x3 P, IP;
                float4 t0,t1,t2;
                BuildVelocityStretchedBasis(v.velocity.xyz, _VelocityStretching, v.attributes.z, t0, t1, t2);
                BuildParameterSpaceMatrices(t0,t1,t2,P,IP);
            
                float3 worldPos;
                float3 view;
                float3 eye;
                float radius = BuildEllipsoidBillboard(worldVertex,v.corner,P,IP,worldPos,view,eye);
            
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert

                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos,v.vertex.w));
                o.mapping = float4(v.corner.xy,1/length(eye),radius);                    // A[1]
                o.viewRay = float4(mul((float3x3)UNITY_MATRIX_V,view), v.attributes.z * 2);  // A[0]
                o.color = v.color;

                float fadeIn =  1 - saturate(v.attributes.x - (1 - _FadeIn)) / _FadeIn;
                float fadeOut = 1 - saturate((1 - _FadeOut) - v.attributes.x) / (1 - _FadeOut);
                o.color.a *= min(fadeIn,fadeOut);
            
                BuildAuxiliaryNormalVectors(worldVertex,worldPos,view,P,IP,o.a2,o.a3);

                o.screenPos = ComputeScreenPos(o.pos);

                return o;
            }

            float Z2EyeDepth(float z) 
            {
                if (unity_OrthoParams.w < 0.5)
                    return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w); // Unity's LinearEyeDepth only works for perspective cameras.
                else{

                    // since we're not using LinearEyeDepth in orthographic, we must reverse depth direction ourselves:
                    #if UNITY_REVERSED_Z 
                        z = 1-z;
                    #endif

                    return ((_ProjectionParams.z - _ProjectionParams.y) * z + _ProjectionParams.y);
                }
            }

            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float4 color = i.color;

                // generate sphere normals:
                float3 p,n;
                float thickness = IntersectEllipsoid(i.viewRay.xyz,i.mapping, i.a2, i.a3, p, n);
                float shapeFalloff = thickness * 0.5f / i.mapping.w;

                //get depth from depth texture
                float sceneDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, i.screenPos.xy/i.screenPos.w).r;
                float eyeDepth = Z2EyeDepth(sceneDepth);
                
                if (i.screenPos.w > eyeDepth) discard;
                
                float2 fluidSurface = SAMPLE_TEXTURE2D_X(_TemporaryBuffer, sampler_TemporaryBuffer, i.screenPos.xy /  i.screenPos.w).xy;
                float distanceToSurface = -fluidSurface.x + i.screenPos.w;
                float distanceToBack = -fluidSurface.y + i.screenPos.w;

                if (_FadeDepth > 0.0001f)
                {
                    // discard foam fragments that are closer than the front surface, if the front surface is closer than the back. 
                    if (distanceToSurface < 0 && distanceToBack < distanceToSurface)
                        discard;
                    color.a *= lerp(shapeFalloff, pow(1 - shapeFalloff,2), saturate((distanceToSurface - i.viewRay.w)/i.viewRay.w)); 
                    color.a *= 1 - saturate(distanceToSurface/_FadeDepth);
                }
                else
                {
                    color.a *= shapeFalloff;
                }

                // premultiply:
                color.rgb *= color.a;

                return color;
            }
             
            ENDHLSL

        } 
       
    } 
FallBack "Diffuse"
}

