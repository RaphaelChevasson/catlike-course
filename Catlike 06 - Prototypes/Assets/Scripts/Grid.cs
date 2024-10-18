using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct Grid
{
	public CellState this[int i]
	{
		get => states[i];
		set => states[i] = value;
	}

	public int Rows { get; private set; }

	public int Columns { get; private set; }

	public int CellCount => states.Length;

	public int HiddenCellCount => CellCount - RevealedCellCount;

	public int RevealedCellCount
	{
		get => revealedCellCount[0];
		set => revealedCellCount[0] = value;
	}

	NativeArray<int> revealedCellCount;

	NativeArray<CellState> states;

	public void Initialize (int rows, int columns)
	{
		Rows = rows;
		Columns = columns;
		revealedCellCount = new NativeArray<int>(1, Allocator.Persistent);
		states = new NativeArray<CellState>(Rows * Columns, Allocator.Persistent);
	}

	public void Dispose ()
	{
		revealedCellCount.Dispose();
		states.Dispose();
	}

	public int GetCellIndex (int row, int column) => row * Columns + column;

	public bool TryGetCellIndex (int row, int column, out int index)
	{
		bool valid = 0 <= row && row < Rows && 0 <= column && column < Columns;
		index = valid ? GetCellIndex(row, column) : -1;
		return valid;
	}

	public void GetRowColumn (int index, out int row, out int column)
	{
		row = index / Columns;
		column = index - row * Columns;
	}

	public void PlaceMines (int mines) => new PlaceMinesJob
	{
		grid = this,
		mines = mines,
		seed = Random.Range(1, int.MaxValue)
	}.Schedule().Complete();

	public void Reveal (int index)
	{
		var job = new RevealRegionJob
		{
			grid = this
		};
		GetRowColumn(index, out job.startRowColumn.x, out job.startRowColumn.y);
		job.Schedule().Complete();
	}

	public void RevealMinesAndMistakes () => new RevealMinesAndMistakesJob
	{
		grid = this
	}.ScheduleParallel(CellCount, Columns, default).Complete();
}
