using Unity.Mathematics;
using UnityEngine.Rendering;

[GenerateHLSL(needAccessors: false)]
public struct Anchor
{
	public float2 position;
	public float3 color;

	public static int Size => 5 * 4;
}

[GenerateHLSL(needAccessors: false)]
public struct Bridge
{
	public float3 position;
	public float2 direction;
	public float length;
	public float3 color;

	public static int Size => 9 * 4;
}

[GenerateHLSL(needAccessors: false)]
public struct Walker
{
	public float3 position;
	public float2 direction;

	public static int Size => 5 * 4;
}
