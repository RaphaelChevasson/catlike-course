using Unity.Mathematics;

public struct BallState
{
	public const int initialStage = 2;

	public static readonly float[] masses =
	{
		0.25f, 0.5f, 1f
	};

	public static readonly float[] radii =
	{
		0.5f, 0.7071067812f, 1f
	};

	public float2 position, velocity;

	public float mass, radius, targetRadius;

	public int stage, type;

	public bool alive;
}
