using Unity.Mathematics;

public static partial class Noise {

	public interface IVoronoiFunction {
		Sample4 Evaluate (VoronoiData data);
	}

	public struct F1 : IVoronoiFunction {

		public Sample4 Evaluate (VoronoiData data) => data.a;
	}

	public struct F2 : IVoronoiFunction {

		public Sample4 Evaluate (VoronoiData data) => data.b;
	}

	public struct F2MinusF1 : IVoronoiFunction {

		public Sample4 Evaluate (VoronoiData data) => data.b - data.a;
	}
}