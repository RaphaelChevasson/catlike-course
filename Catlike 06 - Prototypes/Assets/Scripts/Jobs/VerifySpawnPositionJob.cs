using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
public struct VerifySpawnPositionJob : IJob
{
	[ReadOnly]
	public NativeList<BallState> balls;

	[WriteOnly]
	public NativeReference<bool> success;

	public Area2D worldArea;

	public float2 avoidPosition, position;

	public float avoidRadius, radius;

	public void Execute()
	{
		float2 p = worldArea.Wrap(position - avoidPosition);
		float r = avoidRadius + radius;
		if (dot(p, p) <= r * r)
		{
			success.Value = false;
			return;
		}

		for (int i = 0; i < balls.Length; i++)
		{
			BallState ball = balls[i];
			if (ball.alive)
			{
				p = worldArea.Wrap(position - ball.position);
				r = ball.radius + radius;
				if (dot(p, p) <= r * r)
				{
					success.Value = false;
					return;
				}
			}
		}

		success.Value = true;
	}
}
