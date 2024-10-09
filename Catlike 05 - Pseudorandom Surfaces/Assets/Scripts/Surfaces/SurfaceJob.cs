using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using ProceduralMeshes.Streams;

using static Unity.Mathematics.math;
using static Noise;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
public struct SurfaceJob<N> : IJobFor where N : struct, INoise {

	struct Vertex4 {
		public SingleStream.Stream0 v0, v1, v2, v3;
	}

	NativeArray<Vertex4> vertices;

	Settings settings;

	float3x4 domainTRS;

	float displacement;

	public void Execute (int i) {
		Vertex4 v = vertices[i];
		float4x3 p = domainTRS.TransformVectors(transpose(float3x4(
			v.v0.position, v.v1.position, v.v2.position, v.v3.position
		)));

		var hash = SmallXXHash4.Seed(settings.seed);
		int frequency = settings.frequency;
		float amplitude = 1f, amplitudeSum = 0f;
		float4 sum = 0f;

		for (int o = 0; o < settings.octaves; o++) {
			sum += amplitude * default(N).GetNoise4(p, hash + o, frequency);
			amplitudeSum += amplitude;
			frequency *= settings.lacunarity;
			amplitude *= settings.persistence;
		}
		float4 noise = sum / amplitudeSum;
		noise *= displacement;

		v.v0.position.y = noise.x;
		v.v1.position.y = noise.y;
		v.v2.position.y = noise.z;
		v.v3.position.y = noise.w;
		vertices[i] = v;
	}

	public static JobHandle ScheduleParallel (
		Mesh.MeshData meshData, int resolution, Settings settings, SpaceTRS domain,
		float displacement,
		JobHandle dependency
	) => new SurfaceJob<N>() {
		vertices =
			meshData.GetVertexData<SingleStream.Stream0>().Reinterpret<Vertex4>(12 * 4),
		settings = settings,
		domainTRS = domain.Matrix,
		displacement = displacement
	}.ScheduleParallel(meshData.vertexCount / 4, resolution, dependency);
}