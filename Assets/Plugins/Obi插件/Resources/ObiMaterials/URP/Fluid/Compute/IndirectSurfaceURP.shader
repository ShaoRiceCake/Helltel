 Shader "Hidden/IndirectSurfaceURP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", 2D) = "white"
    }

    SubShader
    {
        Pass
        {
            Cull Back
            Lighting Off
            Fog { Mode Off }
            //ZTest LEqual
            ColorMask R

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"
           
            #pragma shader_feature MODE_2D

            struct v2f
            {
                float4 pos : SV_POSITION;
                float dist : TEXCOORD0;
                float4 projPos : TEXCOORD1;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            StructuredBuffer<float4> _Vertices;
            uniform float4x4 _ObjectToWorld;

            v2f vert(uint svVertexID: SV_VertexID, uint svInstanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                v2f o;

                unity_StereoEyeIndex = svInstanceID & 0x01;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                int vertexID = GetIndirectVertexID(svVertexID);
                float3 pos = _Vertices[vertexID].xyz;
                
                float4 vp = mul(UNITY_MATRIX_V,mul(_ObjectToWorld, float4(pos, 1.0f)));
                
                o.dist = -vp.z;

                o.pos = mul(UNITY_MATRIX_P, vp);

                o.projPos = ComputeScreenPos(o.pos);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return half4(i.dist,0,0,1);
            }
            ENDHLSL
        }

        Pass
        {
            Cull Front
            Lighting Off
            Fog { Mode Off }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"
           
            #pragma shader_feature MODE_2D

            struct v2f
            {
                float4 pos : SV_POSITION;
                float dist : TEXCOORD0;
                float4 projPos : TEXCOORD1;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            StructuredBuffer<float4> _Vertices;
            uniform float4x4 _ObjectToWorld;

            v2f vert(uint svVertexID: SV_VertexID, uint svInstanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                v2f o;

                unity_StereoEyeIndex = svInstanceID & 0x01;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                int vertexID = GetIndirectVertexID(svVertexID);
                float3 pos = _Vertices[vertexID].xyz;
                
                float4 vp = mul(UNITY_MATRIX_V,mul(_ObjectToWorld, float4(pos, 1.0f)));
                
                o.dist = -vp.z;

                o.pos = mul(UNITY_MATRIX_P, vp);

                o.projPos = ComputeScreenPos(o.pos);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                return half4(0,i.dist,0,1);
            }
            ENDHLSL
        }
    }
}