using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
public struct DisperseScentJob : IJobFor
{
	[ReadOnly]
	public Maze maze;

	[ReadOnly, NativeDisableParallelForRestriction]
	public NativeArray<float> oldScent;

	public NativeArray<float> newScent;

	public void Execute (int i)
	{
		MazeFlags cell = maze[i];
		float scent = oldScent[i];

		float fromNeighbors = 0f;
		float dispersalFactor = 0f;
		if (cell.Has(MazeFlags.PassageE))
		{
			fromNeighbors += oldScent[i + maze.StepE];
			dispersalFactor += 1f;
		}
		if (cell.Has(MazeFlags.PassageW))
		{
			fromNeighbors += oldScent[i + maze.StepW];
			dispersalFactor += 1f;
		}
		if (cell.Has(MazeFlags.PassageN))
		{
			fromNeighbors += oldScent[i + maze.StepN];
			dispersalFactor += 1f;
		}
		if (cell.Has(MazeFlags.PassageS))
		{
			fromNeighbors += oldScent[i + maze.StepS];
			dispersalFactor += 1f;
		}

		scent += (fromNeighbors - scent * dispersalFactor) * 0.2f;
		newScent[i] = scent * 0.5f;
	}
}
