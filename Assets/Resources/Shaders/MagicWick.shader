Shader "Custom/MagicWick" {
    Properties {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseIntensity ("Base Intensity", Float) = 1.0
        _GlowColor ("Glow Color", Color) = (0.5,0.8,1,1)
        _GlowIntensity ("Glow Intensity", Float) = 2.0
        _NoiseScale ("Noise Scale", Float) = 1.0
        _NoiseSpeed ("Noise Speed", Float) = 1.0
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimThickness ("Rim Thickness", Range(0,1)) = 0.2
        _CenterOffset ("Center Offset", Vector) = (0,0,0,0)
        _DisplaceAmplitude ("Vertex Displacement Amplitude", Float) = 0.1
        _DisplaceFrequency ("Displacement Frequency", Float) = 1.0
        _DisplaceSpeed ("Displacement Speed", Float) = 1.0
    }

    SubShader {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 200

        Pass {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 worldPos   : TEXCOORD0;
                float3 worldNormal: TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _BaseIntensity;
                float4 _GlowColor;
                float _GlowIntensity;
                float _NoiseScale;
                float _NoiseSpeed;
                float4 _RimColor;
                float _RimThickness;
                float4 _CenterOffset;
                float _DisplaceAmplitude;
                float _DisplaceFrequency;
                float _DisplaceSpeed;
            CBUFFER_END

            // Simple hash noise
            float noise(float3 p) {
                return frac(sin(dot(p, float3(12.9898,78.233,37.719))) * 43758.5453);
            }

            // Fractional Brownian Motion
            float fbm(float3 p) {
                float v = 0.0;
                float amp = 1.0;
                float freq = 1.0;
                for (int i = 0; i < 4; ++i) {
                    v += amp * noise(p * freq);
                    freq *= 2.0;
                    amp  *= 0.5;
                }
                return v;
            }

            Varyings vert(Attributes v) {
                Varyings o;
                float3 posOS = v.positionOS.xyz;
                // Vertex displacement
                float3 worldBase = TransformObjectToWorld(posOS);
                float nDisp = noise(worldBase * _DisplaceFrequency + _Time.y * _DisplaceSpeed);
                posOS += v.normalOS * (nDisp * _DisplaceAmplitude);

                o.positionCS  = TransformObjectToHClip(posOS);
                o.worldPos    = TransformObjectToWorld(posOS);
                o.worldNormal = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            float4 frag(Varyings i) : SV_Target {
                // Base magic color
                float3 color = _BaseColor.rgb * _BaseIntensity;

                // Animated noise flicker
                float t = _Time.y * _NoiseSpeed;
                float n = fbm((i.worldPos + t) * _NoiseScale);

                // Glow from center
                float dist = distance(i.worldPos, _CenterOffset.xyz);
                float glow = saturate(1.0 - dist) * _GlowIntensity;

                // Rim lighting edge
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float rim = 1.0 - saturate(dot(i.worldNormal, viewDir));
                float rimFactor = smoothstep(1.0 - _RimThickness, 1.0, rim);

                // Combine effects
                color += _GlowColor.rgb * glow * n;
                color = lerp(color, _RimColor.rgb, rimFactor * n);

                return float4(color, _BaseColor.a);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Forward"
}