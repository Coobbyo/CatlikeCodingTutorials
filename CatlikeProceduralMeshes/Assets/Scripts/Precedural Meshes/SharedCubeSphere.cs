using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{
	public struct SharedCubeSphere : IMeshGenerator
    {
        public int VertexCount => 6 * Resolution * Resolution + 2;
		public int IndexCount => 6 * 6 * Resolution * Resolution;
		public int JobLength => 6 * Resolution;
		public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));
		public int Resolution { get; set; }

		private struct Side
		{
			public int id;
			public float3 uvOrigin, uVector, vVector;
		}

		public void Execute<S> (int i, S streams) where S : struct, IMeshStreams
		{
			int u = i / 6;
			Side side = GetSide(i - 6 * u);
			int vi = Resolution * (Resolution * side.id + u) + 2;
			int ti = 2 * Resolution * (Resolution * side.id + u);

			u += 1;

			float3 pStart = side.uvOrigin + side.uVector * u / Resolution;

			var vertex = new Vertex();
			if(i == 0)
			{
				vertex.position = -sqrt(1f / 3f);
				streams.SetVertex(0, vertex);
				vertex.position = sqrt(1f / 3f);
				streams.SetVertex(1, vertex);
			}

			vertex.position = CubeToSphere(pStart);
			streams.SetVertex(vi, vertex);

			streams.SetTriangle(ti + 0, 0);
			streams.SetTriangle(ti + 1, 0);
			vi += 1;
			ti += 2;

			for(int v = 1; v < Resolution; v++, vi++, ti += 2)
			{
				vertex.position = CubeToSphere(pStart + side.vVector * v / Resolution);
				streams.SetVertex(vi, vertex);

				streams.SetTriangle(ti + 0, 0);
				streams.SetTriangle(ti + 1, 0);
			}
		}

		private static Side GetSide(int id) => id switch
		{
			0 => new Side
			{
				id = id,
				uvOrigin = -1f,
				uVector = 2f * right(),
				vVector = 2f * up()
			},
			1 => new Side
			{
				id = id,
				uvOrigin = float3(1f, -1f, -1f),
				uVector = 2f * forward(),
				vVector = 2f * up()
			},
			2 => new Side
			{
				id = id,
				uvOrigin = -1f,
				uVector = 2f * forward(),
				vVector = 2f * right()
			},
			3 => new Side
			{
				id = id,
				uvOrigin = float3(-1f, -1f, 1f),
				uVector = 2f * up(),
				vVector = 2f * right()
			},
			4 => new Side {
				id = id,
				uvOrigin = -1f,
				uVector = 2f * up(),
				vVector = 2f * forward()
			},
			_ => new Side {
				id = id,
				uvOrigin = float3(-1f, 1f, -1f),
				uVector = 2f * right(),
				vVector = 2f * forward()
			}
		};

		static float3 CubeToSphere (float3 p) => p * sqrt(
			1f - ((p * p).yxx + (p * p).zzy) / 2f + (p * p).yxx * (p * p).zzy / 3f
		);
	}
}
