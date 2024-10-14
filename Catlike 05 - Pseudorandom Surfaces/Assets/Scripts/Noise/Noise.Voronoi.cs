using System.Runtime.CompilerServices;
using Unity.Mathematics;

using static Unity.Mathematics.math;

public static partial class Noise {

	public struct VoronoiData {
		public Sample4 a, b;
	}

	public struct Voronoi1D<L, D, F> : INoise
		where L : struct, ILattice
		where D : struct, IVoronoiDistance
		where F : struct, IVoronoiFunction {

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Sample4 GetNoise4 (float4x3 positions, SmallXXHash4 hash, int frequency) {
			var l = default(L);
			var d = default(D);
			LatticeSpan4 x = l.GetLatticeSpan4(positions.c0, frequency);

			VoronoiData data = d.InitialData;
			for (int u = -1; u <= 1; u++) {
				SmallXXHash4 h = hash.Eat(l.ValidateSingleStep(x.p0 + u, frequency));
				data = d.UpdateVoronoiData(data, d.GetDistance(h.Floats01A + u - x.g0));
			}
			Sample4 s = default(F).Evaluate(d.Finalize1D(data));
			s.dx *= frequency;
			return s;
		}
	}

	public struct Voronoi2D<L, D, F> : INoise
		where L : struct, ILattice
		where D : struct, IVoronoiDistance
		where F : struct, IVoronoiFunction {

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Sample4 GetNoise4 (float4x3 positions, SmallXXHash4 hash, int frequency) {
			var l = default(L);
			var d = default(D);
			LatticeSpan4
				x = l.GetLatticeSpan4(positions.c0, frequency),
				z = l.GetLatticeSpan4(positions.c2, frequency);

			VoronoiData data = d.InitialData;
			for (int u = -1; u <= 1; u++) {
				SmallXXHash4 hx = hash.Eat(l.ValidateSingleStep(x.p0 + u, frequency));
				float4 xOffset = u - x.g0;
				for (int v = -1; v <= 1; v++) {
					SmallXXHash4 h = hx.Eat(l.ValidateSingleStep(z.p0 + v, frequency));
					float4 zOffset = v - z.g0;
					data = d.UpdateVoronoiData(data, d.GetDistance(
						h.Floats01A + xOffset, h.Floats01B + zOffset
					));
					data = d.UpdateVoronoiData(data, d.GetDistance(
						h.Floats01C + xOffset, h.Floats01D + zOffset
					));
				}
			}
			Sample4 s = default(F).Evaluate(d.Finalize2D(data));
			s.dx *= frequency;
			s.dz *= frequency;
			return s;
		}
	}

	public struct Voronoi3D<L, D, F> : INoise
		where L : struct, ILattice
		where D : struct, IVoronoiDistance
		where F : struct, IVoronoiFunction {

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Sample4 GetNoise4 (float4x3 positions, SmallXXHash4 hash, int frequency) {
			var l = default(L);
			var d = default(D);
			LatticeSpan4
				x = l.GetLatticeSpan4(positions.c0, frequency),
				y = l.GetLatticeSpan4(positions.c1, frequency),
				z = l.GetLatticeSpan4(positions.c2, frequency);

			VoronoiData data = d.InitialData;
			for (int u = -1; u <= 1; u++) {
				SmallXXHash4 hx = hash.Eat(l.ValidateSingleStep(x.p0 + u, frequency));
				float4 xOffset = u - x.g0;
				for (int v = -1; v <= 1; v++) {
					SmallXXHash4 hy = hx.Eat(l.ValidateSingleStep(y.p0 + v, frequency));
					float4 yOffset = v - y.g0;
					for (int w = -1; w <= 1; w++) {
						SmallXXHash4 h =
							hy.Eat(l.ValidateSingleStep(z.p0 + w, frequency));
						float4 zOffset = w - z.g0;
						data = d.UpdateVoronoiData(data, d.GetDistance(
							h.GetBitsAsFloats01(5, 0) + xOffset,
							h.GetBitsAsFloats01(5, 5) + yOffset,
							h.GetBitsAsFloats01(5, 10) + zOffset
						));
						data = d.UpdateVoronoiData(data, d.GetDistance(
							h.GetBitsAsFloats01(5, 15) + xOffset,
							h.GetBitsAsFloats01(5, 20) + yOffset,
							h.GetBitsAsFloats01(5, 25) + zOffset
						));
					}
				}
			}
			Sample4 s = default(F).Evaluate(d.Finalize3D(data));
			s.dx *= frequency;
			s.dy *= frequency;
			s.dz *= frequency;
			return s;
		}
	}
}