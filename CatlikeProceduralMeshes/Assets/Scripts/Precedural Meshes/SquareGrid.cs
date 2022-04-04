using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{
	public struct SquareGrid : IMeshGenerator
    {
        public int VertexCount => 0;
		public int IndexCount => 0;
		public int JobLength => 0;

		public void Execute<S> (int z, S streams) where S : struct, IMeshStreams {}
    }
}
