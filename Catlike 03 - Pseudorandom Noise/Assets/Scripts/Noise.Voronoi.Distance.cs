using Unity.Mathematics;

using static Unity.Mathematics.math;

public static partial class Noise {

	public interface IVoronoiDistance {
		float4 GetDistance (float4 x);

		float4 GetDistance (float4 x, float4 y);

		float4 GetDistance (float4 x, float4 y, float4 z);

		float4x2 Finalize1D (float4x2 minima);

		float4x2 Finalize2D (float4x2 minima);

		float4x2 Finalize3D (float4x2 minima);
	}

	public struct Worley : IVoronoiDistance {

		public float4 GetDistance (float4 x) => abs(x);

		public float4 GetDistance (float4 x, float4 y) => x * x + y * y;

		public float4 GetDistance (float4 x, float4 y, float4 z) => x * x + y * y + z * z;

		public float4x2 Finalize1D (float4x2 minima) => minima;

		public float4x2 Finalize2D (float4x2 minima) {
			minima.c0 = sqrt(min(minima.c0, 1f));
			minima.c1 = sqrt(min(minima.c1, 1f));
			return minima;
		}

		public float4x2 Finalize3D (float4x2 minima) => Finalize2D(minima);
	}

	public struct Chebyshev : IVoronoiDistance {

		public float4 GetDistance (float4 x) => abs(x);

		public float4 GetDistance (float4 x, float4 y) => max(abs(x), abs(y));

		public float4 GetDistance (float4 x, float4 y, float4 z) =>
			max(max(abs(x), abs(y)), abs(z));

		public float4x2 Finalize1D (float4x2 minima) => minima;

		public float4x2 Finalize2D (float4x2 minima) => minima;

		public float4x2 Finalize3D (float4x2 minima) => minima;
	}
}