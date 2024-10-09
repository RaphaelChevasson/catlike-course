using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators {

	public struct SharedCubeSphere : IMeshGenerator {

		struct Side {
			public int id;
			public float3 uvOrigin, uVector, vVector;
			public int seamStep;

			public bool TouchesMinimumPole => (id & 1) == 0;
		}

		public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));

		public int VertexCount => 6 * Resolution * Resolution + 2;

		public int IndexCount => 6 * 6 * Resolution * Resolution;

		public int JobLength => 6 * Resolution;

		public int Resolution { get; set; }

		public void Execute<S> (int i, S streams) where S : struct, IMeshStreams {
			int u = i / 6;
			Side side = GetSide(i - 6 * u);
			int vi = Resolution * (Resolution * side.id + u) + 2;
			int ti = 2 * Resolution * (Resolution * side.id + u);
			bool firstColumn = u == 0;
			u += 1;

			float3 pStart = side.uvOrigin + side.uVector * u / Resolution;
			
			var vertex = new Vertex();
			if (i == 0) {
				vertex.position = -sqrt(1f / 3f);
				streams.SetVertex(0, vertex);
				vertex.position = sqrt(1f / 3f);
				streams.SetVertex(1, vertex);
			}

			vertex.position = CubeToSphere(pStart);
			streams.SetVertex(vi, vertex);

			var triangle = int3(
				vi,
				firstColumn && side.TouchesMinimumPole ? 0 : vi - Resolution,
				vi + (firstColumn ?
					side.TouchesMinimumPole ?
						side.seamStep * Resolution * Resolution :
						Resolution == 1 ? side.seamStep : -Resolution + 1 :
					-Resolution + 1
				)
			);

			streams.SetTriangle(ti, triangle);
			vi += 1;
			ti += 1;

			int zAdd = firstColumn && side.TouchesMinimumPole ? Resolution : 1;
			int zAddLast = firstColumn && side.TouchesMinimumPole ?
				Resolution :
				!firstColumn && !side.TouchesMinimumPole ?
					Resolution * ((side.seamStep + 1) * Resolution - u) + u :
					(side.seamStep + 1) * Resolution * Resolution - Resolution + 1;

			for (int v = 1; v < Resolution; v++, vi++, ti += 2) {
				vertex.position = CubeToSphere(pStart + side.vVector * v / Resolution);
				streams.SetVertex(vi, vertex);

				triangle.x += 1;
				triangle.y = triangle.z;
				triangle.z += v == Resolution - 1 ? zAddLast : zAdd;
				streams.SetTriangle(ti + 0, int3(triangle.x - 1, triangle.y, triangle.x));
				streams.SetTriangle(ti + 1, triangle);
			}

			streams.SetTriangle(ti, int3(
				triangle.x,
				triangle.z,
				side.TouchesMinimumPole ?
					triangle.z + Resolution :
					u == Resolution ? 1 : triangle.z + 1
			));
		}

		static float3 CubeToSphere (float3 p) => p * sqrt(
			1f - ((p * p).yxx + (p * p).zzy) / 2f + (p * p).yxx * (p * p).zzy / 3f
		);

		static Side GetSide (int id) => id switch {
			0 => new Side {
				id = id,
				uvOrigin = -1f,
				uVector = 2f * right(),
				vVector = 2f * up(),
				seamStep = 4
			},
			1 => new Side {
				id = id,
				uvOrigin = float3(1f, -1f, -1f),
				uVector = 2f * forward(),
				vVector = 2f * up(),
				seamStep = 4
			},
			2 => new Side {
				id = id,
				uvOrigin = -1f,
				uVector = 2f * forward(),
				vVector = 2f * right(),
				seamStep = -2
			},
			3 => new Side {
				id = id,
				uvOrigin = float3(-1f, -1f, 1f),
				uVector = 2f * up(),
				vVector = 2f * right(),
				seamStep = -2
			},
			4 => new Side {
				id = id,
				uvOrigin = -1f,
				uVector = 2f * up(),
				vVector = 2f * forward(),
				seamStep = -2
			},
			_ => new Side {
				id = id,
				uvOrigin = float3(-1f, 1f, -1f),
				uVector = 2f * right(),
				vVector = 2f * forward(),
				seamStep = -2
			}
		};
	}
}