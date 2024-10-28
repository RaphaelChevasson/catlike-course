using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
public struct BounceBallsJob : IJob
{
	public NativeList<BallState> balls;

	public Area2D worldArea;

	public float bounceStrength, dt;

	public void Execute()
	{
		for (int i = 0; i < balls.Length; i++)
		{
			BallState a = balls[i];
			if (!a.alive)
			{
				continue;
			}

			for (int j = i + 1; j < balls.Length; j++)
			{
				BallState b = balls[j];
				if (!b.alive)
				{
					continue;
				}

				float2 p = worldArea.Wrap(b.position - a.position);
				float r = a.radius + b.radius;
				if (dot(p, p) < r * r)
				{
					float2 v =
						(2f / (a.mass + b.mass)) *
						(1f - r * rsqrt(max(dot(p, p), 0.0001f))) *
						bounceStrength * dt * p;
					a.velocity += b.mass * v;
					b.velocity -= a.mass * v;
					balls[i] = a;
					balls[j] = b;
				}
			}
		}
	}
}
