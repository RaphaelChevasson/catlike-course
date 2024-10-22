using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct FindDiagonalPassagesJob : IJobFor
{
	public Maze maze;

	public void Execute (int i)
	{
		MazeFlags cell = maze[i];
		if (
			cell.Has(MazeFlags.PassageN | MazeFlags.PassageE) &&
			maze[i + maze.StepN + maze.StepE].Has(MazeFlags.PassageS | MazeFlags.PassageW)
		)
		{
			cell = cell.With(MazeFlags.PassageNE);
		}
		if (
			cell.Has(MazeFlags.PassageN | MazeFlags.PassageW) &&
			maze[i + maze.StepN + maze.StepW].Has(MazeFlags.PassageS | MazeFlags.PassageE)
		)
		{
			cell = cell.With(MazeFlags.PassageNW);
		}
		if (
			cell.Has(MazeFlags.PassageS | MazeFlags.PassageE) &&
			maze[i + maze.StepS + maze.StepE].Has(MazeFlags.PassageN | MazeFlags.PassageW)
		)
		{
			cell = cell.With(MazeFlags.PassageSE);
		}
		if (
			cell.Has(MazeFlags.PassageS | MazeFlags.PassageW) &&
			maze[i + maze.StepS + maze.StepW].Has(MazeFlags.PassageN | MazeFlags.PassageE)
		)
		{
			cell = cell.With(MazeFlags.PassageSW);
		}
		maze[i] = cell;
	}
}
