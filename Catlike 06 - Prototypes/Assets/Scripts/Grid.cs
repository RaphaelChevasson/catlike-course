using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

using Random = UnityEngine.Random;

[Serializable]
public class Grid
{
	public struct Position
	{
		public int anchor, row, column;
	}

	struct Connection
	{
		public int a, b;
		public bool visited;
	}

	const float
		tileSize = 6.7f,
		verticalBridgeDistance = 0.2f;

	static float2[] anchorOffsets =
	{
		float2(-1f,  3f), float2( 1f,  3f),
		float2( 3f,  1f), float2( 3f, -1f),
		float2( 1f, -3f), float2(-1f, -3f),
		float2(-3f, -1f), float2(-3f,  1f)
	};

	static int
		anchorsID = Shader.PropertyToID("_Anchors"),
		bridgesID = Shader.PropertyToID("_Bridges");

	[SerializeField]
	int2 size = int2(9, 5);

	[SerializeField]
	Mesh instanceMesh;

	[SerializeField]
	Material anchorMaterial, bridgeMaterial;

	[SerializeField, ColorUsage(false, true)]
	Color anchorColor, bridgeColor;

	float3 linearAnchorColor, linearBridgeColor;

	NativeArray<Anchor> anchors;

	NativeArray<Bridge> bridges;

	ComputeBuffer anchorsBuffer, bridgesBuffer;

	Connection[] connections;

	int[] instanceIDs;

	int instanceCount;

	public Position GetStartPosition (int directionIndex) => directionIndex switch
	{
		0 => new Position
		{ anchor = Random.Range(0, 2), row = size.y - 1, column = size.x / 2 },
		1 => new Position
		{ anchor = Random.Range(2, 4), row = size.y / 2, column = size.x - 1 },
		2 => new Position
		{ anchor = Random.Range(4, 6), row = 0, column = size.x / 2 },
		_ => new Position
		{ anchor = Random.Range(6, 8), row = size.y / 2, column = 0 }
	};

	public void Initialize ()
	{
		int tileCount = size.x * size.y;
		connections = new Connection[tileCount * 4];
		instanceIDs = new int[tileCount];

		anchors = new(tileCount * 8, Allocator.Persistent);
		anchorsBuffer = new(anchors.Length, Anchor.Size);
		anchorMaterial.SetBuffer(anchorsID, anchorsBuffer);

		bridges = new(tileCount * 4, Allocator.Persistent);
		bridgesBuffer = new(bridges.Length, Bridge.Size);
		bridgeMaterial.SetBuffer(bridgesID, bridgesBuffer);

		linearAnchorColor = anchorColor.linear.GetRGB();
		linearBridgeColor = bridgeColor.linear.GetRGB();
	}

	public void Dispose ()
	{
		anchors.Dispose();
		anchorsBuffer.Release();
		bridges.Dispose();
		bridgesBuffer.Release();
	}

	public void UpdateVisualization ()
	{
		anchorsBuffer.SetData(anchors);
		bridgesBuffer.SetData(bridges);
	}

	public void Draw ()
	{
		if (instanceCount > 0)
		{
			var bounds = new Bounds(Vector3.zero, Vector3.one);
			Graphics.DrawMeshInstancedProcedural(
				instanceMesh, 0, anchorMaterial, bounds, instanceCount * 8
			);
			Graphics.DrawMeshInstancedProcedural(
				instanceMesh, 0, bridgeMaterial, bounds, instanceCount * 4
			);
		}
	}

	public void StartNewGame ()
	{
		for (int i = 0; i < instanceIDs.Length; i++)
		{
			instanceIDs[i] = -1;
		}
		instanceCount = 0;
	}

	public bool IsTileCreated (Position position) => GetID(position) >= 0;

