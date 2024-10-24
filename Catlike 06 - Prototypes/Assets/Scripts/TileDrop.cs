using Unity.Mathematics;

[System.Serializable]
public struct TileDrop
{
	public int2 coordinates;

	public int fromY;

	public TileDrop (int x, int y, int distance)
	{
		coordinates.x = x;
		coordinates.y = y;
		fromY = y + distance;
	}
}
