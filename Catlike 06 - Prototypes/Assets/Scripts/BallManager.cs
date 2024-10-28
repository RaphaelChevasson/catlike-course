using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class BallManager : MonoBehaviour
{
	[SerializeField]
	BallVisualization[] ballPrefabs;

	[SerializeField, Min(0f)]
	float avoidSpawnRadius = 2f, startingCooldown = 4f;

	[SerializeField, Range(0.1f, 1f)]
	float cooldownPersistence = 0.96f;

	[SerializeField, Min(0f)]
	float maxSpeed = 12.5f, maxStartSpeed = 4f;

	[SerializeField, Min(0f)]
	float bounceStrength = 100f, explosionStrength = 2f;

	[SerializeField, Range(0.01f, 1f)]
	float fragmentSeparation = 0.6f;

	float cooldown, cooldownDuration;

	NativeList<BallState> states;

	List<BallVisualization> visualizations;

	UpdateBallJob updateBallJob;

	BounceBallsJob bounceBallsJob;

	VerifySpawnPositionJob verifySpawnPositionJob;

	public void Initialize(Area2D worldArea, ref HitJob hitJob) {
		states = new(100, Allocator.Persistent);
		visualizations = new(states.Capacity);
		cooldown = cooldownDuration = startingCooldown;

		updateBallJob = new UpdateBallJob
		{
			balls = states,
			worldArea = worldArea,
			maxSpeed = maxSpeed
		};
		bounceBallsJob = new BounceBallsJob
		{
			balls = states,
			worldArea = worldArea,
			bounceStrength = bounceStrength
		};
		verifySpawnPositionJob = new VerifySpawnPositionJob
		{
			balls = states,
			success = new NativeReference<bool>(Allocator.Persistent),
			worldArea = worldArea,
			avoidRadius = avoidSpawnRadius,
			radius = BallState.radii[BallState.initialStage]
		};
		hitJob.balls = states;
		hitJob.fragmentSeparation = fragmentSeparation;
		hitJob.explosionStrength = explosionStrength;
	}

	public void Dispose() {
		states.Dispose();
		verifySpawnPositionJob.success.Dispose();
	}

	public void StartNewGame() {
		for (int i = 0; i < visualizations.Count; i++)
		{
			visualizations[i].Despawn();
		}
		visualizations.Clear();
		states.Clear();
		cooldown = cooldownDuration = startingCooldown;
	}

	public JobHandle UpdateBalls(float dt)
	{
		cooldown -= dt;
		bounceBallsJob.dt = updateBallJob.dt = dt;
		return updateBallJob.Schedule(states.Length, default);
	}

	public void ResolveBalls(Vector2 avoidSpawnPosition, JobHandle dependency)
	{
		dependency = bounceBallsJob.Schedule(dependency);
		if (cooldown <= 0f)
		{
			verifySpawnPositionJob.avoidPosition = avoidSpawnPosition;
			verifySpawnPositionJob.position = updateBallJob.worldArea.RandomVector2;
			dependency = verifySpawnPositionJob.Schedule(dependency);
		}
		dependency.Complete();

		if (cooldown <= 0f && verifySpawnPositionJob.success.Value)
		{
			cooldown += cooldownDuration;
			cooldownDuration *= cooldownPersistence;
			states.Add(new BallState
			{
				position = verifySpawnPositionJob.position,
				velocity = Random.insideUnitCircle * maxStartSpeed,
				mass = BallState.masses[BallState.initialStage],
				targetRadius = BallState.radii[BallState.initialStage],
				stage = BallState.initialStage,
				type = Random.Range(0, ballPrefabs.Length),
				alive = true
			});
		}
	}

	public void UpdateVisualization(float dtExtrapolated)
	{
		for (int i = visualizations.Count; i < states.Length; i++)
		{
			visualizations.Add(ballPrefabs[states[i].type].Spawn());
		}

		for (int i = 0; i < visualizations.Count; i++)
		{
			BallState state = states[i];
			if (state.alive)
			{
				visualizations[i].UpdateVisualization(
					state.position + state.velocity * dtExtrapolated,
					Mathf.Min(state.radius + dtExtrapolated, state.targetRadius)
				);
			}
			else
			{
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
