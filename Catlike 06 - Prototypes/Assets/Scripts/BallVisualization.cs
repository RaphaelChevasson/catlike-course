using Unity.Mathematics;
using UnityEngine;

using Random = UnityEngine.Random;

public class BallVisualization : MonoBehaviour
{
	[SerializeField, Min(0f)]
	float minSpinSpeed = 20f, maxSpinSpeed = 60f;

	PrefabInstancePool<BallVisualization> pool;

	Vector3 rotationAxis;

	float radius, rotationSpeed, rotationAngle;

	public BallVisualization Spawn()
	{
		BallVisualization instance = pool.GetInstance(this);
		instance.pool = pool;
		instance.radius = -1f;
		instance.rotationAxis = Random.onUnitSphere;
		instance.rotationSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);
		for (int i = 0; i < instance.transform.childCount; i++)
		{
			instance.transform.GetChild(i).localRotation = Random.rotation;
		}
		return instance;
	}

	public void Despawn() => pool.Recycle(this);

	public void UpdateVisualization(float2 position, float targetRadius)
	{
		rotationAngle += rotationSpeed * Time.deltaTime;
		if (rotationAngle > 360f)
		{
			rotationAngle -= 360f;
		}
		transform.SetLocalPositionAndRotation(
			new Vector3(position.x, position.y),
			Quaternion.AngleAxis(rotationAngle, rotationAxis)
		);
		if (radius != targetRadius)
		{
			radius = targetRadius;
			transform.localScale = Vector3.one * (2f * targetRadius);
		}
	}
}
