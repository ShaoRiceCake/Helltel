Shader "Custom/SpindleDeformation"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _Height("Cylinder Height", Float) = 2.0
        _Radius("Cylinder Radius", Float) = 0.5
        _DeformAmount("Deformation Amount", Range(0, 1)) = 0.0
        _DeformCurve("Deformation Curve", Range(1, 10)) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Height;
                float _Radius;
                float _DeformAmount;
                float _DeformCurve;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                // 获取顶点在圆柱体上的高度比例 (-0.5到0.5)
                float heightRatio = IN.positionOS.y / _Height;
                
                // 计算变形因子 - 使用高度比例的非线性函数
                float deformFactor = pow(abs(heightRatio * 2.0), _DeformCurve);
                
                // 应用变形 - 在顶部和底部收缩，中间保持原状
                float3 deformedPosition = IN.positionOS.xyz;
                deformedPosition.xz *= 1.0 - (deformFactor * _DeformAmount);
                
                // 转换到世界空间和裁剪空间
                float3 positionWS = TransformObjectToWorld(deformedPosition);
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                
                // 处理法线 (简化处理，实际可能需要更精确的法线计算)
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 简单的光照计算
                float3 lightDir = normalize(float3(1, 1, 1));
                float NdotL = saturate(dot(IN.normalWS, lightDir));
                float3 diffuse = _BaseColor.rgb * NdotL;
                
                return half4(diffuse, _BaseColor.a);
            }
            ENDHLSL
        }
    }
}