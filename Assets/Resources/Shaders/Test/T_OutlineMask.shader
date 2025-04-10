Shader "Custom/Test/T_轮廓蒙版"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgeThreshold ("Edge Threshold", Range(0.01, 1.0)) = 0.1
        _ExpandPixels ("Expand Pixels", Range(0, 100)) = 5
        _Feather ("Feather", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Pass
        {
            Name "OUTLINE_MASK"
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _CameraDepthTexture;
            sampler2D _CameraColorTexture;
            float _EdgeThreshold;
            int _ExpandPixels;
            float _Feather;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float3 centerColor = tex2D(_CameraColorTexture, uv).rgb;
                float edge = 0.0;
                float stepSize = 1.0 / 1024.0; // assume 1024x1024, make param

                [unroll]
                for (int x = -1; x <= 1; x++)
                {
                    [unroll]
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 offset = float2(x, y) * stepSize * _ExpandPixels;
                        float3 sampleColor = tex2D(_CameraColorTexture, uv + offset).rgb;
                        edge += distance(centerColor, sampleColor);
                    }
                }
                edge = saturate(edge / 9.0 - _EdgeThreshold);
                return float4(edge, edge, edge, edge);
            }
            ENDHLSL
        }
    }
    FallBack Off
}