using UnityEngine;

public class Game : MonoBehaviour
{
	[SerializeField]
	Match3Skin match3;

	[SerializeField]
	bool automaticPlay;

	Vector3 dragStart;

	bool isDragging;

	void Awake () => match3.StartNewGame();

	void Update ()
	{
		if (match3.IsPlaying)
		{
			if (!match3.IsBusy)
			{
				HandleInput();
			}
			match3.DoWork();
		}
		else if (Input.GetKeyDown(KeyCode.Space))
		{
			match3.StartNewGame();
		}
	}
	
	void HandleInput ()
	{
		if (automaticPlay)
		{
			match3.DoAutomaticMove();
		}
		else if (!isDragging && Input.GetMouseButtonDown(0))
		{
			dragStart = Input.mousePosition;
			isDragging = true;
		}
		else if (isDragging && Input.GetMouseButton(0))
		{
			isDragging = match3.EvaluateDrag(dragStart, Input.mousePosition);
		}
		else
		{
			isDragging = false;
		}
	}
}
