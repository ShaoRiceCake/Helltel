 Shader "Hidden/BurstSurface"
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
            ColorMask R

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature MODE_2D

            #include "UnityCG.cginc"

            struct appdata 
            {
                float4 pos : POSITION;
                float4 col : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 col : COLOR;
                float dist : TEXCOORD0;
                float4 projPos : TEXCOORD1; 
            };

            sampler2D _CameraDepthTexture;

            v2f vert(appdata v)
            {
                v2f o;

                float4 vp = mul(UNITY_MATRIX_MV, float4(v.pos.xyz, 1.0f));

                o.dist = -vp.z;

                o.pos = mul(UNITY_MATRIX_P, vp);

                o.projPos = ComputeScreenPos(o.pos);
                o.col = v.col;
                return o;
            }
           
            float4 frag(v2f i, fixed facing : VFACE) : SV_Target
            {
                return half4(i.dist,0,0,1);
            }
            ENDCG
        }

        Pass
        {
            Cull Front
            Lighting Off
            Fog { Mode Off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature MODE_2D

            #include "UnityCG.cginc"

            struct appdata 
            {
                float4 pos : POSITION;
                float4 col : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 col : COLOR;
                float dist : TEXCOORD0;
                float4 projPos : TEXCOORD1; 
            };

            sampler2D _CameraDepthTexture;

            v2f vert(appdata v)
            {
                v2f o;

                float4 vp = mul(UNITY_MATRIX_MV, float4(v.pos.xyz, 1.0f));

                o.dist = -vp.z;

                o.pos = mul(UNITY_MATRIX_P, vp);

                o.projPos = ComputeScreenPos(o.pos);
                o.col = v.col;
                return o;
            }
           
            float4 frag(v2f i, fixed facing : VFACE) : SV_Target
            {
                return half4(0,i.dist,0,1);
            }
            ENDCG
        }
    }
}