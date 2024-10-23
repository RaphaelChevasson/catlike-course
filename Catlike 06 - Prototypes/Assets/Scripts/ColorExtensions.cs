using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public static class ColorExtensions
{
	public static float3 GetRGB (this Color color) => float3(color.r, color.g, color.b);
}
