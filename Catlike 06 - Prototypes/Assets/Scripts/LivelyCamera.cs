using UnityEngine;

public class LivelyCamera : MonoBehaviour
{
	[SerializeField, Min(0f)]
	float
		springStrength = 100f,
		dampingStrength = 10f,
		jostleStrength = 40f,
		pushStrength = 1f,
		maxDeltaTime = 1f / 60f;

	Vector3 anchorPosition, velocity;

	void Awake () => anchorPosition = transform.localPosition;

	public void JostleY () => velocity.y += jostleStrength;

	public void PushXZ (Vector2 impulse)
	{
		velocity.x += pushStrength * impulse.x;
		velocity.z += pushStrength * impulse.y;
	}

	void LateUpdate ()
	{
		float dt = Time.deltaTime;
		while (dt > maxDeltaTime)
		{
			TimeStep(maxDeltaTime);
			dt -= maxDeltaTime;
		}
		TimeStep(dt);
	}

	void TimeStep (float dt)
	{
		Vector3 displacement = anchorPosition - transform.localPosition;
		Vector3 acceleration = springStrength * displacement - dampingStrength * velocity;
		velocity += acceleration * dt;
		transform.localPosition += velocity * dt;
	}
}
