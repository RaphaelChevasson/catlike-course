using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
	[SerializeField]
	BulletVisualization bulletPrefab;

	[SerializeField]
	ParticleSystem explosionParticleSystem;

	[SerializeField, Min(0)]
	int explosionParticleCount = 50;

	[SerializeField, Min(0f)]
	float speed = 12f, startLifetime = 1f, radius = 0.15f;

	NativeList<BulletState> states;

	List<BulletVisualization> visualizations;

	UpdateBulletJob updateBulletJob;

	public void Initialize(Area2D worldArea, ref HitJob hitJob) {
		states = new(100, Allocator.Persistent);
		visualizations = new(states.Capacity);
		updateBulletJob = new UpdateBulletJob
		{
			bullets = states,
			worldArea = worldArea
		};
		hitJob.bullets = states;
		hitJob.bulletRadius = radius;
	}

	public void Dispose() {
		states.Dispose();
	}

	public void StartNewGame() {
		for (int i = 0; i < visualizations.Count; i++)
		{
			visualizations[i].Despawn();
		}
		visualizations.Clear();
		states.Clear();
	}

	public void Add(Vector2 position, float angle)
	{
		Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
		states.Add(new BulletState
		{
			position = position,
			velocity = (Vector2)(rotation * new Vector3(0f, speed)),
			timeRemaining = startLifetime
		});
		visualizations.Add(bulletPrefab.Spawn(rotation));
	}

	public JobHandle UpdateBullets(float dt)
	{
		updateBulletJob.dt = dt;
		return updateBulletJob.Schedule(states.Length, default);
	}

	public void UpdateVisualization(float dtExtrapolated)
	{
		for (int i = 0; i < visualizations.Count; i++)
		{
			BulletState state = states[i];
			if (state.Alive)
			{
				visualizations[i].UpdateVisualization(
					state.position + state.velocity * dtExtrapolated,
					Mathf.Max(0f, state.timeRemaining - dtExtrapolated) / startLifetime
				);
			}
			else
			{
				if (state.exploded)
				{
					explosionParticleSystem.Emit(
						new ParticleSystem.EmitParams
						{
							position = new Vector3(state.position.x, state.position.y),
							applyShapeToPosition = true
						},
						explosionParticleCount
					);
				}

				int lastIndex = states.Length - 1;
				states[i] = states[lastIndex];
				states.Length -= 1;

				visualizations[i].Despawn();
				visualizations[i] = visualizations[lastIndex];
				visualizations.RemoveAt(lastIndex);
				i -= 1;
			}
		}
	}
}
