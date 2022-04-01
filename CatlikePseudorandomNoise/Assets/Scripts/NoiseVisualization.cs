using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Noise;

public class NoiseVisualization : Visualization
{
	public enum NoiseType
	{
		Perlin, PerlinTurbulence, Value, ValueTurbulence,
		Simplex, SimplexTurbulence, SimplexValue, SimplexValueTurbulence,
		VoronoiWorleyF1, VoronoiWorleyF2, VoronoiWorleyF2MinusF1,
		VoronoiChebyshevF1, VoronoiChebyshevF2, VoronoiChebyshevF2MinusF1
	}
	
    private static int
		noiseId = Shader.PropertyToID("_Noise");

	private static ScheduleDelegate[,] noiseJobs =
	{
		//Perlin
		{
			Job<Lattice1D<LatticeNormal, Perlin>>.ScheduleParallel,
			Job<Lattice1D<LatticeTiling, Perlin>>.ScheduleParallel,
			Job<Lattice2D<LatticeNormal, Perlin>>.ScheduleParallel,
			Job<Lattice2D<LatticeTiling, Perlin>>.ScheduleParallel,
			Job<Lattice3D<LatticeNormal, Perlin>>.ScheduleParallel,
			Job<Lattice3D<LatticeTiling, Perlin>>.ScheduleParallel
		},
		//Perlin Turbulence
		{
			Job<Lattice1D<LatticeNormal, Turbulence<Perlin>>>.ScheduleParallel,
			Job<Lattice1D<LatticeTiling, Turbulence<Perlin>>>.ScheduleParallel,
			Job<Lattice2D<LatticeNormal, Turbulence<Perlin>>>.ScheduleParallel,
			Job<Lattice2D<LatticeTiling, Turbulence<Perlin>>>.ScheduleParallel,
			Job<Lattice3D<LatticeNormal, Turbulence<Perlin>>>.ScheduleParallel,
			Job<Lattice3D<LatticeTiling, Turbulence<Perlin>>>.ScheduleParallel
		},
		//Value
		{
			Job<Lattice1D<LatticeNormal, Value>>.ScheduleParallel,
			Job<Lattice1D<LatticeTiling, Value>>.ScheduleParallel,
			Job<Lattice2D<LatticeNormal, Value>>.ScheduleParallel,
			Job<Lattice2D<LatticeTiling, Value>>.ScheduleParallel,
			Job<Lattice3D<LatticeNormal, Value>>.ScheduleParallel,
			Job<Lattice3D<LatticeTiling, Value>>.ScheduleParallel
		},
		//Value Turbulence
		{
			Job<Lattice1D<LatticeNormal, Turbulence<Value>>>.ScheduleParallel,
			Job<Lattice1D<LatticeTiling, Turbulence<Value>>>.ScheduleParallel,
			Job<Lattice2D<LatticeNormal, Turbulence<Value>>>.ScheduleParallel,
			Job<Lattice2D<LatticeTiling, Turbulence<Value>>>.ScheduleParallel,
			Job<Lattice3D<LatticeNormal, Turbulence<Value>>>.ScheduleParallel,
			Job<Lattice3D<LatticeTiling, Turbulence<Value>>>.ScheduleParallel
		},
		//Simplex
		{
			Job<Simplex1D<Simplex>>.ScheduleParallel,
			Job<Simplex1D<Simplex>>.ScheduleParallel,
			Job<Simplex2D<Simplex>>.ScheduleParallel,
			Job<Simplex2D<Simplex>>.ScheduleParallel,
			Job<Simplex3D<Simplex>>.ScheduleParallel,
			Job<Simplex3D<Simplex>>.ScheduleParallel
		},
		//Simplex Turbulence
		{
			Job<Simplex1D<Turbulence<Simplex>>>.ScheduleParallel,
			Job<Simplex1D<Turbulence<Simplex>>>.ScheduleParallel,
			Job<Simplex2D<Turbulence<Simplex>>>.ScheduleParallel,
			Job<Simplex2D<Turbulence<Simplex>>>.ScheduleParallel,
			Job<Simplex3D<Turbulence<Simplex>>>.ScheduleParallel,
			Job<Simplex3D<Turbulence<Simplex>>>.ScheduleParallel
		},
		//Simplex Value
		{
			Job<Simplex1D<Value>>.ScheduleParallel,
			Job<Simplex1D<Value>>.ScheduleParallel,
			Job<Simplex2D<Value>>.ScheduleParallel,
			Job<Simplex2D<Value>>.ScheduleParallel,
			Job<Simplex3D<Value>>.ScheduleParallel,
			Job<Simplex3D<Value>>.ScheduleParallel
		},
		//Simplex Value Turbulence
		{
			Job<Simplex1D<Turbulence<Value>>>.ScheduleParallel,
			Job<Simplex1D<Turbulence<Value>>>.ScheduleParallel,
			Job<Simplex2D<Turbulence<Value>>>.ScheduleParallel,
			Job<Simplex2D<Turbulence<Value>>>.ScheduleParallel,
			Job<Simplex3D<Turbulence<Value>>>.ScheduleParallel,
			Job<Simplex3D<Turbulence<Value>>>.ScheduleParallel
		},
		//Voronoi Worley F1
		{
			Job<Voronoi1D<LatticeNormal, Worley, F1>>.ScheduleParallel,
			Job<Voronoi1D<LatticeTiling, Worley, F1>>.ScheduleParallel,
			Job<Voronoi2D<LatticeNormal, Worley, F1>>.ScheduleParallel,
			Job<Voronoi2D<LatticeTiling, Worley, F1>>.ScheduleParallel,
			Job<Voronoi3D<LatticeNormal, Worley, F1>>.ScheduleParallel,
			Job<Voronoi3D<LatticeTiling, Worley, F1>>.ScheduleParallel
		},
		//Voronoi Worley F2
		{
			Job<Voronoi1D<LatticeNormal, Worley, F2>>.ScheduleParallel,
			Job<Voronoi1D<LatticeTiling, Worley, F2>>.ScheduleParallel,
			Job<Voronoi2D<LatticeNormal, Worley, F2>>.ScheduleParallel,
			Job<Voronoi2D<LatticeTiling, Worley, F2>>.ScheduleParallel,
			Job<Voronoi3D<LatticeNormal, Worley, F2>>.ScheduleParallel,
			Job<Voronoi3D<LatticeTiling, Worley, F2>>.ScheduleParallel
		},
		//Voronoi Worley F2 Minus F1
		{
			Job<Voronoi1D<LatticeNormal, Worley, F2MinusF1>>.ScheduleParallel,
			Job<Voronoi1D<LatticeTiling, Worley, F2MinusF1>>.ScheduleParallel,
			Job<Voronoi2D<LatticeNormal, Worley, F2MinusF1>>.ScheduleParallel,
			Job<Voronoi2D<LatticeTiling, Worley, F2MinusF1>>.ScheduleParallel,
			Job<Voronoi3D<LatticeNormal, Worley, F2MinusF1>>.ScheduleParallel,
			Job<Voronoi3D<LatticeTiling, Worley, F2MinusF1>>.ScheduleParallel
		},
		//Voronoi Chebyshev F1
		{
			Job<Voronoi1D<LatticeNormal, Worley, F1>>.ScheduleParallel,
			Job<Voronoi1D<LatticeTiling, Worley, F1>>.ScheduleParallel,
			Job<Voronoi2D<LatticeNormal, Chebyshev, F1>>.ScheduleParallel,
			Job<Voronoi2D<LatticeTiling, Chebyshev, F1>>.ScheduleParallel,
			Job<Voronoi3D<LatticeNormal, Chebyshev, F1>>.ScheduleParallel,
			Job<Voronoi3D<LatticeTiling, Chebyshev, F1>>.ScheduleParallel
		},
		//Voronoi Chebyshev F2
		{
			Job<Voronoi1D<LatticeNormal, Worley, F2>>.ScheduleParallel,
			Job<Voronoi1D<LatticeTiling, Worley, F2>>.ScheduleParallel,
			Job<Voronoi2D<LatticeNormal, Chebyshev, F2>>.ScheduleParallel,
			Job<Voronoi2D<LatticeTiling, Chebyshev, F2>>.ScheduleParallel,
			Job<Voronoi3D<LatticeNormal, Chebyshev, F2>>.ScheduleParallel,
			Job<Voronoi3D<LatticeTiling, Chebyshev, F2>>.ScheduleParallel
		},
		//Voronoi Chebyshev F2 Minus F1
		{
			Job<Voronoi1D<LatticeNormal, Worley, F2MinusF1>>.ScheduleParallel,
			Job<Voronoi1D<LatticeTiling, Worley, F2MinusF1>>.ScheduleParallel,
			Job<Voronoi2D<LatticeNormal, Chebyshev, F2MinusF1>>.ScheduleParallel,
			Job<Voronoi2D<LatticeTiling, Chebyshev, F2MinusF1>>.ScheduleParallel,
			Job<Voronoi3D<LatticeNormal, Chebyshev, F2MinusF1>>.ScheduleParallel,
			Job<Voronoi3D<LatticeTiling, Chebyshev, F2MinusF1>>.ScheduleParallel
		}
	};

    [SerializeField] private Settings noiseSettings = Settings.Default;
	[SerializeField] private NoiseType type;
	[SerializeField, Range(1, 3)] private int dimensions = 3;
	[SerializeField] private bool tiling;
	[SerializeField] private SpaceTRS domain = new SpaceTRS
	{
		scale = 8f
	};

	private NativeArray<float4> noise;
	private ComputeBuffer noiseBuffer;

    protected override void EnableVisualization(int dataLength, MaterialPropertyBlock propertyBlock)
    {
		noise = new NativeArray<float4>(dataLength, Allocator.Persistent);
		noiseBuffer = new ComputeBuffer(dataLength * 4, 4);
		propertyBlock.SetBuffer(noiseId, noiseBuffer);
	}

    protected override void DisableVisualization()
	{
		noise.Dispose();
		noiseBuffer.Release();
		noiseBuffer = null;
	}

    protected override void UpdateVisualization(
		NativeArray<float3x4> positions, int resolution, JobHandle handle)
    {
		noiseJobs[(int)type, dimensions - 1](
			positions, noise, noiseSettings, domain, resolution, handle
		).Complete();
		noiseBuffer.SetData(noise.Reinterpret<float>(4 * 4));
	}
}
