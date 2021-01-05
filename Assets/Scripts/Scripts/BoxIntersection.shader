// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/BoxIntersection"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_BoxSize("BoxSize", vector) = (1, 1, 1, 1)
		_Kd("water scattering kd", Vector) = (.3867, .1055, .0469, 0)
		_Attenuation("_Attenuation", Vector) = (.45, .1718, .1133, 0)
		_DiffuseRadiance("Water Diffuse", color) = (.0338, .1015, .2109, 0)
		_Skybox("Skybox", CUBE) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 objPos : TEXCOORD1;
				float3 objCameraPos: TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			samplerCUBE _Skybox;
			float3 _BoxSize;
			float3 _Kd;
			float3 _Attenuation;
			float3 _DiffuseRadiance;
			sampler2D _RefractTexture;
			sampler2D _ReflectTexture;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.objPos = v.vertex.xyz;//mul(unity_ObjectToWorld, v.vertex);
				o.objCameraPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1));
				return o;
			}
			
			float2 BoxIntersection(float3 ro, float3 rd, float3 boxSize, out float3 outNormal) 
			{
				float3 m = 1.0f / rd;
				float3 n = m*ro;
				float3 k = abs(m)*boxSize;

				float3 t1 = -n - k;
				float3 t2 = -n + k;

				float tN = max(max(t1.x, t1.y), t1.z);
				float tF = min(min(t2.x, t2.y), t2.z);

				if (tN > tF || tF < 0.0f)
					return float2(0, 0);

				//outNormal = -sign(rd)*step(t1.yzx, t1.xyz)*step(t1.zxy, t1.xyz); //normal of tN
				outNormal = sign(rd)*step(t2.xyz, t2.yzx)*step(t2.xyz, t2.zxy);//normal of tF

				return float2(tN, tF);
			}

			void ComputeScattering(half depth, half3 diffuseRadiance,
				float3 attenuation_c, half3 kd, out half3 outScattering, out half3 inScattering)
			{
				outScattering = exp(-attenuation_c*depth);
				inScattering = diffuseRadiance* (1 - outScattering*exp(-depth*kd));
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				
				float3 viewVector = i.objPos - i.objCameraPos;
				float3 outNormal;
				float2 d = BoxIntersection(i.objCameraPos.xyz, (viewVector), _BoxSize, outNormal);
				float3 near = i.objCameraPos.xyz + viewVector * d.x;
				float3 far = i.objCameraPos.xyz + viewVector * d.y;
				float len = length(far - near);

				float3 outScattering;
				float3 inScattering;
				ComputeScattering(len*20, _DiffuseRadiance, _Attenuation, _Kd, outScattering, inScattering);

				float3 refVect = reflect(-viewVector, outNormal);
				float3 refColor = texCUBE(_Skybox, normalize(refVect)).xyz;

				col.xyz = outScattering+inScattering;
				col.w = saturate( len*20);
				//col.w = 1;
				return col;
			}
			ENDCG
		}
	}
}
