Shader "Hidden/Outline"
{
    Properties
    {
        _OutlineColor("外轮廓颜色", Color) = (1,1,1,1)
        _InnerFlickerColor("内填充颜色", Color) = (1,1,1,1)
        _OutlineThickness("外轮廓粗细", Range(0,0.01)) = 0.0005
        _MaskThreshold("遮罩阈值（调试用）", Range(0,1)) = 0.5

        // 闪烁控制（统一用于外轮廓和内部区域）
        _FlickerSpeed("闪烁速度", Float) = 3.0
        _FlickerIntensity("闪烁强度", Range(0,1)) = 1.0

    }

    SubShader
    {
        Tags {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
        }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct Attributes { uint vertexID : SV_VertexID; };
            struct Varyings {
                float4 positionCS : SV_POSITION;
                half2 uv : TEXCOORD0;
            };

            TEXTURE2D_X(_OutlineMask);
            SAMPLER(sampler_linear_clamp_OutlineMask);

            float4 _OutlineColor;
            float  _OutlineThickness;
            float  _MaskThreshold;

            float  _FlickerSpeed;
            float  _FlickerIntensity;
            float4 _InnerFlickerColor;

            Varyings vert(Attributes IN)
            {
                Varyings o;
                o.positionCS = GetFullScreenTriangleVertexPosition(IN.vertexID);
                o.uv         = GetFullScreenTriangleTexCoord(IN.vertexID);
                return o;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // 根据屏幕尺寸计算像素偏移，并压缩X方向（缩小横向采样范围）
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 pixelOffset = float2(_OutlineThickness * _ScreenParams.z / aspect, _OutlineThickness * _ScreenParams.w);

                // 对角方向偏移
                float2 d = pixelOffset * 0.70710678;

                half4 center = SAMPLE_TEXTURE2D_X(_OutlineMask, sampler_linear_clamp_OutlineMask, uv);
                half  centerMask = center.a;

                bool isOutline = (centerMask <= _MaskThreshold) && (
                    SAMPLE_TEXTURE2D_X(_OutlineMask, sampler_linear_clamp_OutlineMask, uv + float2(-pixelOffset.x, 0)).a > _MaskThreshold ||
                    SAMPLE_TEXTURE2D_X(_OutlineMask, sampler_linear_clamp_OutlineMask, uv + float2( pixelOffset.x, 0)).a > _MaskThreshold ||
                    SAMPLE_TEXTURE2D_X(_OutlineMask, sampler_linear_clamp_OutlineMask, uv + float2(0, -pixelOffset.y)).a > _MaskThreshold ||
                    SAMPLE_TEXTURE2D_X(_OutlineMask, sampler_linear_clamp_OutlineMask, uv + float2(0,  pixelOffset.y)).a > _MaskThreshold ||
                    SAMPLE_TEXTURE2D_X(_OutlineMask, sampler_linear_clamp_OutlineMask, uv + float2(-d.x, -d.y)).a > _MaskThreshold ||
                    SAMPLE_TEXTURE2D_X(_OutlineMask, sampler_linear_clamp_OutlineMask, uv + float2(-d.x,  d.y)).a > _MaskThreshold ||
                    SAMPLE_TEXTURE2D_X(_OutlineMask, sampler_linear_clamp_OutlineMask, uv + float2( d.x, -d.y)).a > _MaskThreshold ||
                    SAMPLE_TEXTURE2D_X(_OutlineMask, sampler_linear_clamp_OutlineMask, uv + float2( d.x,  d.y)).a > _MaskThreshold
                );

                float t = _Time.y;
                float flicker = abs(sin(t * _FlickerSpeed)) * _FlickerIntensity;

                if (isOutline)
                {
                    return half4(_OutlineColor.rgb, _OutlineColor.a * flicker);
                }
                else if (centerMask > _MaskThreshold)
                {
                    half3 baseCol = center.rgb;
                    half3 flicked = lerp(baseCol, _InnerFlickerColor.rgb, flicker);
                    return half4(flicked, center.a);
                }
                else
                {
                    return center;
                }
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
