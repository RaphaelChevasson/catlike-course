using Unity.Mathematics;

using static Unity.Mathematics.math;

public static class MathExtensions {

	public static float4x3 TransformVectors (
		this float3x4 trs, float4x3 p, float w = 1f
	) => float4x3(
		trs.c0.x * p.c0 + trs.c1.x * p.c1 + trs.c2.x * p.c2 + trs.c3.x * w,
		trs.c0.y * p.c0 + trs.c1.y * p.c1 + trs.c2.y * p.c2 + trs.c3.y * w,
		trs.c0.z * p.c0 + trs.c1.z * p.c1 + trs.c2.z * p.c2 + trs.c3.z * w
	);

	public static float4x3 TransformVectors (this float3x3 m, float4x3 v) => float4x3(
		m.c0.x * v.c0 + m.c1.x * v.c1 + m.c2.x * v.c2,
		m.c0.y * v.c0 + m.c1.y * v.c1 + m.c2.y * v.c2,
		m.c0.z * v.c0 + m.c1.z * v.c1 + m.c2.z * v.c2
	);

	public static float3x4 Get3x4 (this float4x4 m) =>
		float3x4(m.c0.xyz, m.c1.xyz, m.c2.xyz, m.c3.xyz);
}