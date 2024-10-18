using TMPro;
using UnityEngine;

public class Game : MonoBehaviour
{
	[SerializeField]
	TextMeshPro minesText;

	[SerializeField]
	Material material;

	[SerializeField]
	Mesh mesh;

	[SerializeField, Min(1)]
	int rows = 8, columns = 21, mines = 30;

	Grid grid;

	GridVisualization visualization;

	int markedSureCount;

	bool isGameOver;

	void OnEnable ()
	{
		grid.Initialize(rows, columns);
		visualization.Initialize(grid, material, mesh);
		StartNewGame();
	}

	void StartNewGame ()
	{
		isGameOver = false;
		mines = Mathf.Min(mines, grid.CellCount);
		minesText.SetText("{0}", mines);
		markedSureCount = 0;
		grid.PlaceMines(mines);
	}

	void OnDisable ()
	{
		grid.Dispose();
		visualization.Dispose();
	}

	void Update ()
	{
		if (grid.Rows != rows || grid.Columns != columns)
		{
			OnDisable();
			OnEnable();
		}

		PerformAction();
		visualization.Draw();
	}

	void PerformAction ()
	{
		bool revealAction = Input.GetMouseButtonDown(0);
		bool markAction = Input.GetMouseButtonDown(1);
		if (
			(revealAction || markAction) &&
			visualization.TryGetHitCellIndex(
				Camera.main.ScreenPointToRay(Input.mousePosition), out int cellIndex
			)
		)
		{
			if (isGameOver)
			{
				StartNewGame();
			}
			if (revealAction)
			{
				DoRevealAction(cellIndex);
			}
			else
			{
				DoMarkAction(cellIndex);
			}
		}
	}

	void DoMarkAction (int cellIndex)
	{
		CellState state = grid[cellIndex];
		if (state.Is(CellState.Revealed))
		{
			return;
		}

		if (state.IsNot(CellState.Marked))
		{
			grid[cellIndex] = state.With(CellState.MarkedSure);
			markedSureCount += 1;
		}
		else if (state.Is(CellState.MarkedSure))
		{
			grid[cellIndex] =
				state.Without(CellState.MarkedSure).With(CellState.MarkedUnsure);
			markedSureCount -= 1;
		}
		else
		{
			grid[cellIndex] = state.Without(CellState.MarkedUnsure);
		}

		minesText.SetText("{0}", mines - markedSureCount);
	}

	void DoRevealAction (int cellIndex)
	{
		CellState state = grid[cellIndex];
		if (state.Is(CellState.MarkedOrRevealed))
		{
			return;
		}

		grid.Reveal(cellIndex);

		if (state.Is(CellState.Mine))
		{
			isGameOver = true;
			minesText.SetText("FAILURE");
			grid.RevealMinesAndMistakes();
		}
		else if (grid.HiddenCellCount == mines)
		{
			isGameOver = true;
			minesText.SetText("SUCCESS");
		}
	}
}
