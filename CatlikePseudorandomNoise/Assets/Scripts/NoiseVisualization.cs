using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Noise;

public class NoiseVisualization : Visualization
{
	public enum NoiseType { Perlin, PerlinTurbulence, Value, ValueTurbulence }
	
    private static int
		noiseId = Shader.PropertyToID("_Noise");

	private static ScheduleDelegate[,] noiseJobs =
	{
		{
			Job<Lattice1D<Perlin>>.ScheduleParallel,
			Job<Lattice2D<Perlin>>.ScheduleParallel,
			Job<Lattice3D<Perlin>>.ScheduleParallel
		},
		{
			Job<Lattice1D<Turbulence<Perlin>>>.ScheduleParallel,
			Job<Lattice2D<Turbulence<Perlin>>>.ScheduleParallel,
			Job<Lattice3D<Turbulence<Perlin>>>.ScheduleParallel
		},
		{
			Job<Lattice1D<Value>>.ScheduleParallel,
			Job<Lattice2D<Value>>.ScheduleParallel,
			Job<Lattice3D<Value>>.ScheduleParallel
		},
		{
			Job<Lattice1D<Turbulence<Value>>>.ScheduleParallel,
			Job<Lattice2D<Turbulence<Value>>>.ScheduleParallel,
			Job<Lattice3D<Turbulence<Value>>>.ScheduleParallel
		}
	};

    [SerializeField] private Settings noiseSettings = Settings.Default;
	[SerializeField] private NoiseType type;
	[SerializeField, Range(1, 3)] private int dimensions = 3;
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
