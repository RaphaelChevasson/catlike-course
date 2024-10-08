using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

using quaternion = Unity.Mathematics.quaternion;

namespace ProceduralMeshes.Generators {

	public struct GeoIcosphere : IMeshGenerator {

		struct Strip {
			public int id;
			public float3 lowLeftCorner, lowRightCorner, highLeftCorner, highRightCorner;
			public float3
				bottomLeftAxis, bottomRightAxis,
				midLeftAxis, midCenterAxis, midRightAxis,
				topLeftAxis, topRightAxis;
		}

		public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));

		public int VertexCount => 5 * ResolutionV * Resolution + 2;

		public int IndexCount => 6 * 5 * ResolutionV * Resolution;

		public int JobLength => 5 * Resolution;

		public int Resolution { get; set; }

		int ResolutionV => 2 * Resolution;

		public void Execute<S> (int i, S streams) where S : struct, IMeshStreams {
			int u = i / 5;
			Strip strip = GetStrip(i - 5 * u);
			int vi = ResolutionV * (Resolution * strip.id + u) + 2;
			int ti = 2 * ResolutionV * (Resolution * strip.id + u);
			bool firstColumn = u == 0;

			int4 quad = int4(
				vi,
				firstColumn ? 0 : vi - ResolutionV,
				firstColumn ?
					strip.id == 0 ?
						4 * ResolutionV * Resolution + 2 :
						vi - ResolutionV * (Resolution + u) :
					vi - ResolutionV + 1,
				vi + 1
			);

			u += 1;

			var vertex = new Vertex();
			if (i == 0) {
				vertex.position = down();
				streams.SetVertex(0, vertex);
				vertex.position = up();
				streams.SetVertex(1, vertex);
			}

			vertex.position = mul(
				quaternion.AxisAngle(
					strip.bottomRightAxis, EdgeRotationAngle * u / Resolution
				),
				down()
			);
			streams.SetVertex(vi, vertex);
			vi += 1;

			for (int v = 1; v < ResolutionV; v++, vi++, ti += 2) {
				float h = u + v;
				float3 leftAxis, rightAxis, leftStart, rightStart;
				float edgeAngleScale, faceAngleScale;
				if (v <= Resolution - u) {
					leftAxis = strip.bottomLeftAxis;
					rightAxis = strip.bottomRightAxis;
					leftStart = rightStart = down();
					edgeAngleScale = h / Resolution;
					faceAngleScale = v / h;
				}
				else if (v < Resolution) {
					leftAxis = strip.midCenterAxis;
					rightAxis = strip.midRightAxis;
					leftStart = strip.lowLeftCorner;
					rightStart = strip.lowRightCorner;
					edgeAngleScale = h / Resolution - 1f;
					faceAngleScale = (Resolution - u) / (ResolutionV - h);
				}
				else if (v <= ResolutionV - u) {
					leftAxis = strip.midLeftAxis;
					rightAxis = strip.midCenterAxis;
					leftStart = rightStart = strip.lowLeftCorner;
					edgeAngleScale = h / Resolution - 1f;
					faceAngleScale = (v - Resolution) / (h - Resolution);
				}
				else {
					leftAxis = strip.topLeftAxis;
					rightAxis = strip.topRightAxis;
					leftStart = strip.highLeftCorner;
					rightStart = strip.highRightCorner;
					edgeAngleScale = h / Resolution - 2f;
					faceAngleScale = (Resolution - u) / (3f * Resolution - h);
				}

				float3 pLeft = mul(
					quaternion.AxisAngle(leftAxis, EdgeRotationAngle * edgeAngleScale),
					leftStart
				);
				float3 pRight = mul(
					quaternion.AxisAngle(rightAxis, EdgeRotationAngle * edgeAngleScale),
					rightStart
				);
				float3 axis = normalize(cross(pRight, pLeft));
				float angle = acos(dot(pRight, pLeft)) * faceAngleScale;
				vertex.position = mul(
					quaternion.AxisAngle(axis, angle), pRight
				);
				streams.SetVertex(vi, vertex);
				streams.SetTriangle(ti + 0, quad.xyz);
				streams.SetTriangle(ti + 1, quad.xzw);

				quad.y = quad.z;
				quad +=
					int4(1, 0, firstColumn && v <= Resolution - u ? ResolutionV : 1, 1);
			}

			if (!firstColumn) {
				quad.z = ResolutionV * Resolution * (strip.id == 0 ? 5 : strip.id) -
					Resolution + u + 1;
			}
			quad.w = u < Resolution ? quad.z + 1 : 1;

			streams.SetTriangle(ti + 0, quad.xyz);
			streams.SetTriangle(ti + 1, quad.xzw);
		}

		static Strip GetStrip (int id) => id switch {
			0 => CreateStrip(0),
			1 => CreateStrip(1),
			2 => CreateStrip(2),
			3 => CreateStrip(3),
			_ => CreateStrip(4)
		};

		static Strip CreateStrip (int id) {
			var s = new Strip {
				id = id,
				lowLeftCorner = GetCorner(2 * id, -1),
				lowRightCorner = GetCorner(id == 4 ? 0 : 2 * id + 2, -1),
				highLeftCorner = GetCorner(id == 0 ? 9 : 2 * id - 1, 1),
				highRightCorner = GetCorner(2 * id + 1, 1)
			};
			s.bottomLeftAxis = normalize(cross(down(), s.lowLeftCorner));
			s.bottomRightAxis = normalize(cross(down(), s.lowRightCorner));
			s.midLeftAxis = normalize(cross(s.lowLeftCorner, s.highLeftCorner));
			s.midCenterAxis = normalize(cross(s.lowLeftCorner, s.highRightCorner));
			s.midRightAxis = normalize(cross(s.lowRightCorner, s.highRightCorner));
			s.topLeftAxis = normalize(cross(s.highLeftCorner, up()));
			s.topRightAxis = normalize(cross(s.highRightCorner, up()));
			return s;
		}

		static float3 GetCorner (int id, int ySign) => float3(
			0.4f * sqrt(5f) * sin(0.2f * PI * id),
			ySign * 0.2f * sqrt(5f),
			-0.4f * sqrt(5f) * cos(0.2f * PI * id)
		);

		static float EdgeRotationAngle => acos(dot(up(), GetCorner(0, 1)));
	}
}