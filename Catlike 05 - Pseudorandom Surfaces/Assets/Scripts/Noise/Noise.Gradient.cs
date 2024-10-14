using Unity.Mathematics;

using static Unity.Mathematics.math;

public static partial class Noise {

	public interface IGradient {
		Sample4 Evaluate (SmallXXHash4 hash, float4 x);

		Sample4 Evaluate (SmallXXHash4 hash, float4 x, float4 y);

		Sample4 Evaluate (SmallXXHash4 hash, float4 x, float4 y, float4 z);

		Sample4 EvaluateCombined (Sample4 value);
	}

	public struct Smoothstep<G> : IGradient where G : struct, IGradient {

		public Sample4 Evaluate (SmallXXHash4 hash, float4 x) =>
			default(G).Evaluate(hash, x);

		public Sample4 Evaluate (SmallXXHash4 hash, float4 x, float4 y) =>
			default(G).Evaluate(hash, x, y);

		public Sample4 Evaluate (SmallXXHash4 hash, float4 x, float4 y, float4 z) =>
			default(G).Evaluate(hash, x, y, z);

		public Sample4 EvaluateCombined (Sample4 value) =>
			default(G).EvaluateCombined(value).Smoothstep;
	}

	public struct Turbulence<G> : IGradient where G : struct, IGradient {

		public Sample4 Evaluate (SmallXXHash4 hash, float4 x) =>
			default(G).Evaluate(hash, x);

		public Sample4 Evaluate (SmallXXHash4 hash, float4 x, float4 y) =>
			default(G).Evaluate(hash, x, y);

		public Sample4 Evaluate (SmallXXHash4 hash, float4 x, float4 y, float4 z) =>
			default(G).Evaluate(hash, x, y, z);

		public Sample4 EvaluateCombined (Sample4 value) {
			Sample4 s = default(G).EvaluateCombined(value);
			s.dx = select(-s.dx, s.dx, s.v >= 0f);
			s.dy = select(-s.dy, s.dy, s.v >= 0f);
			s.dz = select(-s.dz, s.dz, s.v >= 0f);
			s.v = abs(s.v);
			return s;
		}
	}

	public struct Value : IGradient {

		public Sample4 Evaluate (SmallXXHash4 hash, float4 x) => hash.Floats01A * 2f - 1f;

		public Sample4 Evaluate (SmallXXHash4 hash, float4 x, float4 y) =>
			hash.Floats01A * 2f - 1f;

		public Sample4 Evaluate (SmallXXHash4 hash, float4 x, float4 y, float4 z) =>
			hash.Floats01A * 2f - 1f;

		public Sample4 EvaluateCombined (Sample4 value) => value;
	}

	public struct Perlin : IGradient {

		public Sample4 Evaluate (SmallXXHash4 hash, float4 x) =>
			BaseGradients.Line(hash, x);

		public Sample4 Evaluate (SmallXXHash4 hash, float4 x, float4 y) =>
			BaseGradients.Square(hash, x, y) * (2f / 0.53528f);

		public Sample4 Evaluate (SmallXXHash4 hash, float4 x, float4 y, float4 z) =>
			BaseGradients.Octahedron(hash, x, y, z) * (1f / 0.56290f);

		public Sample4 EvaluateCombined (Sample4 value) => value;
	}

	public struct Simplex : IGradient {

		public Sample4 Evaluate (SmallXXHash4 hash, float4 x) =>
			BaseGradients.Line(hash, x) * (32f / 27f);

		public Sample4 Evaluate (SmallXXHash4 hash, float4 x, float4 y) =>
			BaseGradients.Circle(hash, x, y) * (5.832f / sqrt(2f));

		public Sample4 Evaluate (SmallXXHash4 hash, float4 x, float4 y, float4 z) =>
			BaseGradients.Sphere(hash, x, y, z) * (1024f / (125f * sqrt(3f)));

		public Sample4 EvaluateCombined (Sample4 value) => value;
	}

	public static class BaseGradients {

		public static Sample4 Line (SmallXXHash4 hash, float4 x) {
			float4 l =
				(1f + hash.Floats01A) * select(-1f, 1f, ((uint4)hash & 1 << 8) == 0);
			return new Sample4 {
				v = l * x,
				dx = l
			};
		}

		public static Sample4 Square (SmallXXHash4 hash, float4 x, float4 y) {
			float4x2 v = SquareVectors(hash);
			return new Sample4 {
				v = v.c0 * x + v.c1 * y,
				dx = v.c0,
				dz = v.c1
			};
		}

		public static Sample4 Circle (SmallXXHash4 hash, float4 x, float4 y) {
			float4x2 v = SquareVectors(hash);
			return new Sample4 {
				v = v.c0 * x + v.c1 * y,
				dx = v.c0,
				dz = v.c1
			} * rsqrt(v.c0 * v.c0 + v.c1 * v.c1);
		}

		public static Sample4 Octahedron (
			SmallXXHash4 hash, float4 x, float4 y, float4 z
		) {
			float4x3 v = OctahedronVectors(hash);
			return new Sample4 {
				v = v.c0 * x + v.c1 * y + v.c2 * z,
				dx = v.c0,
				dy = v.c1,
				dz = v.c2
			};
		}

		public static Sample4 Sphere (SmallXXHash4 hash, float4 x, float4 y, float4 z) {
			float4x3 v = OctahedronVectors(hash);
			return new Sample4 {
				v = v.c0 * x + v.c1 * y + v.c2 * z,
				dx = v.c0,
				dy = v.c1,
				dz = v.c2
			} * rsqrt(v.c0 * v.c0 + v.c1 * v.c1 + v.c2 * v.c2);
		}

		static float4x2 SquareVectors (SmallXXHash4 hash) {
			float4x2 v;
			v.c0 = hash.Floats01A * 2f - 1f;
			v.c1 = 0.5f - abs(v.c0);
			v.c0 -= floor(v.c0 + 0.5f);
			return v;
		}

		static float4x3 OctahedronVectors (SmallXXHash4 hash) {
			float4x3 g;
			g.c0 = hash.Floats01A * 2f - 1f;
			g.c1 = hash.Floats01D * 2f - 1f;
			g.c2 = 1f - abs(g.c0) - abs(g.c1);
			float4 offset = max(-g.c2, 0f);
			g.c0 += select(-offset, offset, g.c0 < 0f);
			g.c1 += select(-offset, offset, g.c1 < 0f);
			return g;
		}
	}
}