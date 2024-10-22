using UnityEngine.UI;

[System.Flags]
public enum MazeFlags
{
	Empty = 0,

	PassageN = 0b0001,
	PassageE = 0b0010,
	PassageS = 0b0100,
	PassageW = 0b1000,

	PassagesStraight = 0b1111,

	PassageNE = 0b0001_0000,
	PassageSE = 0b0010_0000,
	PassageSW = 0b0100_0000,
	PassageNW = 0b1000_0000,

	PassagesDiagonal = 0b1111_0000
}

public static class MazeFlagsExtensions
{
	public static bool Has (this MazeFlags flags, MazeFlags mask) =>
		(flags & mask) == mask;

	public static bool HasAny (this MazeFlags flags, MazeFlags mask) =>
		(flags & mask) != 0;

	public static bool HasNot (this MazeFlags flags, MazeFlags mask) =>
		(flags & mask) != mask;

	public static bool HasExactlyOne (this MazeFlags flags) =>
		flags != 0 && (flags & (flags - 1)) == 0;

	public static MazeFlags With (this MazeFlags flags, MazeFlags mask) =>
		flags | mask;

	public static MazeFlags Without (this MazeFlags flags, MazeFlags mask) =>
		flags & ~mask;

	public static MazeFlags StraightPassages (this MazeFlags flags) =>
		flags & MazeFlags.PassagesStraight;

	public static MazeFlags DiagonalPassages (this MazeFlags flags) =>
		flags & MazeFlags.PassagesDiagonal;

	public static MazeFlags RotatedDiagonalPassages (this MazeFlags flags, int rotation)
	{
		int bits = (int)(flags & MazeFlags.PassagesDiagonal);
		bits = (bits >> rotation) | (bits << (4 - rotation));
		return (MazeFlags)bits & MazeFlags.PassagesDiagonal;
	}
}
