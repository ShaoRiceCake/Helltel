Shader "Custom/Mesh_OldTV"
{
    Properties
    {
        _MainTex ("主贴图", 2D) = "white" {}
        _PixelDensity ("像素密度", Float) = 64
        _PixelSampleOffset ("像素采样偏移", Vector) = (0.5, 0.5, 0, 0)
        _AspectRatio ("宽高比", Vector) = (1.0, 1.0, 0, 0)

        _OverlayTex ("叠加贴图", 2D) = "black" {}
        _OverlayAlpha ("叠加透明度", Range(0,1)) = 0.3

        _OldTVTintColor ("老电视色调", Color) = (0.0, 0.2, 0.0, 1.0)
        _TintStrength ("色调强度", Range(0,1)) = 0.2

        _ScanLineStrength ("扫描线强度", Range(0,1)) = 1
        _ScanLineSpeed ("扫描线速度", Float) = 4
        _ScanLineWidth ("扫描线宽度", Float) = 12
        _ScanLineSpacing ("扫描线间隔", Float) = 200

        _FishEyeCenterScale ("鱼眼中心缩放因子", Range(0,1)) = 0.7

        _VignetteStrength ("暗角强度", Range(0, 1)) = 1
        _VignetteColor ("暗角颜色", Color) = (0, 0, 0, 1)
        _VignetteSoftness ("暗角柔和度", Range(0,1)) = 0.55

        _NoiseStrength ("噪声抖动强度", Float) = 0.15

        _ChromAbStrength ("色散强度", Range(0,1)) = 0.2
        _ChromAbOffset ("色散偏移量", Float) = 0.005
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 200
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "MeshPass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _OverlayTex;
            float _OverlayAlpha;

            float4 _OldTVTintColor;
            float _TintStrength;

            float _ScanLineStrength;
            float _ScanLineSpeed;
            float _ScanLineWidth;
            float _ScanLineSpacing;

            float _PixelDensity;
            float2 _AspectRatio;
            float4 _PixelSampleOffset;

            float _FishEyeCenterScale;

            float _VignetteStrength;
            float _VignetteSoftness;
            float4 _VignetteColor;

            float _NoiseStrength;

            float _ChromAbStrength;
            float _ChromAbOffset;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float2 FishEyeDistort(float2 uv, float centerScale)
            {
                float2 center = float2(0.5, 0.5);
                float2 delta = uv - center;
                float r = length(delta);
                float maxR = length(center);
                float factor = lerp(centerScale, 1.0, smoothstep(0.0, maxR, r));
                return center + delta * factor;
            }

            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898,78.233))) * 43758.5453);
            }

            float3 ApplyVignette(float2 uv, float3 rgb, float strength, float softness, float4 vignetteColor)
            {
                float2 center = float2(0.5, 0.5);
                float len = length(uv - center);
                float edge0 = 0.8;
                float edge1 = edge0 - strength * softness;
                float mask = smoothstep(edge0, edge1, len);
                return lerp(vignetteColor.rgb, rgb, mask);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 pixelSize = float2(1.0 / _PixelDensity, 1.0 / _PixelDensity) * _AspectRatio;
                float2 pixelUV = floor(IN.uv / pixelSize) * pixelSize + pixelSize * _PixelSampleOffset.xy;
                float2 distortedUV = FishEyeDistort(pixelUV, _FishEyeCenterScale);
                float noiseX = rand(distortedUV + _Time.y);
                float noiseY = rand(distortedUV + _Time.y + 42.0);
                float2 jitter = (_NoiseStrength * 0.01) * (float2(noiseX, noiseY) - 0.5);
                float2 finalUV = distortedUV + jitter;

                float4 col = tex2D(_MainTex, finalUV);
                col.rgb = lerp(col.rgb, _OldTVTintColor.rgb, _TintStrength);

                float scan = sin((_Time.y * _ScanLineSpeed + finalUV.y * _ScanLineSpacing)) * 0.5 + 0.5;
                float stripe = smoothstep(0.5 - _ScanLineWidth * 0.5, 0.5 + _ScanLineWidth * 0.5, scan);
                col.rgb *= lerp(1.0, stripe, _ScanLineStrength);

                float4 overlay = tex2D(_OverlayTex, finalUV);
                float overlayWeight = overlay.a * _OverlayAlpha;
                col.rgb = lerp(col.rgb, overlay.rgb, overlayWeight);

                float2 abOffset = float2(_ChromAbOffset, 0);
                float r = tex2D(_MainTex, finalUV + abOffset).r;
                float b = tex2D(_MainTex, finalUV - abOffset).b;
                float3 chromaSample = float3(r, col.g, b);
                col.rgb = lerp(col.rgb, chromaSample, _ChromAbStrength);

                col.rgb = ApplyVignette(IN.uv, col.rgb, _VignetteStrength, _VignetteSoftness, _VignetteColor);
                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Transparent/Diffuse"
}
