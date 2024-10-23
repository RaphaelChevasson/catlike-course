using TMPro;
using UnityEngine;

public class Game : MonoBehaviour
{
	[SerializeField]
	TextMeshPro startText;

	[SerializeField]
	TextMeshPro[] displayTexts;

	[SerializeField]
	Player[] players;

	[SerializeField]
	Grid grid;

	bool isPlaying;

	int activePlayerCount, currentPlayerIndex;

	void Awake ()
	{
		grid.Initialize();
		for (int i = 0; i < players.Length; i++)
		{
			players[i].Initialize(grid);
		}
	}

	void OnDestroy ()
	{
		grid.Dispose();
		for (int i = 0; i < players.Length; i++)
		{
			players[i].Dispose();
		}
	}

	void Update ()
	{
		if (isPlaying)
		{
			UpdateGame();
		}
		else
		{
			for (int i = 1; i <= displayTexts.Length; i++)
			{
				if (Input.GetKeyDown(KeyCode.Alpha0 + i))
				{
					StartNewGame(i);
					break;
				}
			}
		}

		for (int i = 0; i < activePlayerCount; i++)
		{
			players[i].UpdateVisualization();
		}
		grid.Draw();
		for (int i = 0; i < activePlayerCount; i++)
		{
			players[i].Draw();
		}
	}

	void StartNewGame (int newPlayerCount)
	{
		grid.StartNewGame();
		startText.gameObject.SetActive(false);
		isPlaying = true;
		currentPlayerIndex = 0;
		for (int i = 0; i < activePlayerCount; i++)
		{
			players[i].Clear();
		}
		for (int i = 0; i < newPlayerCount; i++)
		{
			int directionIndex = i == 1 && newPlayerCount == 2 ? 2 : i;
			players[i].StartNewGame(
				displayTexts[directionIndex], grid.GetStartPosition(directionIndex)
			);
		}
		players[0].CreateTile();
		activePlayerCount = newPlayerCount;
		grid.UpdateVisualization();
	}

	void UpdateGame ()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			PlaceTile();
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			players[currentPlayerIndex].RotateTile(true);
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			players[currentPlayerIndex].RotateTile(false);
		}
	}

	void PlaceTile ()
	{
		int i = currentPlayerIndex;
		do
		{
			players[i].Walk();
			i = (i + 1) % activePlayerCount;
		}
		while (i != currentPlayerIndex);

		do
		{
			i = (i + 1) % activePlayerCount;
		}
		while (i != currentPlayerIndex && !players[i].CanKeepWalking);
		currentPlayerIndex = i;

		if (players[currentPlayerIndex].CanKeepWalking)
		{
			players[currentPlayerIndex].CreateTile();
		}
		else
		{
			isPlaying = false;
		}
		grid.UpdateVisualization();
	}
}
