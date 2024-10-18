using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
struct PlaceMinesJob : IJob
{
	public Grid grid;

	public int mines, seed;

	public void Execute ()
	{
		grid.RevealedCellCount = 0;
		int candidateCount = grid.CellCount;
		var candidates = new NativeArray<int>(
			candidateCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
		);

		for (int i = 0; i < grid.CellCount; i++)
		{
			grid[i] = CellState.Zero;
			candidates[i] = i;
		}

		Random random = new Random((uint)seed);
		for (int m = 0; m < mines; m++)
		{
			int candidateIndex = random.NextInt(candidateCount--);
			SetMine(candidates[candidateIndex]);
			candidates[candidateIndex] = candidates[candidateCount];
		}
	}

	void SetMine (int i)
	{
		grid[i] = grid[i].With(CellState.Mine);

		grid.GetRowColumn(i, out int r, out int c);
		Increment(r - 1, c);
		Increment(r + 1, c);
		Increment(r, c - 1);
		Increment(r, c + 1);

		int rowOffset = (c & 1) == 0 ? 1 : -1;
		Increment(r + rowOffset, c - 1);
		Increment(r + rowOffset, c + 1);
	}

	void Increment (int r, int c)
	{
		if (grid.TryGetCellIndex(r, c, out int i))
		{
			grid[i] += 1;
		}
	}
}
