using Unity.Mathematics;

using static Unity.Mathematics.math;

public static partial class Noise {

	public struct Sample4 {

		public float4 v, dx, dy, dz;

		public float4x3 Derivatives => float4x3(dx, dy, dz);

		public static implicit operator Sample4 (float4 v) => new Sample4 { v = v };

		public static Sample4 operator + (Sample4 a, Sample4 b) => new Sample4 {
			v = a.v + b.v,
			dx = a.dx + b.dx,
			dy = a.dy + b.dy,
			dz = a.dz + b.dz
		};

		public static Sample4 operator - (Sample4 a, Sample4 b) => new Sample4 {
			v = a.v - b.v,
			dx = a.dx - b.dx,
			dy = a.dy - b.dy,
			dz = a.dz - b.dz
		};

		public static Sample4 operator * (Sample4 a, float4 b) => new Sample4 {
			v = a.v * b,
			dx = a.dx * b,
			dy = a.dy * b,
			dz = a.dz * b
		};

		public static Sample4 operator * (float4 a, Sample4 b) => b * a;

		public static Sample4 operator / (Sample4 a, float4 b) => new Sample4 {
			v = a.v / b,
			dx = a.dx / b,
			dy = a.dy / b,
			dz = a.dz / b
		};
	}
}