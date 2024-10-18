[System.Flags]
public enum CellState
{
	Zero, One, Two, Three, Four, Five, Six,
	Mine = 1 << 3,
	MarkedSure = 1 << 4,
	MarkedUnsure = 1 << 5,
	Revealed = 1 << 6,
	Marked = MarkedSure | MarkedUnsure,
	MarkedOrRevealed = Marked | Revealed,
	MarkedSureOrMine = MarkedSure | Mine
}

public static class CellStateExtensionMethods
{
	public static bool Is (this CellState s, CellState mask) => (s & mask) != 0;

	public static bool IsNot (this CellState s, CellState mask) => (s & mask) == 0;

	public static CellState With (this CellState s, CellState mask) => s | mask;

	public static CellState Without (this CellState s, CellState mask) => s & ~mask;
}