#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	StructuredBuffer<float3> _Positions, _Colors;
#endif

void ConfigureProcedural () {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		float3 position = _Positions[unity_InstanceID];

		unity_ObjectToWorld = 0.0;
		unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0);
		unity_ObjectToWorld._m00_m11_m22 = 1.0;
	#endif
}

void ConfigureProcedural_float (float3 In, out float3 Out) {
	Out = In;
}

void GetInstanceColor_float (out float3 Color)
{
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		Color = _Colors[unity_InstanceID];
	#else
		Color = 0;
	#endif
}
