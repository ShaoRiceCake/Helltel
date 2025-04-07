 Shader "Hidden/IndirectThickness"
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
            Cull Off
            Blend One One
            BlendOp Min, Add // color is blended picking darkest, depth additively.
            Lighting Off
            ZWrite Off
            ZTest Always
            Fog { Mode Off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"
           
            #pragma shader_feature MODE_2D

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 col : COLOR;
                float dist : TEXCOORD0;
                float4 projPos : TEXCOORD1;
            };

            StructuredBuffer<float4> _Vertices;
            StructuredBuffer<float4> _Colors;
            uniform float4x4 _ObjectToWorld;

            v2f vert(uint svVertexID: SV_VertexID, uint svInstanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                v2f o;
                
                int vertexID = GetIndirectVertexID(svVertexID);
                float3 pos = _Vertices[vertexID].xyz;
                
                float4 vp = mul(UNITY_MATRIX_V,mul(_ObjectToWorld, float4(pos, 1.0f)));

                #if MODE_2D
                    o.dist = pos.z;
                #else
                    o.dist = -vp.z;
                #endif

                o.pos = mul(UNITY_MATRIX_P, vp);

                o.projPos = ComputeScreenPos(o.pos);
                o.col = _Colors[vertexID];
                return o;
            }

            sampler2D _CameraDepthTexture;

            float Z2EyeDepth(float z) 
            {
                if (unity_OrthoParams.w < 0.5)
                    return LinearEyeDepth(z); // Unity's LinearEyeDepth only works for perspective cameras.
                else{

                    // since we're not using LinearEyeDepth in orthographic, we must reverse depth direction ourselves:
                    #if UNITY_REVERSED_Z 
                        z = 1-z;
                    #endif

                    return ((_ProjectionParams.z - _ProjectionParams.y) * z + _ProjectionParams.y);
                }
            }

            float4 frag(v2f i, fixed facing : VFACE) : SV_Target
            {
                //get depth from depth texture
                float sceneDepth = tex2Dproj(_CameraDepthTexture,i.projPos).r;

                //linear depth between camera and far clipping plane
                sceneDepth = Z2EyeDepth(sceneDepth);

                float depth = -sign(facing) * min(sceneDepth,i.dist);

                // blend color multiplicatively.
                // blend depth of both front and back faces additively, to calculate thickness.
                #if MODE_2D
                    return half4(facing > 0 ? i.col.rgb : float3(1,1,1), depth);
                #else 
                    return half4(facing < 0 ? i.col.rgb : float3(1,1,1), depth);
                #endif 
            }
            ENDCG
        }
    }
}