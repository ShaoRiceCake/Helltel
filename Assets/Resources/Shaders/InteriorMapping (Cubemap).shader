Shader "Custom/InteriorMapping (Cubemap)"
{
    Properties
    {
        _CubeMap("Room Cubemap", Cube) = "white" {}
        _RoomWidth("Room Width",Float) = 1.0
        _RoomHeight("Room Height", Float) = 1.0
        _RoomDepth("Room Depth", Float) = 1.0
        [Toggle]_TilingMode("Enable Tiling Mode",Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Geometry"}

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile __ _TILINGMODE_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS      : NORMAL;
                float4 tangentOS     : TANGENT;
                float2 uv:TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv:TEXCOORD0;
                float3 viewTS : TEXCOORD1; 
                
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _CubeMap_ST;
            half _RoomWidth;
            half _RoomHeight;
            half _RoomDepth; 
            CBUFFER_END
            TEXTURECUBE(_CubeMap);SAMPLER(sampler_CubeMap);


            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(positionWS);

                OUT.uv = TRANSFORM_TEX(IN.uv, _CubeMap);

                float3 viewWS = GetWorldSpaceViewDir(positionWS);

                VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                float3x3 tangentSpaceTransform = float3x3(normalInput.tangentWS,normalInput.bitangentWS,normalInput.normalWS);
                OUT.viewTS = mul(tangentSpaceTransform, viewWS);
                //为了保证调整图片的tiling的时候效果正常，假设tiling.x为2那么需要水平方向放2个房间，相当于视角偏移了
                OUT.viewTS *= _CubeMap_ST.xyx;
                return OUT;
            }


            half4 SimpleCubeInterior(float2 uv,float3 viewTS)
            {
                uv = frac(uv);
                viewTS = - normalize(viewTS);
                uv = uv * 2.0 - 1.0;
	            float3 pos = float3(uv,1.0);

	            float3 id = 1.0 / viewTS;
	            float3 k = abs(id) - pos * id;
	            float kMin = min(min(k.x, k.y), k.z);
	            pos  += kMin * viewTS;

                //求出离开点后，我们把这个pos看做从原点出发的一条向量，然后将z轴翻转一下用于cubemap采样
                pos *= float3(1,1,-1);
                float4 roomColor = SAMPLE_TEXTURECUBE(_CubeMap, sampler_CubeMap,pos);


                return half4(roomColor.rgb, 1.0);
            }

            half4 CubeInterior(float2 uv,float3 viewTS,half roomWidth,half roomHeight,half roomDepth)
            {
                uv = frac(uv);
                viewTS = - normalize(viewTS);
          
                uv = uv * 2.0 - 1.0;
                float3 pos = float3(uv,roomDepth);
                
                float3 roomSize = float3(roomWidth,roomHeight,roomDepth);
                
                float3 id = 1.0 / viewTS;
	            float3 k = abs(id) * roomSize - pos * id;
	            float kMin = min(min(k.x, k.y), k.z);
	            pos += kMin * viewTS;
                
                #if defined(_TILINGMODE_ON)
                    //平铺模式
                    if(pos.y >= roomSize.y-0.001)
                    {
                        pos.xz = fmod(pos.xz + roomSize.xz,2) - 1;
                        pos.y /= roomSize.y;
                    }
                    else if(pos.y <= 0.001 - roomSize.y) //下
                    {
                        pos.xz = fmod(pos.xz + roomSize.xz,2) - 1;
                        pos.y /= roomSize.y;
                    }
                    else if(pos.x >= roomSize.x -0.001) //右
                    {
                        pos.yz = fmod(pos.yz + roomSize.yz,2) - 1;
                        pos.x /= roomSize.x;
                    }
                    else if(pos.x <= 0.001 - roomSize.x)//左
                    {
                        pos.yz = fmod(pos.yz + roomSize.yz,2) - 1;
                        pos.x /= roomSize.x;
                    }
                    else if(pos.z >= roomSize.z - 0.001) //前
                    {
                        pos.xy = fmod(pos.xy + roomSize.xy,2) - 1;
                        pos.z /= roomSize.z;
                    }
                    else if(pos.z <= 0.001 - roomSize.z)//后
                    {
                        pos.xy = fmod(pos.xy + roomSize.xy,2) - 1;
                        pos.z /= roomSize.z;
                    }
                #else
                    //拉伸模式
                    pos /= roomSize;
                #endif                

                pos *= float3(1.0,1.0,-1.0);
                float4 roomColor = SAMPLE_TEXTURECUBE(_CubeMap, sampler_CubeMap,pos);


                return half4(roomColor.rgb, 1.0);
            }


            half4 frag(Varyings IN) : SV_Target
            {
                
                //half4 roomColor = SimpleCubeInterior(IN.uv,IN.viewTS);

                half4 roomColor = CubeInterior(IN.uv,IN.viewTS,_RoomWidth,_RoomHeight,_RoomDepth);
                return roomColor;
            }

            ENDHLSL
        }
    }
}
