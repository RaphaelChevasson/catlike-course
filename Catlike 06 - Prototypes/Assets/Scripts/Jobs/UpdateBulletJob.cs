using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
public struct UpdateBulletJob : IJobFor
{
	public NativeList<BulletState> bullets;

	public Area2D worldArea;

	public float dt;

	public void Execute(int i)
	{
		BulletState bullet = bullets[i];
		bullet.timeRemaining -= dt;
		bullet.position = worldArea.Wrap(bullet.position + bullet.velocity * dt);
		bullets[i] = bullet;
	}
}
