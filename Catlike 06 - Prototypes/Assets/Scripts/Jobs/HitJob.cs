using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
public struct HitJob : IJob
{
	public NativeList<BallState> balls;

	public NativeList<BulletState> bullets;

	public NativeReference<int> health, score;

	public Area2D worldArea;

	public float2 playerPosition;

	public float bulletRadius, playerRadius, fragmentSeparation, explosionStrength;

	public void Execute()
	{
		for (int i = 0; i < balls.Length; i++)
		{
			CheckBall(i);
		}
	}

	void CheckBall (int i)
	{
		BallState ball = balls[i];
		if (!ball.alive)
		{
			return;
		}

		for (int b = 0; b < bullets.Length; b++)
		{
			BulletState bullet = bullets[b];
			if (bullet.Alive && CheckHit(ball, bullet.position, bulletRadius))
			{
				bullet.Explode();
				bullets[b] = bullet;
				ball.alive = false;
				balls[i] = ball;
				score.Value += 1;
				return;
			}
		}

		if (health.Value > 0 && CheckHit(ball, playerPosition, playerRadius))
		{
			ball.alive = false;
			balls[i] = ball;
			health.Value -= 1;
			score.Value += 1;
		}
	}

	bool CheckHit (BallState ball, float2 position, float radius)
	{
		float2 p = worldArea.Wrap(position - ball.position);
		float r = ball.radius + radius;
		if (dot(p, p) >= r)
		{
			return false;
		}

		if (ball.stage > 0)
		{
			float2 direction = p * rsqrt(max(dot(p, p), 0.0001f));
			float2 displacement =
				fragmentSeparation * ball.radius * float2(direction.y, -direction.x);

			ball.velocity -= direction * explosionStrength / ball.mass;
			ball.stage -= 1;
			ball.radius = ball.targetRadius = BallState.radii[ball.stage];
			ball.mass = BallState.masses[ball.stage];

			float2 originalPosition = ball.position;
			ball.position = originalPosition + displacement;
			balls.Add(ball);
			ball.position = originalPosition - displacement;
			balls.Add(ball);
		}

		return true;
	}
}
