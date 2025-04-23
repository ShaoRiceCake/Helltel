Shader "Hidden/OutlineComposite"
{
    Properties
    {
        _OutlineEdges("Edges", 2D) = "white" {}
        _MainTex("Camera Color", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _OutlineEdges;

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 edge = tex2D(_OutlineEdges, i.uv);
                return lerp(col, fixed4(1, 0, 0, 1), edge.r); // 红色轮廓
            }
            ENDHLSL
        }
    }
}
