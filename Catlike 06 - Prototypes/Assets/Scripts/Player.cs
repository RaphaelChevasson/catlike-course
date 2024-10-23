using TMPro;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class Player
{
	[SerializeField]
	Path path;

	[SerializeField, ColorUsage(false, true)]
	Color anchorColor, bridgeColor, positionColor;

	float3 linearAnchorColor, linearBridgeColor, linearPositionColor;

	TextMeshPro displayText;

	Grid grid;

	Grid.Position position;

	public bool CanKeepWalking
	{ get; private set; }

	public void Initialize (Grid grid)
	{
		this.grid = grid;
		linearAnchorColor = anchorColor.linear.GetRGB();
		linearBridgeColor = bridgeColor.linear.GetRGB();
		linearPositionColor = positionColor.linear.GetRGB();
		path.Initialize();
	}

	public void StartNewGame (TextMeshPro displayText, Grid.Position startPosition)
	{
		this.displayText = displayText;
		position = startPosition;
		displayText.SetText("0");
		displayText.gameObject.SetActive(true);
		CanKeepWalking = true;
	}

	public void Clear ()
	{
		displayText.gameObject.SetActive(false);
		CanKeepWalking = false;
		path.Clear();
	}

	public void Dispose () => path.Dispose();

	public void CreateTile () => grid.CreateTile(position, linearPositionColor);

	public void RotateTile (bool clockwise) => grid.RotateTile(position, clockwise);

	public void Walk ()
	{
		if (CanKeepWalking)
		{
			while (CanKeepWalking && grid.IsTileCreated(position))
			{
				CanKeepWalking = grid.TryMoveThroughTile(
					ref position, path, linearAnchorColor, linearBridgeColor
				);
			}
			displayText.SetText("{0}", path.Length);
		}
	}

	public void UpdateVisualization () => path.UpdateVisualization();

	public void Draw () => path.Draw();
}
