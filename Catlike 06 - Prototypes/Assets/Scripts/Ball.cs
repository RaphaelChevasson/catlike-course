using UnityEngine;
using UnityEngine.UIElements;

public class Ball : MonoBehaviour
{
	[SerializeField, Min(0f)]
	float
		maxXSpeed = 20f,
		maxStartXSpeed = 2f,
		constantYSpeed = 10f,
		extents = 0.5f;

	[SerializeField]
	ParticleSystem bounceParticleSystem, startParticleSystem, trailParticleSystem;

	[SerializeField]
	int
		bounceParticleEmission = 20,
		startParticleEmission = 100;

	Vector2 position, velocity;

	public float Extents => extents;

	public Vector2 Position => position;

	public Vector2 Velocity => velocity;

	void Awake () => gameObject.SetActive(false);

	public void StartNewGame ()
	{
		position = Vector2.zero;
		UpdateVisualization();
		velocity.x = Random.Range(-maxStartXSpeed, maxStartXSpeed);
		velocity.y = -constantYSpeed;
		gameObject.SetActive(true);
		startParticleSystem.Emit(startParticleEmission);
		SetTrailEmission(true);
		trailParticleSystem.Play();
	}

	public void EndGame ()
	{
		position.x = 0f;
		gameObject.SetActive(false);
		SetTrailEmission(false);
	}

	public void SetXPositionAndSpeed (float start, float speedFactor, float deltaTime)
	{
		velocity.x = maxXSpeed * speedFactor;
		position.x = start + velocity.x * deltaTime;
	}

	public void Move () => position += velocity * Time.deltaTime;

	public void UpdateVisualization () => trailParticleSystem.transform.localPosition =
		transform.localPosition = new Vector3(position.x, 0f, position.y);

	public void BounceX (float boundary)
	{
		float durationAfterBounce = (position.x - boundary) / velocity.x;
		position.x = 2f * boundary - position.x;
		velocity.x = -velocity.x;
		EmitBounceParticles(
			boundary,
			position.y - velocity.y * durationAfterBounce,
			boundary < 0f ? 90f : 270f
		);
	}

	public void BounceY (float boundary)
	{
		float durationAfterBounce = (position.y - boundary) / velocity.y;
		position.y = 2f * boundary - position.y;
		velocity.y = -velocity.y;
		EmitBounceParticles(
			position.x - velocity.x * durationAfterBounce,
			boundary,
			boundary < 0f ? 0f : 180f
		);
	}

	void EmitBounceParticles (float x, float z, float rotation)
	{
		ParticleSystem.ShapeModule shape = bounceParticleSystem.shape;
		shape.position = new Vector3(x, 0f, z);
		shape.rotation = new Vector3(0f, rotation, 0f);
		bounceParticleSystem.Emit(bounceParticleEmission);
	}

	void SetTrailEmission (bool enabled)
	{
		ParticleSystem.EmissionModule emission = trailParticleSystem.emission;
		emission.enabled = enabled;
	}
}
