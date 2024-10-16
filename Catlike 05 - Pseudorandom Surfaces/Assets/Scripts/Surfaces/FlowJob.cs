using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

using static Unity.Mathematics.math;
using static Noise;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = false)]
public struct FlowJob<N> : IJobParticleSystemParallelForBatch where N : struct, INoise {

	Settings settings;

	float3x4 domainTRS;

	float3x3 derivativeMatrix;

	float displacement;

	bool isPlane, isCurl;

	NativeSlice<float4> GetSlice (NativeArray<float> data, int i) =>
		data.Slice(i, 4).SliceConvert<float4>();

	public void Execute (ParticleSystemJobData data, int startIndex, int count) {
		if (count != 4) {
			return;
		}

		NativeSlice<float4>
			px = GetSlice(data.positions.x, startIndex),
			py = GetSlice(data.positions.y, startIndex),
			pz = GetSlice(data.positions.z, startIndex);

		NativeSlice<float4>
			vx = GetSlice(data.velocities.x, startIndex),
			vy = GetSlice(data.velocities.y, startIndex),
			vz = GetSlice(data.velocities.z, startIndex);

		NativeSlice<float4> life = GetSlice(data.aliveTimePercent, startIndex);

		float4x3 p = float4x3(px[0], py[0], pz[0]);

		if (isPlane) {
			p.c1 = 0f;
			life[0] = select(life[0], 100f, abs(p.c0) > 0.5f | abs(p.c2) > 0.5f);
		}
		else {
			p = p.NormalizeRows();
		}

		Sample4 noise = GetFractalNoise<N>(
			domainTRS.TransformVectors(p), settings
		) * displacement;

		noise.Derivatives = derivativeMatrix.TransformVectors(noise.Derivatives);

		if (isPlane) {
			if (isCurl) {
				vx[0] = noise.dz;
				vz[0] = -noise.dx;
			}
			else {
				vx[0] = -noise.dx;
				vz[0] = -noise.dz;
			}

			py[0] = noise.v + 0.05f;
		}
		else {
			noise.v += 1f;
			noise.dx /= noise.v;
			noise.dy /= noise.v;
			noise.dz /= noise.v;

			float4 pd = p.c0 * noise.dx + p.c1 * noise.dy + p.c2 * noise.dz;
			noise.dx -= pd * p.c0;
			noise.dy -= pd * p.c1;
			noise.dz -= pd * p.c2;

			if (isCurl) {
				vx[0] = p.c1 * noise.dz - p.c2 * noise.dy;
				vy[0] = p.c2 * noise.dx - p.c0 * noise.dz;
				vz[0] = p.c0 * noise.dy - p.c1 * noise.dx;
			}
			else {
				vx[0] = -noise.dx;
				vy[0] = -noise.dy;
				vz[0] = -noise.dz;
			}

			noise.v += 0.05f;
			px[0] = p.c0 * noise.v;
			py[0] = p.c1 * noise.v;
			pz[0] = p.c2 * noise.v;
		}
	}

	public static JobHandle ScheduleParallel (
		ParticleSystem system,
		Settings settings, SpaceTRS domain, float displacement, bool isPlane, bool isCurl
	) => new FlowJob<N>() {
		settings = settings,
		domainTRS = domain.Matrix,
		derivativeMatrix = domain.DerivativeMatrix,
		displacement = displacement,
		isPlane = isPlane,
		isCurl = isCurl
	}.ScheduleBatch(system, 4);
}

public delegate JobHandle FlowJobScheduleDelegate (
	ParticleSystem system,
	Settings settings, SpaceTRS domain, float displacement, bool isPlane, bool isCurl
);