	public void CreateTile (Position position, float3 linearPositionColor)
	{
		int id = ClaimID(position);
		float2 center = GetTileCenter(position);
		for (int i = 0; i < 8; i++)
		{
			anchors[id * 8 + i] = new Anchor
			{
				position = center + anchorOffsets[i],
				color = i == position.anchor ? linearPositionColor : linearAnchorColor
			};
		}

		Span<int> anchorIndices = stackalloc int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
		int availableIndices = 8;
		for (int i = 0; i < 4; i++)
		{
			int r = Random.Range(0, availableIndices--);
			int a = anchorIndices[r];
			anchorIndices[r] = anchorIndices[availableIndices];
			r = Random.Range(0, availableIndices--);
			int b = anchorIndices[r];
			anchorIndices[r] = anchorIndices[availableIndices];
			CreateConnection(id, i, a, b, center);
		}
	}

	public void RotateTile (Position position, bool clockwise)
	{
		int id = GetID(position);
		int step = clockwise ? 1 : -1;
		float2 center = GetTileCenter(position);
		for (int i = 0; i < 4; i++)
		{
			Connection c = connections[id * 4 + i];
			c.a += step;
			c.b += step;
			CreateConnection(
				id, i,
				c.a == -1 ? 7 : c.a == 8 ? 0 : c.a,
				c.b == -1 ? 7 : c.b == 8 ? 0 : c.b,
				center
			);
		}
		bridgesBuffer.SetData(bridges);
	}

	public bool TryMoveThroughTile (
		ref Position position, Path path,
		float3 linearAnchorColor, float3 linearBridgeColor
	)
	{
		int id = GetID(position);
		int connectionIndex = -1;
		Connection connection;
		do
		{
			connection = connections[id * 4 + ++connectionIndex];
		}
		while (connection.a != position.anchor && connection.b != position.anchor);
		if (connection.visited)
		{
			return false;
		}
		connection.visited = true;
		connections[id * 4 + connectionIndex] = connection;

		int exitAnchor = position.anchor == connection.a ? connection.b : connection.a;

		int anchorIndex = id * 8 + position.anchor;
		Anchor anchor = anchors[anchorIndex];
		anchor.color = linearAnchorColor;
		anchors[anchorIndex] = anchor;

		anchorIndex = id * 8 + exitAnchor;
		anchor = anchors[anchorIndex];
		anchor.color = linearAnchorColor;
		anchors[anchorIndex] = anchor;

		int bridgeIndex = id * 4 + connectionIndex;
		Bridge bridge = bridges[bridgeIndex];
		bridge.color = linearBridgeColor;
		bridges[bridgeIndex] = bridge;

		float2 center = GetTileCenter(position);
		path.Add(
			center + anchorOffsets[position.anchor],
			center + anchorOffsets[exitAnchor],
			bridge.position.y
		);
		position.anchor = exitAnchor;
		return StepToAdjacentTile(ref position);
	}

	bool StepToAdjacentTile (ref Position position)
	{
		(int rowDelta, int columnDelta, int anchorBase) step = position.anchor switch
		{
			var a when a < 2 => (1, 0, 5),
			var a when a < 4 => (0, 1, 9),
			var a when a < 6 => (-1, 0, 5),
			_ => (0, -1, 9)
		};
		position.row += step.rowDelta;
		position.column += step.columnDelta;
		position.anchor = step.anchorBase - position.anchor;
		return
			0 <= position.column && position.column < size.x &&
			0 <= position.row && position.row < size.y;
	}

	int ClaimID (Position position) =>
		instanceIDs[position.row * size.x + position.column] = instanceCount++;

	int GetID (Position position) => instanceIDs[position.row * size.x + position.column];

	float2 GetTileCenter (Position position) => float2(
		position.column - size.x * 0.5f + 0.5f,
		position.row - size.y * 0.5f + 0.5f
	) * tileSize;

	void CreateConnection (int id, int connectionIndex, int a, int b, float2 center)
	{
		connections[id * 4 + connectionIndex] = new Connection { a = a, b = b };
		float2 positionA = anchorOffsets[a], positionB = anchorOffsets[b];
		center += (positionA + positionB) * 0.5f;
		float2 line = positionB - positionA;
		float length = math.length(line);
		bridges[id * 4 + connectionIndex] = new Bridge
		{
			position = float3(
				center.x, (1.5f - connectionIndex) * verticalBridgeDistance, center.y
			),
			length = length,
			direction = line / length,
			color = linearBridgeColor
		};
	}
}