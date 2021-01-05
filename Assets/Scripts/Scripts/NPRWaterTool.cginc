#ifndef NPRWATERTOOL
#define NPRWATERTOOL


half3 PerPixelNormal(sampler2D bumpMap, half4 coords, half bumpStrength)
{
	half2 bump = (UnpackNormal(tex2D(bumpMap, coords.xy)) + UnpackNormal(tex2D(bumpMap, coords.zw))) * 0.5;
	bump += (UnpackNormal(tex2D(bumpMap, coords.xy * 2))*0.5 + UnpackNormal(tex2D(bumpMap, coords.zw * 2))*0.5) * 0.5;
	bump += (UnpackNormal(tex2D(bumpMap, coords.xy * 8))*0.5 + UnpackNormal(tex2D(bumpMap, coords.zw * 8))*0.5) * 0.5;

	half3 worldNormal = half3(0, 0, 0);
	worldNormal.xz = bump.xy * bumpStrength;
	worldNormal.y = 1;
	return worldNormal;
}

float3 CalculateWavesDisplacement(float3 vert, float4 _QA, float4 _A, float4 _S, float4 _Dx, float4 _Dz, float4 _L)
{
	float3 pos = float3(0, 0, 0);
	float4 phase = _Dx*vert.x + _Dz*vert.z + _S*_Time.y;
	float4 sinp = float4(0, 0, 0, 0), cosp = float4(0, 0, 0, 0);
	sincos(_L*phase, sinp, cosp);

	pos.x = dot(_QA*_Dx, cosp);
	pos.z = dot(_QA*_Dz, cosp);
	pos.y = dot(_A, sinp);

	return pos;
}

float3 CalculateWavesNormal(float3 vert, float4 _QA, float4 _A, float4 _S, float4 _Dx, float4 _Dz, float4 _L)
{
	float3 nor = float3(0, 0, 0);
	float4 phase = _Dx*vert.x + _Dz*vert.z + _S*_Time.y;
	float4 sinp = float4(0, 0, 0, 0), cosp = float4(0, 0, 0, 0);
	sincos(_L*phase, sinp, cosp);

	nor.x = -dot(_L*_A*_Dx, cosp);
	nor.z = -dot(_L*_A*_Dz, cosp);
	nor.y = 1 - dot(_QA*_L, sinp);

	nor = normalize(nor);

	return nor;
}

float3 CalculateWavesDisplacementNormal(float3 vert, float4 _QA, float4 _A, float4 _S, float4 _Dx, float4 _Dz, float4 _L, out float3 nor)
{
	float3 pos = float3(0, 0, 0);
	float4 phase = _Dx*vert.x + _Dz*vert.z + _S*_Time.y;
	float4 sinp = float4(0, 0, 0, 0), cosp = float4(0, 0, 0, 0);
	sincos(_L*phase, sinp, cosp);

	pos.x = dot(_QA*_Dx, cosp);
	pos.z = dot(_QA*_Dz, cosp);
	pos.y = dot(_A, sinp);

	nor.x = -dot(_L*_A*_Dx, cosp);
	nor.z = -dot(_L*_A*_Dz, cosp);
	nor.y = 1 - dot(_QA*_L, sinp);

	nor = normalize(nor);

	return pos;
}


float2 BoxIntersection(float3 ro, float3 rd, float3 boxSize, out float3 nNormal, out float3 fNormal)
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

	nNormal = -sign(rd)*step(t1.yzx, t1.xyz)*step(t1.zxy, t1.xyz); //normal of tN
	fNormal = sign(rd)*step(t2.xyz, t2.yzx)*step(t2.xyz, t2.zxy);//normal of tF

	return float2(tN, tF);
}

void ComputeScattering(half depth, half3 diffuseRadiance,
	float3 attenuation_c, half3 kd, out half3 outScattering, out half3 inScattering)
{
	outScattering = exp(-attenuation_c*depth);
	inScattering = diffuseRadiance* (1 - outScattering*exp(-depth*kd));
}

// Fresnel approximation, power = 5
float FastFresnel(float3 I, float3 N, float R0)
{
	float icosIN = saturate(1 - dot(I, N));
	float i2 = icosIN*icosIN, i4 = i2*i2;
	return R0 + (1 - R0)*(i4*icosIN);
}

void CalculateWavesBinormalTangent(float3 vert, float4 _QA, float4 _A, float4 _S, float4 _Dx, float4 _Dz, float4 _L, out float3 binormal, out float3 tangent)
{
	float4 phase = _Dx*vert.x + _Dz*vert.z + _S*_Time.y;
	float4 sinp = float4(0, 0, 0, 0), cosp = float4(0, 0, 0, 0);
	sincos(_L*phase, sinp, cosp);

	binormal = float3(0, 0, 0);
	binormal.x = 1 - dot(_QA, _Dx*sinp*_Dx*_L);
	binormal.z = -dot(_QA, _Dz*sinp*_Dz*_L);
	binormal.y = dot(_A, _Dx*cosp*_L);

	tangent = float3(0, 0, 0);
	tangent.x = -dot(_QA, _Dx*sinp*_Dz*_L);
	tangent.z = 1 - dot(_QA, _Dz*sinp*_Dz*_L);
	tangent.y = dot(_A, _Dz*cosp*_L);

	binormal = normalize(binormal);
	tangent = normalize(tangent);
}

#endif