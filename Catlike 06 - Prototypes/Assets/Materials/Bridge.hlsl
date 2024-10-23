#include "../Scripts/GPUStructs.cs.hlsl"

#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	StructuredBuffer<Bridge> _Bridges;
#endif

void ConfigureProcedural () {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		unity_ObjectToWorld = 0.0;
		unity_ObjectToWorld._m03_m13_m23 = _Bridges[unity_InstanceID].position;
		unity_ObjectToWorld._m11_m33 = 1.0;

		float2 direction = _Bridges[unity_InstanceID].direction;
		unity_ObjectToWorld._m00_m20 = float2(direction.y, -direction.x);
		unity_ObjectToWorld._m02_m22 = direction;
	#endif
}

void ConfigureProcedural_float (float3 In, out float3 Out) {
	Out = In;
}

void GetInstanceColor_float (out float3 Color)
{
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		Color = _Bridges[unity_InstanceID].color;
	#else
		Color = 0;
	#endif
}

void GetInstanceScale_float (float2 ScaleXY, out float3 Scale)
{
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		Scale = float3(ScaleXY, _Bridges[unity_InstanceID].length);
	#else
		Scale = float3(ScaleXY, 1);
	#endif
}