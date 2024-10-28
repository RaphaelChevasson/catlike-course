using TMPro;
using Unity.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
	[SerializeField]
	TextMeshPro scoreDisplay;

	[SerializeField]
	HealthBar healthBar;

	[SerializeField, Min(1)]
	int maxHealth = 10;

	[SerializeField]
	ParticleSystem explosionParticleSystem;

	[SerializeField, Min(1)]
	int hitParticleCount = 100, destructionParticleCount = 400;

	[SerializeField, Min(0f)]
	float radius = 0.5f;

	[SerializeField, Min(0.01f)]
	float cursorFollowSpeed = 40f, cursorSnapDuration = 0.05f;

	[SerializeField, Min(0f)]
	float fireCooldown = 0.1f, fireSpreadAngle = 5f;

	[SerializeField]
	BulletManager bulletManager;

	Vector2 fireOffset, previousPosition, velocity;

	float cooldown, directionAngle, previousDirectionAngle;

	int lastCheckedHealth, lastCheckedScore;

	NativeReference<int> health, score;

	public float Radius => radius;

	public bool FreeAim
	{ get; set; }

	public Vector2 Position
	{ get; private set; }

	public Vector2 TargetPosition
	{ private get; set; }

	public void Initialize(ref HitJob hitJob) {
		health = new(Allocator.Persistent);
		score = new(Allocator.Persistent);
		scoreDisplay.SetText("");
		gameObject.SetActive(false);
		hitJob.health = health;
		hitJob.score = score;
		hitJob.playerRadius = radius;
	}

	public void Dispose() {
		health.Dispose();
		score.Dispose();
	}

	public void StartNewGame(Vector2 position)
	{
		Position = TargetPosition = previousPosition = position;
		velocity = Vector2.zero;
		directionAngle = previousDirectionAngle = 0f;
		cooldown = fireCooldown;
		fireOffset = new Vector2(0f, radius);
		health.Value = lastCheckedHealth = maxHealth;
		healthBar.Show(1f);
		score.Value = lastCheckedScore = 0;
		scoreDisplay.SetText("0");
		gameObject.SetActive(true);
	}

	public void UpdateState(float dt)
	{
		previousPosition = Position;
		previousDirectionAngle = directionAngle;
		Vector2 targetVector = TargetPosition - Position;
		float squareTargetDistance = targetVector.sqrMagnitude;
		if (squareTargetDistance > 0.0001f)
		{
			Position = Vector2.SmoothDamp(
				Position, TargetPosition, ref velocity,
				cursorSnapDuration, cursorFollowSpeed, dt
			);
			if (FreeAim)
			{
				fireOffset = targetVector * (radius / Mathf.Sqrt(squareTargetDistance));
				directionAngle =
					Mathf.Atan2(targetVector.x, targetVector.y) * -Mathf.Rad2Deg;
			}
		}

		cooldown -= dt;
		if (cooldown <= 0f)
		{
			cooldown += fireCooldown;
			bulletManager.Add(
				Position - fireOffset,
				directionAngle + 180f + Random.Range(-fireSpreadAngle, fireSpreadAngle)
			);
		}
	}

	public bool UpdateVisualization(float dtInterpolator)
	{
		transform.SetLocalPositionAndRotation(
			Vector2.LerpUnclamped(previousPosition, Position, dtInterpolator),
			Quaternion.Euler(
				0f, 0f,
				Mathf.LerpAngle(previousDirectionAngle, directionAngle, dtInterpolator)
			)
		);

		if (lastCheckedScore != score.Value)
		{
			lastCheckedScore = score.Value;
			scoreDisplay.SetText("{0}", lastCheckedScore);
		}

		if (lastCheckedHealth == health.Value)
		{
			return false;
		}

		lastCheckedHealth = health.Value;
		healthBar.Show((float)lastCheckedHealth / maxHealth);
		bool isDestroyed = lastCheckedHealth <= 0;
		explosionParticleSystem.Emit(
			new ParticleSystem.EmitParams
			{
				position = Position,
				applyShapeToPosition = true
			},
			isDestroyed ? destructionParticleCount : hitParticleCount
		);
		if (isDestroyed)
		{
			gameObject.SetActive(false);
		}
		return isDestroyed;
	}
}
