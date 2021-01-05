Shader "zero/Water/RefractionShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON  
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 depth: TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				UNITY_FOG_COORDS(3)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half _SeaLevel;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.depth = mul(unity_ObjectToWorld, v.vertex).yw;
				//o.depth = mul(unity_MatrixV * unity_ObjectToWorld, v.vertex).zw;
				//o.depth = o.vertex.zw;
				o.uv2 = half2(0,0);
#ifndef LIGHTMAP_OFF
				o.uv2 = v.uv2 * unity_LightmapST.xy + unity_LightmapST.zw;
#endif

				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float4 col = tex2D(_MainTex, i.uv);
				col.a = (i.depth.x / i.depth.y - _SeaLevel)*0.02f;
				//col.a = i.depth.x / i.depth.y;
				
#ifndef LIGHTMAP_OFF
				half3 lightmapCol = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv2));
				col.xyz *= lightmapCol;
#endif
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
