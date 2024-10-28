using Unity.Jobs;
using TMPro;
using UnityEngine;

public class Game : MonoBehaviour
{
	[SerializeField]
	BallManager ballManager;

	[SerializeField]
	BulletManager bulletManager;

	[SerializeField]
	Player player;

	[SerializeField]
	Transform worldBoundsPrefab;

	[SerializeField]
	TextMeshPro instructionsDisplay;

	[SerializeField, Min(0f)]
	float worldBoundsRadius = 1.24f;

	[SerializeField, Min(0.001f)]
	float fixedDeltaTime = 0.01f;

	Area2D worldArea, playerArea;

	float dt;

	bool isPlaying;

	HitJob hitJob;

	void Awake()
	{
		worldArea = Area2D.FromView(Camera.main);
		Transform
			boundsT = Instantiate(worldBoundsPrefab),
			boundsB = Instantiate(worldBoundsPrefab),
			boundsL = Instantiate(worldBoundsPrefab),
			boundsR = Instantiate(worldBoundsPrefab);
		boundsT.localPosition = new Vector3(0f, worldArea.extents.y);
		boundsB.localPosition = new Vector3(0f, -worldArea.extents.y);
		boundsL.localPosition = new Vector3(-worldArea.extents.x, 0f);
		boundsR.localPosition = new Vector3(worldArea.extents.x, 0f);
		boundsT.localScale = boundsB.localScale =
			new Vector3(worldArea.extents.x, worldBoundsRadius, worldBoundsRadius) * 2f;
		boundsL.localScale = boundsR.localScale =
			new Vector3(worldBoundsRadius, worldArea.extents.y, worldBoundsRadius) * 2f;

		hitJob.worldArea = worldArea;
		ballManager.Initialize(worldArea, ref hitJob);
		bulletManager.Initialize(worldArea, ref hitJob);
		player.Initialize(ref hitJob);
		playerArea.extents = worldArea.extents - worldBoundsRadius - player.Radius;
		Cursor.visible = false;
		Application.targetFrameRate = 0;
	}

	void OnDisable()
	{
		ballManager.Dispose();
		bulletManager.Dispose();
		player.Dispose();
	}

	void StartNewGame()
	{
		isPlaying = true;
		dt = 0f;
		ballManager.StartNewGame();
		bulletManager.StartNewGame();
		player.StartNewGame(GetTargetPoint());
		instructionsDisplay.gameObject.SetActive(false);
	}

	void Update()
	{
		if (isPlaying)
		{
			player.FreeAim = !Input.GetMouseButton(0) && !Input.GetKey(KeyCode.Space);
			player.TargetPosition = GetTargetPoint();
		}
		else if (Input.GetKeyDown(KeyCode.Space))
		{
			StartNewGame();
		}

		dt += Time.deltaTime;
		while (dt > fixedDeltaTime)
		{
			UpdateGameState(fixedDeltaTime);
			dt -= fixedDeltaTime;
		}

		ballManager.UpdateVisualization(dt);
		bulletManager.UpdateVisualization(dt);
		if (isPlaying && player.UpdateVisualization(dt / fixedDeltaTime))
		{
			isPlaying = false;
			instructionsDisplay.SetText("GAME OVER");
			instructionsDisplay.gameObject.SetActive(true);
		}
	}

	void UpdateGameState(float dt)
	{
		if (isPlaying)
		{
			player.UpdateState(dt);
			hitJob.playerPosition = player.Position;
		}
		JobHandle handle = JobHandle.CombineDependencies(
			ballManager.UpdateBalls(dt),
			bulletManager.UpdateBullets(dt)
		);
		handle = hitJob.Schedule(handle);
		ballManager.ResolveBalls(player.Position, handle);
	}

	Vector2 GetTargetPoint()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Vector2 p = ray.origin - ray.direction * (ray.origin.z / ray.direction.z);
		p.x = Mathf.Clamp(p.x, -playerArea.extents.x, playerArea.extents.x);
		p.y = Mathf.Clamp(p.y, -playerArea.extents.y, playerArea.extents.y);
		return p;
	}
}
