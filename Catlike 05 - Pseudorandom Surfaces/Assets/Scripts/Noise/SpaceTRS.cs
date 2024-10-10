using Unity.Mathematics;

[System.Serializable]
public struct SpaceTRS {

	public float3 translation, rotation, scale;

	public float3x4 Matrix {
		get {
			float3x3 m = math.mul(
				float3x3.Scale(scale), float3x3.EulerZXY(math.radians(rotation))
			);
			return math.float3x4(m.c0, m.c1, m.c2, translation);
		}
	}

	public float3x3 DerivativeMatrix =>
		math.mul(float3x3.EulerYXZ(-math.radians(rotation)), float3x3.Scale(scale));
}