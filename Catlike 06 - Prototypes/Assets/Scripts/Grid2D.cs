using Unity.Mathematics;

[System.Serializable]
public struct Grid2D<T>
{
	T[] cells;

	int2 size;

	public T this[int x, int y]
	{
		get => cells[y * size.x + x];
		set => cells[y * size.x + x] = value;
	}

	public T this[int2 c]
	{
		get => cells[c.y * size.x + c.x];
		set => cells[c.y * size.x + c.x] = value;
	}

	public int2 Size => size;

	public int SizeX => size.x;

	public int SizeY => size.y;

	public bool IsUndefined => cells == null || cells.Length == 0;

	public Grid2D (int2 size)
	{
		this.size = size;
		cells = new T[size.x * size.y];
	}

	public bool AreValidCoordinates (int2 c) =>
		0 <= c.x && c.x < size.x && 0 <= c.y && c.y < size.y;

	public void Swap (int2 a, int2 b) => (this[a], this[b]) = (this[b], this[a]);
}
