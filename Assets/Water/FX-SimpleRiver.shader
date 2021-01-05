Shader "Custom/FX/FX-SimpleRiver"
{
    Properties
    {
		[Header(Color)]
		_WaterColor("Water Color", Color) = (0,0.3411765,0.6235294,1)
		_DeepWaterColor("Deep Water Color", Color) = (0,0.3411765,0.6235294,1)
		_DepthTransparency("Depth Transparency", Range(0.5,10)) = 1.5
		_Fade("Fade", Range(0.1,5)) = 1

		[Space(50)]
		[Header(Shore)]
		_ShoreFade("Shore Fade", Float) = 0.3
		_ShoreTransparency("Shore Transparency", Float) = 0.04


		[Space(50)]
		[Header(Light)]
		_Specular("Specular", Range(0, 10)) = 1
		_LightWrapping("Light Wrapping", Float) = 0
		_Gloss("Gloss", Range(0, 1)) = 0.55

		[Space(50)]
		[Header(Small Waves)]
		[NoScaleOffset] _SmallWavesTexture("Small Waves Texture", 2D) = "bump" {}
		_SmallWavesTiling("Small Waves Tiling", Float) = 1.5
		_SmallWavesSpeed("Small Waves Speed", Float) = 60
		_SmallWaveRrefraction("Small Wave Rrefraction", Range(0, 3)) = 1

		[Space(50)]
		[Header(Medium Waves)]
		[NoScaleOffset]_MediumWavesTexture("Medium Waves Texture", 2D) = "bump" {}
		_MediumWavesTiling("Medium Waves Tiling", Float) = 3
		_MediumWavesSpeed("Medium Waves Speed", Float) = -80
		_MediumWaveRefraction("Medium Wave Refraction", Range(0, 3)) = 2

		[Space(50)]
		[Header(Large Waves)]
		[NoScaleOffset]_LargeWavesTexture("Large Waves Texture", 2D) = "bump" {}
		_LargeWavesTiling("Large Waves Tiling", Float) = 0.5
		_LargeWavesSpeed("Large Waves Speed", Float) = 60
		_LargeWaveRefraction("Large Wave Refraction", Range(0, 3)) = 2.5

		[Space(50)]
		[Header(TilingDistance)]
		_MediumTilingDistance("Medium Tiling Distance", Float) = 200
		_LongTilingDistance("Long Tiling Distance", Float) = 500
		_DistanceTilingFade("Distance Tiling Fade", Float) = 1



		[Space(50)]
		[Header(Reflections)]
		_ReflectionIntensity("Reflection Intensity ", Range(0, 1)) = 0.5
		[HideInInspector]_ReflectionTex("Reflection Tex", 2D) = "white" {}
		_RefractionDistance("Refraction Distance", Float) = 10
		_RefractionFalloff("Refraction Falloff", Float) = 1

		[Space(50)]
		[Header(Foam)]
		
		//foam
		[NoScaleOffset]_FoamTexture("Foam Texture", 2D) = "white" {}
		_FoamTiling("Foam Tiling", Float) = 3
		_FoamBlend("Foam Blend", Float) = 0.15
		_FoamVisibility("Foam Visibility", Range(0, 1)) = 0.3
		_FoamIntensity("Foam Intensity", Float) = 10
		_FoamContrast("Foam Contrast", Range(0, 0.5)) = 0.25
		_FoamColor("Foam Color", Color) = (0.3823529,0.3879758,0.3879758,1)
		_FoamSpeed("Foam Speed", Float) = 120
		_FoamDistFalloff("Foam Dist. Falloff", Float) = 16
		_FoamDistFade("Foam Dist. Fade", Float) = 9.5/**/

		//shore wave
		[Space(50)]
		[Header(Surge)]
		[Toggle]
		_SurgeType("SurgeType",Float) = 1
		_Range("Range", Range(0,5)) = 3  //the range of the wave
		_SurgeColor("Surge Color", Color) = (0,0.3411765,0.6235294,1) //the color of the wave
		_SurgeTex("Surge Tex", 2D) = "Surge" {}
		_SurgeSpeed("SurgeSpeed", float) = -12.64 //the speed of the wave
		_SurgeRange("SurgeRange", float) = 0.3
		_SurgeDelta("SurgeDelta", float) = 2.43

		_NoiseRange("NoiseRange", float) = 6.43
		_NoiseTex("Noise", 2D) = "white" {} //the noise of the wave
    }
    SubShader
    {
	   Tags {
				"IgnoreProjector" = "True"
				"Queue" = "Transparent"
				"RenderType" = "Transparent"
			}

		GrabPass{ "_GrabTexture" }

        Pass
        {
			Tags {
				"LightMode" = "ForwardBase"
			}

			Blend SrcAlpha OneMinusSrcAlpha

			Cull Off
			ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#include "UnityCG.cginc"
			#include "UnityPBSLighting.cginc"
			#include "UnityStandardBRDF.cginc"


			#pragma target 3.0

			uniform float4 _WaterColor;
			uniform float4 _DeepWaterColor;
			uniform float _DepthTransparency;
			uniform float _Fade;
			
			//Custom global texture, depth and screen color
			//uniform sampler2D_float _LastDepthTexture;
			//uniform sampler2D_float _SceneColorTexture;
			uniform sampler2D _GrabTexture;
			uniform sampler2D_float _CameraDepthTexture;

			//reflection related
			uniform sampler2D _ReflectionTex;
			uniform float4 _ReflectionTex_ST;

			uniform float _ReflectionIntensity;
			uniform fixed _EnableReflections;

			uniform float _RefractionDistance;
			uniform float _RefractionFalloff;

			//wave parameters

			uniform sampler2D _SmallWavesTexture;
			uniform sampler2D _MediumWavesTexture;
			uniform sampler2D _LargeWavesTexture;

			uniform float _SmallWaveRrefraction;
			uniform float _SmallWavesSpeed;
			uniform float _SmallWavesTiling;


			uniform float _MediumWavesTiling;
			uniform float _MediumWavesSpeed;
			uniform float _MediumWaveRefraction;

			uniform float _LargeWaveRefraction;
			uniform float _LargeWavesTiling;
			uniform float _LargeWavesSpeed;

			//wave distance
			uniform float _MediumTilingDistance;
			uniform float _DistanceTilingFade;
			uniform float _LongTilingDistance;

			//light
			uniform float _Specular;
			uniform float _LightWrapping;
			uniform float _Gloss;

			//shore
			uniform float _ShoreFade;
			uniform float _ShoreTransparency;

			//foam parameter
			uniform float _FoamBlend;
			uniform float4 _FoamColor;
			uniform float _FoamIntensity;
			uniform float _FoamContrast;
			uniform sampler2D _FoamTexture;
			uniform float _FoamSpeed;
			uniform float _FoamTiling;
			uniform float _FoamDistFalloff;
			uniform float _FoamDistFade;
			uniform float _FoamVisibility;

			//foam wave
			uniform float4 _SurgeColor;
			uniform sampler2D _SurgeTex;
			uniform float _SurgeSpeed;
			uniform float _SurgeRange;
			uniform float _Range;
			fixed _SurgeDelta;

			uniform sampler2D _NoiseTex;
			uniform float _NoiseRange;
			uniform float _SurgeType;

            struct VertexInput
            {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float2 texcoord0 : TEXCOORD0;
            };

            struct Interpolators
            {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 posWorld : TEXCOORD1;
				float3 normalDir : TEXCOORD2;
				float3 tangentDir : TEXCOORD3;
				float3 bitangentDir : TEXCOORD4;
				float4 screenPos : TEXCOORD5;
				float4 projPos : TEXCOORD6;
            };

			Interpolators vert (VertexInput v)
            {
				Interpolators i = (Interpolators)0;
				i.uv = v.texcoord0;
				i.normalDir = UnityObjectToWorldNormal(v.normal);
				i.tangentDir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
				i.bitangentDir = normalize(cross(i.normalDir, i.tangentDir) * v.tangent.w);

				i.posWorld = mul(unity_ObjectToWorld, v.vertex);

				i.pos = UnityObjectToClipPos(v.vertex);

				i.projPos = ComputeScreenPos(i.pos);
				COMPUTE_EYEDEPTH(i.projPos.z);
				i.screenPos = i.pos;
				
                return i;
            }

			float3 getWave(sampler2D tex, float3 objScale, Interpolators i,
				float tiling, float speed, float refraction,
				float _clampedDistance1, float _clampedDistance2) {

				//UV zoom setting
				float2 scale = objScale.rb*tiling;

				//the UV offset depending on time and speed
				float2 _smallWavesPanner = (i.uv + (float3((speed / scale), 0.0) * (_Time.r / 100.0)));

				float2 wavesUV = _smallWavesPanner * scale;

				//When sampling, the samples are sampled according to the time offset and the offset scaling
				float3 wavesTex = UnpackNormal(tex2D(tex, wavesUV));
				
				float3 wavesTex2 = UnpackNormal(tex2D(tex, wavesUV / 20.0));

				float3 wavesTex3 = UnpackNormal(tex2D(tex, wavesUV / 60));

				return lerp(
					float3(0, 0, 1),
					lerp(lerp(wavesTex.rgb, wavesTex2.rgb, _clampedDistance1), wavesTex3.rgb, _clampedDistance2),
					lerp(lerp(refraction, refraction / 2.0, _clampedDistance1), (refraction / 8), _clampedDistance2));
			}

			float3 getFoam(Interpolators i, float3 _blendWaterColor,float3 objScale, float depthGap, float3 normalWaveLocal) {

				//Sample according to wave normals and screen coordinates
				float2 _remap = (i.screenPos.rg + normalWaveLocal.rg)*0.5 + 0.5;
				float4 _ReflectionTex_var = tex2D(_ReflectionTex, TRANSFORM_TEX(_remap, _ReflectionTex));

				float _rotator_ang = 1.5708;
				float _rotator_spd = 1.0;
				float _rotator_cos = cos(_rotator_spd*_rotator_ang);
				float _rotator_sin = sin(_rotator_spd*_rotator_ang);
				float2 _rotator_piv = float2(0.5, 0.5);

				//rotate the UV, uv * 2D
				float2 _rotator = (mul(i.uv - _rotator_piv, float2x2(_rotator_cos, -_rotator_sin, _rotator_sin, _rotator_cos)) + _rotator_piv);

				//The Tiling of the foam map is multiplied by the object scale to get the UV scale
				float2 _FoamDivision = objScale.rb*_FoamTiling;

				//UV offset by time
				float3 _foamUVSpeed = (float3(_FoamSpeed / _FoamDivision, 0.0)*(_Time.r / 100.0));

				////rotated UV + uv offset
				float2 _FoamAdd = (_rotator + _foamUVSpeed);

				////UV * zoom setting
				float2 _foamUV = (_FoamAdd*_FoamDivision);
				float4 _foamTex1 = tex2D(_FoamTexture, _foamUV);

				float2 _FoamAdd2 = (i.uv + _foamUVSpeed);
				float2 _foamUV2 = (_FoamAdd2*_FoamDivision);
				float4 _foamTex2 = tex2D(_FoamTexture, _foamUV2);

				float2 _foamUV3 = (_FoamAdd*objScale.rb*_FoamTiling / 3.0);
				float4 _foamTex3 = tex2D(_FoamTexture, _foamUV3);

				float2 maxUV = (_FoamAdd2*_foamUV3);
				float4 _foamTex4 = tex2D(_FoamTexture, maxUV);

				//Depending on the distance to mix the foam texture of several different UV
				float3 blendFoamRGB = lerp((_foamTex1.rgb - _foamTex2.rgb), (_foamTex3.rgb - _foamTex4.rgb),
					saturate(pow((distance(i.posWorld.rgb, _WorldSpaceCameraPos) / 20), 3)));
				//shade
				float3 foamRGBGray = (dot(blendFoamRGB, float3(0.3, 0.59, 0.11)) - _FoamContrast) / (1.0 - 2 * _FoamContrast);

				//float depth = (saturate(depthGap / _FoamBlend) - 1.0);

				//Mix colors according to depth
				float3 foamRGB = foamRGBGray * _FoamColor.rgb *_FoamIntensity;// * lerp(1, depth, _FormType)

				float3 sqrtFoamRGB = (foamRGB*foamRGB);
				return lerp(_blendWaterColor, sqrtFoamRGB, _FoamVisibility);

			}

			//chore wave
			float3 getSurge(Interpolators i,float3 objScale, float depthGap)
			{
				//zoom UV
				float2  surgeUVScale = objScale.rb / 200;
				//noise texture
				fixed4 noiseColor = tex2D(_NoiseTex, i.uv*objScale.rb / 5);
				//the first wave
				fixed4 surgeColor = tex2D(_SurgeTex, float2(1 - min(_Range, depthGap) / _Range + _SurgeRange * sin(_Time.x*_SurgeSpeed + noiseColor.r*_NoiseRange), 1)*surgeUVScale);
				surgeColor.rgb *= (1 - (sin(_Time.x*_SurgeSpeed + noiseColor.r*_NoiseRange) + 1) / 2)*noiseColor.r;
				//the second wave
				fixed4 surgeColor2 = tex2D(_SurgeTex, float2(1 - min(_Range, depthGap) / _Range + _SurgeRange * sin(_Time.x*_SurgeSpeed + _SurgeDelta + noiseColor.r*_NoiseRange) + 0.5, 1)*surgeUVScale);
				surgeColor2.rgb *= (1 - (sin(_Time.x*_SurgeSpeed + _SurgeDelta + noiseColor.r*_NoiseRange) + 1) / 2)*noiseColor.r;

				//control the range of the wave depending on the depth
				half surgeWave = 1 - min(_Range, depthGap) / _Range;
				return (surgeColor.rgb + surgeColor2.rgb * _SurgeColor) * surgeWave;
			}
			

            fixed4 frag (Interpolators i) : SV_Target
            {
				//Through the transformation matrix from the world to the model space to get the change of the basis vector, and then get the scaling information
				float3 recipObjScale = float3(length(unity_WorldToObject[0].xyz), length(unity_WorldToObject[1].xyz), length(unity_WorldToObject[2].xyz));
				float3 objScale = 1.0 / recipObjScale;

#if UNITY_UV_STARTS_AT_TOP 
				float grabSign = -_ProjectionParams.x;
#else
				float grabSign = _ProjectionParams.x;
#endif

				i.normalDir = normalize(i.normalDir);
				i.screenPos = float4(i.screenPos.xy / i.screenPos.w, 0, 0);
				i.screenPos.y *= _ProjectionParams.x;

				//the change of the tangent
				float3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.normalDir);
				//the view direction
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				
				//the distance to the camera
				float _distance = distance(i.posWorld.rgb, _WorldSpaceCameraPos);
				float _clampedDistance1 = saturate(pow((_distance / _MediumTilingDistance), _DistanceTilingFade));
				float _clampedDistance2 = saturate(pow((_distance / _LongTilingDistance), _DistanceTilingFade));


				float3 _SmallWaveNormal = getWave(_SmallWavesTexture, objScale, i, _SmallWavesTiling, _SmallWavesSpeed, _SmallWaveRrefraction, _clampedDistance1, _clampedDistance2);
				float3 _MediumWaveNormal = getWave(_MediumWavesTexture, objScale, i, _MediumWavesTiling, _MediumWavesSpeed, _MediumWaveRefraction, _clampedDistance1, _clampedDistance2);
				float3 _LargeWaveNormal = getWave(_LargeWavesTexture, objScale, i, _LargeWavesTiling, _LargeWavesSpeed, _LargeWaveRefraction, _clampedDistance1, _clampedDistance2);

				//combine the waves
				float3 normalWaveLocal = (_SmallWaveNormal + _MediumWaveNormal + _LargeWaveNormal);
				float3 normalDirection = normalize(mul(normalWaveLocal, tangentTransform));

				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float3 lightColor = _LightColor0.rgb;
				float3 halfDirection = normalize(viewDirection + lightDirection);

				float attenuation = 1;
				float3 attenColor = attenuation * _LightColor0.xyz;

				// Gloss:
				float gloss = _Gloss;
				float specPow = exp2(gloss * 10.0 + 1.0);

				// the light Specular:
				float NdotL = saturate(dot(normalDirection, lightDirection));
				float3 specularColor = (_Specular*_LightColor0.rgb);
				float3 directSpecular = attenColor * pow(max(0, dot(halfDirection, normalDirection)), specPow)*specularColor;
				
				//the depth of the scene
				float sceneZ = max(0, LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)))) - _ProjectionParams.g);
				float partZ = max(0, i.projPos.z - _ProjectionParams.g);

				float depthGap = sceneZ - partZ;

				float deepMultiplier = pow(saturate(depthGap / _DepthTransparency), _ShoreFade)*saturate(depthGap / _ShoreTransparency);

				//the offset of sceneUV
				float2 sceneUVs = float2(1, grabSign) * i.screenPos.xy * 0.5 + 0.5
					+ lerp(
					((normalWaveLocal.rg*(_MediumWaveRefraction*0.02))*deepMultiplier),
						float2(0, 0), saturate(pow((distance(i.posWorld.rgb, _WorldSpaceCameraPos) / _RefractionDistance),
							_RefractionFalloff)));

				float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
				
				//combine deep color and  water color depending on the depth
				float3 _blendWaterColor = saturate(
					_DeepWaterColor.rgb + sceneColor.rgb * saturate(_Fade - depthGap) * _WaterColor.rgb
				);
				//return float4(_blendWaterColor,1);

				/*foam*/
				float3 foamColor = getFoam(i, _blendWaterColor,objScale, depthGap, normalWaveLocal);

				/*wave*/
				float3 surgeFinalColor = getSurge(i, objScale, depthGap);
				//return float4(surgeFinalColor, 1);

				//return float4(foamColor,1);
				float foamDepht = 1 - (saturate(depthGap / _FoamBlend));
				//foamDepht 0.5 to reduce the light
				float3 finalColor = directSpecular + _blendWaterColor+ lerp(foamDepht*0.5, surgeFinalColor, _SurgeType)* foamColor;
				fixed4 finalRGBA = fixed4(lerp(sceneColor.rgb, finalColor, deepMultiplier), 1);
				return finalRGBA;
            }
            ENDCG
        }
    }
}
