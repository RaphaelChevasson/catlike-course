using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
struct InitializeVisualizationJob : IJobFor
{
	[WriteOnly, NativeDisableParallelForRestriction]
	public NativeArray<float3> positions, colors;

	public int rows, columns;

	public void Execute (int i)
	{
		float3 cellPosition = GetCellPosition(i);
		int blockOffset = i * GridVisualization.blocksPerCell;

		for (int bi = 0; bi < GridVisualization.blocksPerCell; bi++)
		{
			positions[blockOffset + bi] = cellPosition + GetBlockPosition(bi);
			colors[blockOffset + bi] = 0.5f;
		}
	}

	float3 GetBlockPosition (int i)
	{
		int r = i / GridVisualization.columnsPerCell;
		int c = i - r * GridVisualization.columnsPerCell;
		return float3(c, 0f, r);
	}

	float3 GetCellPosition (int i)
	{
		int r = i / columns;
		int c = i - r * columns;
		return
			float3(
				c - (columns - 1) * 0.5f,
				0f,
				r - (rows - 1) * 0.5f - (c & 1) * 0.5f + 0.25f
			) * float3(
				GridVisualization.columnsPerCell + 1,
				0f,
				GridVisualization.rowsPerCell + 1
			) - float3(
				GridVisualization.columnsPerCell / 2,
				0f,
				GridVisualization.rowsPerCell / 2
			);
	}
}
