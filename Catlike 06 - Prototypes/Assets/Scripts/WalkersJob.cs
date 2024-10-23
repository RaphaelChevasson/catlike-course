using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static UnityEditor.VersionControl.Asset;
using static UnityEngine.Rendering.HableCurve;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
struct WalkersJob : IJob
{
	struct PathSegment
	{
		public float3 from, to;
		public float2 direction;
		public float length;
	}

	struct WalkerState
	{
		public int index;
		public float progress;
	}

	[ReadOnly]
	NativeList<PathSegment> pathSegments;

	NativeList<Walker> walkers;

	NativeList<WalkerState> walkerStates;

	NativeArray<float> spawnCooldown;

	float dt, spawnRate, speed;

	public bool HasSomethingToDraw => pathSegments.Length > 0;

	public int PathSegmentCount => pathSegments.Length;

	public int WalkerCapacity => walkers.Capacity;

	public NativeArray<Walker> Walkers => walkers.AsArray();

	public void Initialize (float spawnRate, float speed)
	{
		this.spawnRate = spawnRate;
		this.speed = speed;
		const int initialCapacity = 64;
		pathSegments = new(initialCapacity, Allocator.Persistent);
		walkers = new(initialCapacity, Allocator.Persistent);
		walkerStates = new(initialCapacity, Allocator.Persistent);
		spawnCooldown = new(1, Allocator.Persistent);
	}

	public void Clear () {
		pathSegments.Clear();
		walkers.Clear();
		walkerStates.Clear();
		spawnCooldown[0] = 0f;
	}

	public void Dispose () {
		pathSegments.Dispose();
		walkers.Dispose();
		walkerStates.Dispose();
		spawnCooldown.Dispose();
	}

	public void AddPathSegment (float2 from, float2 to, float y) {
		float2 line = to - from;
		float length = math.length(line);
		pathSegments.Add(new PathSegment
		{
			from = float3(from.x, y, from.y),
			to = float3(to.x, y, to.y),
			direction = line / length,
			length = length
		});
	}

	public JobHandle Schedule (float dt)
	{
		this.dt = dt;
		return pathSegments.Length > 0 ? this.Schedule() : default;
	}

	public void Execute ()
	{
		float cooldown = spawnCooldown[0] - spawnRate * dt;
		for (int i = 0; i < walkerStates.Length; i++)
		{
			cooldown = UpdateWalker(i, cooldown);
		}
		if (cooldown <= 0f)
		{
			walkerStates.Add(new WalkerState {
				index = 0,
				progress = -cooldown / spawnRate
			});
			walkers.Length += 1;
			cooldown = UpdateWalker(walkerStates.Length - 1, cooldown + 1f);
		}
		spawnCooldown[0] = cooldown;
	}

	public float UpdateWalker (int i, float cooldown)
	{
		WalkerState state = walkerStates[i];
		PathSegment segment = pathSegments[state.index];
		state.progress += speed * dt;

		while (state.progress > segment.length)
		{
			state.progress -= segment.length;
			if (++state.index >= pathSegments.Length)
			{
				state.index = 0;
				state.progress = speed * (-cooldown / spawnRate);
				cooldown += 1f;
			}
			segment = pathSegments[state.index];
		}

		walkerStates[i] = state;
		walkers[i] = new Walker
		{
			position =
				lerp(segment.from, segment.to, max(0f, state.progress) / segment.length),
			direction = segment.direction
		};
		return cooldown;
	}
}
