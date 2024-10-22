using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct Scent
{
	NativeArray<float> scentA, scentB;

	bool useA;

	float cooldown;

	public Scent (Maze maze)
	{
		scentA = new NativeArray<float>(
			maze.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory
		);
		scentB = new NativeArray<float>(maze.Length, Allocator.Persistent);
		useA = false;
		cooldown = 0f;
	}

	public void Dispose ()
	{
		if (scentA.IsCreated)
		{
			scentA.Dispose();
			scentB.Dispose();
		}
	}

	public NativeArray<float> Disperse (Maze maze, Vector3 playerPosition)
	{
		cooldown -= Time.deltaTime;
		if (cooldown <= 0f)
		{
			cooldown += 0.1f;
			new DisperseScentJob
			{
				maze = maze,
				oldScent = useA ? scentA : scentB,
				newScent = useA ? scentB : scentA,
			}.ScheduleParallel(maze.Length, maze.SizeEW, default).Complete();

			useA = !useA;
		}
		NativeArray<float> current = useA ? scentA : scentB;
		current[maze.WorldPositionToIndex(playerPosition)] = 1f;
		return current;
	}
}
