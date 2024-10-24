using Unity.Mathematics;

using static Unity.Mathematics.math;

[System.Serializable]
public struct Move
{
	public MoveDirection Direction
	{ get; private set; }

	public int2 From
	{ get; private set; }

	public int2 To
	{ get; private set; }

	public bool IsValid => Direction != MoveDirection.None;

	public Move (int2 coordinates, MoveDirection direction)
	{
		Direction = direction;
		From = coordinates;
		To = coordinates + direction switch
		{
			MoveDirection.Up => int2(0, 1),
			MoveDirection.Right => int2(1, 0),
			MoveDirection.Down => int2(0, -1),
			_ => int2(-1, 0)
		};
	}

	public static Move FindMove (Match3Game game)
	{
		int2 s = game.Size;
		for (int2 c = 0; c.y < s.y; c.y++)
		{
			for (c.x = 0; c.x < s.x; c.x++)
			{
				TileState t = game[c];

				if (c.x >= 3 && game[c.x - 2, c.y] == t && game[c.x - 3, c.y] == t)
				{
					return new Move(c, MoveDirection.Left);
				}

				if (c.x + 3 < s.x && game[c.x + 2, c.y] == t && game[c.x + 3, c.y] == t)
				{
					return new Move(c, MoveDirection.Right);
				}

				if (c.y >= 3 && game[c.x, c.y - 2] == t && game[c.x, c.y - 3] == t)
				{
					return new Move(c, MoveDirection.Down);
				}

				if (c.y + 3 < s.y && game[c.x, c.y + 2] == t && game[c.x, c.y + 3] == t)
				{
					return new Move(c, MoveDirection.Up);
				}

				if (c.y > 1)
				{
					if (c.x > 1 && game[c.x - 1, c.y - 1] == t)
					{
						if (
							c.x >= 2 && game[c.x - 2, c.y - 1] == t ||
							c.x + 1 < s.x && game[c.x + 1, c.y - 1] == t
						)
						{
							return new Move(c, MoveDirection.Down);
						}
						if (
							c.y >= 2 && game[c.x - 1, c.y - 2] == t ||
							c.y + 1 < s.y && game[c.x - 1, c.y + 1] == t
						)
						{
							return new Move(c, MoveDirection.Left);
						}
					}

					if (c.x + 1 < s.x && game[c.x + 1, c.y - 1] == t)
					{
						if (c.x + 2 < s.x && game[c.x + 2, c.y - 1] == t)
						{
							return new Move(c, MoveDirection.Down);
						}
						if (
							c.y >= 2 && game[c.x + 1, c.y - 2] == t ||
							c.y + 1 < s.y && game[c.x + 1, c.y + 1] == t
						)
						{
							return new Move(c, MoveDirection.Right);
						}
					}
				}

				if (c.y + 1 < s.y)
				{
					if (c.x > 1 && game[c.x - 1, c.y + 1] == t)
					{
						if (
							c.x >= 2 && game[c.x - 2, c.y + 1] == t ||
							c.x + 1 < s.x && game[c.x + 1, c.y + 1] == t
						)	
						{
							return new Move(c, MoveDirection.Up);
						}
						if (c.y + 2 < s.y && game[c.x - 1, c.y + 2] == t)
						{
							return new Move(c, MoveDirection.Left);
						}
					}

					if (c.x + 1 < s.x && game[c.x + 1, c.y + 1] == t)
					{
						if (c.x + 2 < s.x && game[c.x + 2, c.y + 1] == t)
						{
							return new Move(c, MoveDirection.Up);
						}
						if (c.y + 2 < s.y && game[c.x + 1, c.y + 2] == t)
						{
							return new Move(c, MoveDirection.Right);
						}
					}
				}
			}
		}

		return default;
	}
}
