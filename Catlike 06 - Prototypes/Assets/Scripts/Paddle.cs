using TMPro;
using UnityEngine;

public class Paddle : MonoBehaviour
{
	static readonly int
		emissionColorId = Shader.PropertyToID("_EmissionColor"),
		faceColorId = Shader.PropertyToID("_FaceColor"),
		timeOfLastHitId = Shader.PropertyToID("_TimeOfLastHit");

	[SerializeField]
	TextMeshPro scoreText;

	[SerializeField]
	MeshRenderer goalRenderer;

	[SerializeField, ColorUsage(true, true)]
	Color goalColor = Color.white;

	[SerializeField, Min(0f)]
	float
		minExtents = 4f,
		maxExtents = 4f,
		speed = 10f,
		maxTargetingBias = 0.75f;

	[SerializeField]
	bool isAI;

	int score;

	float extents, targetingBias;

	Material goalMaterial, paddleMaterial, scoreMaterial;

	void Awake ()
	{
		goalMaterial = goalRenderer.material;
		goalMaterial.SetColor(emissionColorId, goalColor);
		paddleMaterial = GetComponent<MeshRenderer>().material;
		scoreMaterial = scoreText.fontMaterial;
		SetScore(0);
	}

	public void StartNewGame ()
	{
		SetScore(0);
		ChangeTargetingBias();
	}

	public bool HitBall (float ballX, float ballExtents, out float hitFactor)
	{
		ChangeTargetingBias();
		hitFactor =
			(ballX - transform.localPosition.x) /
			(extents + ballExtents);

		bool success = -1f <= hitFactor && hitFactor <= 1f;
		if (success)
		{
			paddleMaterial.SetFloat(timeOfLastHitId, Time.time);
		}
		return success;
	}

	public bool ScorePoint (int pointsToWin)
	{
		goalMaterial.SetFloat(timeOfLastHitId, Time.time);
		SetScore(score + 1, pointsToWin);
		return score >= pointsToWin;
	}

	public void Move (float target, float arenaExtents)
	{
		Vector3 p = transform.localPosition;
		p.x = isAI ? AdjustByAI(p.x, target) : AdjustByPlayer(p.x);
		float limit = arenaExtents - extents;
		p.x = Mathf.Clamp(p.x, -limit, limit);
		transform.localPosition = p;
	}

	float AdjustByAI (float x, float target)
	{
		target += targetingBias * extents;
		if (x < target)
		{
			return Mathf.Min(x + speed * Time.deltaTime, target);
		}
		return Mathf.Max(x - speed * Time.deltaTime, target);
	}

	float AdjustByPlayer (float x)
	{
		bool goRight = Input.GetKey(KeyCode.RightArrow);
		bool goLeft = Input.GetKey(KeyCode.LeftArrow);
		if (goRight && !goLeft)
		{
			return x + speed * Time.deltaTime;
		}
		else if (goLeft && !goRight)
		{
			return x - speed * Time.deltaTime;
		}
		return x;
	}

	void SetScore (int newScore, float pointsToWin = 1000f)
	{
		score = newScore;
		scoreText.SetText("{0}", newScore);
		scoreMaterial.SetColor(faceColorId, goalColor * (newScore / pointsToWin));
		SetExtents(Mathf.Lerp(maxExtents, minExtents, newScore / (pointsToWin - 1f)));
	}

	void ChangeTargetingBias () =>
		targetingBias = Random.Range(-maxTargetingBias, maxTargetingBias);

	void SetExtents (float newExtents)
	{
		extents = newExtents;
		Vector3 s = transform.localScale;
		s.x = 2f * newExtents;
		transform.localScale = s;
	}
}
