using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
struct UpdateVisualizationJob : IJobFor
{
	readonly static ulong[] bitmaps =
	{
		0b00000_01110_01010_01010_01010_01110_00000, // 0
		0b00000_00100_00110_00100_00100_01110_00000, // 1
		0b00000_01110_01000_01110_00010_01110_00000, // 2
		0b00000_01110_01000_01110_01000_01110_00000, // 3
		0b00000_01010_01010_01110_01000_01000_00000, // 4
		0b00000_01110_00010_01110_01000_01110_00000, // 5
		0b00000_01110_00010_01110_01010_01110_00000, // 6

		0b00000_10001_01010_00100_01010_10001_00000, // mine
		0b00000_00000_00100_01110_00100_00000_00000, // marked sure
		0b11111_11111_11011_10001_11011_11111_11111, // marked mistaken
		0b00000_01110_01010_01000_00100_00000_00100, // marked unsure
		0b00000_00000_00000_00000_00000_00000_00000  // hidden
	};

	static readonly float3[] colorations =
	{
		1.00f * float3(1f, 1f, 1f), // 0
		1.00f * float3(0f, 0f, 1f), // 1
		2.00f * float3(0f, 1f, 1f), // 2
		5.00f * float3(0f, 1f, 0f), // 3
		10.0f * float3(1f, 1f, 0f), // 4
		20.0f * float3(1f, 0f, 0f), // 5
		20.0f * float3(1f, 0f, 1f), // 6

		30.0f * float3(1f, 0f, 1f), // mine
		1.00f * float3(1f, 0f, 0f), // marked sure
		50.0f * float3(1f, 0f, 1f), // marked mistaken
		0.25f * float3(1f, 1f, 1f), // marked unsure
		0.00f * float3(0f, 0f, 0f)  // hidden
	};

	enum Symbol { Mine = 7, MarkedSure, MarkedMistaken, MarkedUnsure, Hidden }

	static int GetSymbolIndex (CellState state) =>
		state.Is(CellState.Revealed) ?
			state.Is(CellState.Mine) ? (int)Symbol.Mine :
			state.Is(CellState.MarkedSure) ? (int)Symbol.MarkedMistaken :
			(int)state.Without(CellState.Revealed) :
		state.Is(CellState.MarkedSure) ? (int)Symbol.MarkedSure :
		state.Is(CellState.MarkedUnsure) ? (int)Symbol.MarkedUnsure :
		(int)Symbol.Hidden;

	[NativeDisableParallelForRestriction]
	public NativeArray<float3> positions, colors, ripples;

	public int rippleCount;

	[ReadOnly]
	public Grid grid;

	public void Execute (int i)
	{
		int blockOffset = i * GridVisualization.blocksPerCell;
		int symbolIndex = GetSymbolIndex(grid[i]);
		ulong bitmap = bitmaps[symbolIndex];
		float3 coloration = colorations[symbolIndex];

		for (int bi = 0; bi < GridVisualization.blocksPerCell; bi++)
		{
			bool altered = (bitmap & ((ulong)1 << bi)) != 0;

			float3 position = positions[blockOffset + bi];
			float ripples = AccumulateRipples(position);
			position.y = (altered ? 0.5f : 0f) - 0.5f * ripples;
			positions[blockOffset + bi] = position;
			colors[blockOffset + bi] =
				(altered ? coloration : 0.5f) * (1f - 0.05f * ripples);
		}
	}

	float AccumulateRipples (float3 position)
	{
		float sum = 0f;
		for (int r = 0; r < rippleCount; r++)
		{
			float3 ripple = ripples[r];
			float d = 50f * ripple.z - distance(position.xz, ripple.xy);
			if (0 < d && d < 10f)
			{
				sum += (1f - cos(d * 2f * PI / 10f)) * (1f - ripple.z * ripple.z);
			}
		}
		return sum;
	}
}
