using Unity.Mathematics;
using UnityEngine;

using Random = UnityEngine.Random;

using static Unity.Mathematics.math;

public struct Area2D
{
	public float2 extents;

	public Vector2 RandomVector2 =>
		new(Random.Range(-extents.x, extents.x), Random.Range(-extents.y, extents.y));

	public float2 Wrap(float2 position) => position + extents *
		select(select(0f, 2f, position < -extents), -2f, position > extents);

	public static Area2D FromView(Camera camera)
	{
		Area2D area;
		area.extents.y =
			-Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) *
			camera.transform.localPosition.z;
		area.extents.x = area.extents.y * camera.aspect;
		return area;
	}
}
