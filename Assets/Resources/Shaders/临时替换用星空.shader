Shader "Custom/FancyStarrySky"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _Speed ("Rotation Speed", Float) = 1.0
        _StarDensity ("Star Density", Range(0, 1)) = 0.5
        _TwirlStrength ("Twirl Strength", Range(0, 1)) = 0.3
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Speed;
                half _StarDensity;
                half _TwirlStrength;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Center of the screen
                float2 center = float2(0.5, 0.5);

                // Calculate time-based rotation
                float angle = _Time.y * _Speed;
                float s = sin(angle);
                float c = cos(angle);

                // Rotate UV coordinates around the center
                float2x2 rotMatrix = float2x2(c, -s, s, c);
                float2 rotatedUV = mul(rotMatrix, input.uv - center) + center;

                // Twirl effect
                float2 offset = rotatedUV - center;
                float dist = length(offset);
                float theta = atan2(offset.y, offset.x);
                theta += _TwirlStrength * sin(dist * 10.0);
                float2 twirledUV = center + float2(cos(theta), sin(theta)) * dist;

                // Sample texture and apply color tint
                half4 col = tex2D(_MainTex, twirledUV) * _Color;

                // Add star noise based on density
                float starNoise = frac(sin(dot(twirledUV.xy ,float2(12.9898,78.233))) * 43758.5453);
                if (starNoise > (1.0 - _StarDensity))
                {
                    col.rgb += float3(1, 1, 1) * saturate(starNoise * 10.0);
                }

                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}



