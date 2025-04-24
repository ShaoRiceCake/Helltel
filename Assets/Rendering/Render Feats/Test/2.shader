Shader "Hidden/OutlineSobel"
{
    Properties { _MainTex ("Base (Mask)", 2D) = "white" {} }
    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            ZTest Always Cull Off ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float2 _MainTex_TexelSize;

            fixed4 frag(v2f_img i) : SV_Target {
                float2 uv = i.uv;
                float3 tl = tex2D(_MainTex, uv + float2(-_MainTex_TexelSize.x,  _MainTex_TexelSize.y)).rgb;
                float3 t  = tex2D(_MainTex, uv + float2(0,   _MainTex_TexelSize.y)).rgb;
                float3 tr = tex2D(_MainTex, uv + float2(_MainTex_TexelSize.x,  _MainTex_TexelSize.y)).rgb;
                float3 l  = tex2D(_MainTex, uv + float2(-_MainTex_TexelSize.x, 0)).rgb;
                float3 r  = tex2D(_MainTex, uv + float2(_MainTex_TexelSize.x, 0)).rgb;
                float3 bl = tex2D(_MainTex, uv + float2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y)).rgb;
                float3 b  = tex2D(_MainTex, uv + float2(0,  -_MainTex_TexelSize.y)).rgb;
                float3 br = tex2D(_MainTex, uv + float2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y)).rgb;

                float3 gx = -tl - 2*l - bl + tr + 2*r + br;
                float3 gy = -tl - 2*t - tr + bl + 2*b + br;
                float3 edge = sqrt(gx * gx + gy * gy);
                return fixed4(edge, 1);
            }
            ENDHLSL
        }
    }
}
