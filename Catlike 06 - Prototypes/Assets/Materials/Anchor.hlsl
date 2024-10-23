#include "../Scripts/GPUStructs.cs.hlsl"

#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	StructuredBuffer<Anchor> _Anchors;
#endif

void ConfigureProcedural () {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		unity_ObjectToWorld = 0.0;
		unity_ObjectToWorld._m03_m23 = _Anchors[unity_InstanceID].position;
		unity_ObjectToWorld._m00_m11_m22_m33 = 1.0;
	#endif
}

void ConfigureProcedural_float (float3 In, out float3 Out) {
	Out = In;
}

void GetInstanceColor_float (out float3 Color)
{
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		Color = _Anchors[unity_InstanceID].color;
	#else
		Color = 0;
	#endif
}
