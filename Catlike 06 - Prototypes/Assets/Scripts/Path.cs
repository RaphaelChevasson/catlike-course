using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor.Sprites;
using UnityEngine;

[System.Serializable]
public class Path
{
	const float walkerYOffset = 0.075f;

	static int
		walkersId = Shader.PropertyToID("_Walkers"),
		colorId = Shader.PropertyToID("_Color");

	[SerializeField, Min(0.1f)]
	float walkerSpawnRate = 2f, walkerSpeed = 3f;

	[SerializeField]
	Mesh walkerMesh;

	[SerializeField]
	Material walkerMaterial;

	[SerializeField, ColorUsage(false, true)]
	Color walkerColor;

	ComputeBuffer walkersBuffer;

	WalkersJob walkersJob;

	JobHandle walkerJobHandle;

	public int Length => walkersJob.PathSegmentCount;

	public void Initialize () {
		walkersJob.Initialize(walkerSpawnRate, walkerSpeed);
		walkerMaterial = new Material(walkerMaterial);
		walkerMaterial.SetColor(colorId, walkerColor);
		walkersBuffer = new(walkersJob.WalkerCapacity, Walker.Size);
		walkerMaterial.SetBuffer(walkersId, walkersBuffer);
	}

	public void Clear () => walkersJob.Clear();

	public void Dispose ()
	{
		walkersJob.Dispose();
		walkersBuffer.Release();
	}

	public void Add (float2 from, float2 to, float y) =>
		walkersJob.AddPathSegment(from, to, y + walkerYOffset);

	public void UpdateVisualization () =>
		walkerJobHandle = walkersJob.Schedule(Time.deltaTime);

	public void Draw ()
	{
		if (walkersJob.HasSomethingToDraw)
		{
			walkerJobHandle.Complete();
			if (walkersBuffer.count < walkersJob.WalkerCapacity)
			{
				walkersBuffer.Release();
				walkersBuffer = new(walkersJob.WalkerCapacity, Walker.Size);
				walkerMaterial.SetBuffer(walkersId, walkersBuffer);
			}

			NativeArray<Walker> walkers = walkersJob.Walkers;
			walkersBuffer.SetData(walkers);
			Graphics.DrawMeshInstancedProcedural(
				walkerMesh, 0, walkerMaterial,
				new Bounds(Vector3.zero, Vector3.one), walkers.Length
			);
		}
	}
}
