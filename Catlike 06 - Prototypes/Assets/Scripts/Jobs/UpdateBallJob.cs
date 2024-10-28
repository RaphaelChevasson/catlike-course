using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

using static Unity.Mathematics.math;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
public struct UpdateBallJob : IJobFor
{
	public NativeList<BallState> balls;

	public Area2D worldArea;

	public float dt, maxSpeed;

	public void Execute(int i)
	{
		BallState ball = balls[i];
		if (dot(ball.velocity, ball.velocity) > maxSpeed * maxSpeed)
		{
			ball.velocity = normalize(ball.velocity) * maxSpeed;
		}
		ball.position = worldArea.Wrap(ball.position + ball.velocity * dt);
		ball.radius = min(ball.targetRadius, ball.radius + dt);
		balls[i] = ball;
	}
}
