using Unity.Mathematics;

public struct BulletState
{
	public float2 position, velocity;

	public float timeRemaining;

	public bool exploded;

	public bool Alive => timeRemaining > 0f;

	public void Explode()
	{
		exploded = true;
		timeRemaining = 0f;
	}
}
