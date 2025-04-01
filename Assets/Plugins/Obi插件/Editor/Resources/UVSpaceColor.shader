Shader "Obi/UVSpaceColor"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
                fixed4 color    : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2_f
			{
				float2 uv : TEXCOORD0;
                fixed4 color    : COLOR;
				float4 vertex : SV_POSITION;
			};
			
			v2_f vert (appdata v)
			{
				v2_f o;
                o.uv = v.uv;
                o.vertex = float4(v.uv.xy,0,1);
				o.vertex = UnityObjectToClipPos(float4(v.uv,0,1));
                o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2_f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}
}
