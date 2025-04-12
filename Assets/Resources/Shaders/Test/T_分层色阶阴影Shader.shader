Shader "Custom/Test/T_分层色阶阴影Shader"
{
    Properties
    {
        [MainTexture]_MainTex("主纹理", 2D) = "white" {}
        _UVScale("UV缩放（世界坐标）", Vector) = (1, 1, 0, 0)

        [Toggle(_IS_SOLID_COLOR)] _IsSolidColor("是否使用纯色", Float) = 0
        _SolidColor("纯色颜色", Color) = (1, 1, 1, 1)

        _ColorLevelCount("贴图灰度色阶数量", Range(2, 100)) = 3

        _LightingColorLevelCount("光照色阶数量", Range(2, 30)) = 3
        _NoiseStrength("噪波扰动强度", Range(0, 1)) = 0.3
        _NoiseEdgeFade("边缘扰动范围", Range(0, 1)) = 0.5
        _NoiseTex("噪波贴图", 2D) = "white" {}

        _HeightMap("高度贴图", 2D) = "white" {}
        _MetallicMap("金属度贴图", 2D) = "white" {}
        _MixedAOMap("环境光遮蔽混合贴图", 2D) = "white" {}
        _NormalMap("法线贴图", 2D) = "bump" {}
        _RoughnessMap("粗糙度贴图", 2D) = "white" {}

        _MaxLightIntensity("接受场景亮度上限", Range(0, 10)) = 6.98 // 新增属性
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float3 worldTangent : TEXCOORD3;
                float3 worldBitangent : TEXCOORD4;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);
            TEXTURE2D(_HeightMap);
            SAMPLER(sampler_HeightMap);
            TEXTURE2D(_MetallicMap);
            SAMPLER(sampler_MetallicMap);
            TEXTURE2D(_MixedAOMap);
            SAMPLER(sampler_MixedAOMap);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_RoughnessMap);
            SAMPLER(sampler_RoughnessMap);

            CBUFFER_START(UnityPerMaterial)
            float4 _SolidColor;
            float4 _UVScale;
            float _ColorLevelCount;
            float _LightingColorLevelCount;
            float _NoiseStrength;
            float _NoiseEdgeFade;
            float _IsSolidColor;
            float _MaxLightIntensity; // 新增变量
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.worldNormal = TransformObjectToWorldNormal(input.normalOS);
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                output.worldTangent = TransformObjectToWorldDir(input.tangentOS.xyz);
                output.worldBitangent = cross(output.worldNormal, output.worldTangent) * input.tangentOS.w;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 finalColor;

                if (_IsSolidColor > 0.5)
                {
                    finalColor = _SolidColor;
                }
                else
                {
                    half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                    float interval = 1.0 / _ColorLevelCount;
                    // 分别对 RGB 通道进行色阶分离
                    float rLevel = floor(texColor.r / interval);
                    float quantizedR = (rLevel + 0.5) * interval;
                    float gLevel = floor(texColor.g / interval);
                    float quantizedG = (gLevel + 0.5) * interval;
                    float bLevel = floor(texColor.b / interval);
                    float quantizedB = (bLevel + 0.5) * interval;
                    finalColor = float4(quantizedR, quantizedG, quantizedB, texColor.a);
                }

                // 读取 PBR 贴图
                float height = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, input.uv).r;
                float metallic = SAMPLE_TEXTURE2D(_MetallicMap, sampler_MetallicMap, input.uv).r;
                float mixedAO = SAMPLE_TEXTURE2D(_MixedAOMap, sampler_MixedAOMap, input.uv).r;
                float3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));
                float roughness = SAMPLE_TEXTURE2D(_RoughnessMap, sampler_RoughnessMap, input.uv).r;

                // 对 PBR 贴图进行色阶分离
                float heightInterval = 1.0 / _ColorLevelCount;
                float heightLevel = floor(height / heightInterval);
                float quantizedHeight = (heightLevel + 0.5) * heightInterval;

                float metallicInterval = 1.0 / _ColorLevelCount;
                float metallicLevel = floor(metallic / metallicInterval);
                float quantizedMetallic = (metallicLevel + 0.5) * metallicInterval;

                float mixedAOInterval = 1.0 / _ColorLevelCount;
                float mixedAOLevel = floor(mixedAO / mixedAOInterval);
                float quantizedMixedAO = (mixedAOLevel + 0.5) * mixedAOInterval;

                float roughnessInterval = 1.0 / _ColorLevelCount;
                float roughnessLevel = floor(roughness / roughnessInterval);
                float quantizedRoughness = (roughnessLevel + 0.5) * roughnessInterval;

                // 对法线贴图三个通道进行色阶分离
                float normalInterval = 1.0 / _ColorLevelCount;
                float normalRLevel = floor(normalTS.r / normalInterval);
                float quantizedNormalR = (normalRLevel + 0.5) * normalInterval;
                float normalGLevel = floor(normalTS.g / normalInterval);
                float quantizedNormalG = (normalGLevel + 0.5) * normalInterval;
                float normalBLevel = floor(normalTS.b / normalInterval);
                float quantizedNormalB = (normalBLevel + 0.5) * normalInterval;
                float3 quantizedNormalTS = float3(quantizedNormalR, quantizedNormalG, quantizedNormalB);

                // 光照色阶计算
                float3 normal = normalize(input.worldNormal);
                float totalLighting = 0;

                // 主光源计算
                Light mainLight = GetMainLight();
                float ndotlMain = saturate(dot(normal, normalize(mainLight.direction)));
                float3 lightColMain = mainLight.color.rgb;
                float lightIntensityMain = ndotlMain * dot(lightColMain, float3(0.299, 0.587, 0.114));
                lightIntensityMain = min(lightIntensityMain, _MaxLightIntensity); // 限制光照强度

                float lightInterval = 1.0 / _LightingColorLevelCount;
                float lightLevelMain = floor(lightIntensityMain / lightInterval);
                float levelCenterMain = (lightLevelMain + 0.5) * lightInterval;

                // 世界坐标采样噪波贴图
                float2 worldUV = input.worldPos.xz * _UVScale.xy;
                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, worldUV).r;

                float distanceToCenterMain = abs(lightIntensityMain - levelCenterMain);
                float edgeThreshold = lightInterval * _NoiseEdgeFade;

                float lightingResultMain = levelCenterMain;
                if (distanceToCenterMain < edgeThreshold)
                {
                    float offset = (noise - 0.5) * _NoiseStrength * lightInterval;
                    lightingResultMain = clamp(levelCenterMain + offset, 0, 1);
                }

                totalLighting += lightingResultMain;

                // 额外光源计算
                uint additionalLightsCount = GetAdditionalLightsCount();
                for (uint i = 0; i < additionalLightsCount; ++i)
                {
                    Light additionalLight = GetAdditionalLight(i, input.worldPos);
                    float ndotlAdditional = saturate(dot(normal, normalize(additionalLight.direction)));
                    float3 lightColAdditional = additionalLight.color.rgb;
                    float lightIntensityAdditional = ndotlAdditional * dot(lightColAdditional, float3(0.299, 0.587, 0.114));
                    lightIntensityAdditional = min(lightIntensityAdditional, _MaxLightIntensity); // 限制光照强度

                    float lightLevelAdditional = floor(lightIntensityAdditional / lightInterval);
                    float levelCenterAdditional = (lightLevelAdditional + 0.5) * lightInterval;

                    float distanceToCenterAdditional = abs(lightIntensityAdditional - levelCenterAdditional);

                    float lightingResultAdditional = levelCenterAdditional;
                    if (distanceToCenterAdditional < edgeThreshold)
                    {
                        float offset = (noise - 0.5) * _NoiseStrength * lightInterval;
                        lightingResultAdditional = clamp(levelCenterAdditional + offset, 0, 1);
                    }

                    totalLighting += lightingResultAdditional;
                }

                finalColor.rgb *= totalLighting * quantizedMixedAO;

                return finalColor;
            }
            ENDHLSL
        }
    }

    FallBack "Diffuse"
}