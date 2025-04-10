Shader "Custom/Test/T_特效Dissolve_Glitch_飘散"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "black" {}
        _TimeOffset ("Time Offset", Float) = 0
        _Direction ("Dissolve Direction", Vector) = (0,1,0,0)
        _MaxDistance ("Max Distance", Float) = 5.0
        _SpeedMin ("Speed Min", Float) = 0.1
        _SpeedMax ("Speed Max", Float) = 1.0
        _GlitchStrength ("Glitch Strength", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Name "DISSOLVE"
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"    

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _Direction;
            float _TimeOffset;
            float _MaxDistance;
            float _SpeedMin;
            float _SpeedMax;
            float _GlitchStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            float rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898,78.233))) * 43758.5453);
            }

            v2f vert(appdata v)
            {
                v2f o;
                // 将 float4 截断为 float3，避免类型隐式截断错误
                o.pos = TransformObjectToHClip(v.vertex.xyz); 
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float mask = tex2D(_MaskTex, i.uv).r;
                if (mask < 0.1) discard;

                float dissolveProgress = saturate(_TimeOffset / _MaxDistance);
                float2 offset = _Direction.xy * dissolveProgress;

                float rnd = rand(i.uv);
                float randomSpeed = lerp(_SpeedMin, _SpeedMax, rnd);
                offset *= randomSpeed;

                float2 glitchOffset = float2(rand(i.uv * 12.34), rand(i.uv * 56.78)) * _GlitchStrength * 0.01;
                float2 finalUV = i.uv + offset + glitchOffset;

                float4 col = tex2D(_MainTex, finalUV);
                col.rgb += glitchOffset.xyx * 0.2;
                col.a *= saturate(1.0 - dissolveProgress);
                return col;
            }
            ENDHLSL
        }
    }
}