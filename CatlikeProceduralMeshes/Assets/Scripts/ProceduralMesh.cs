using ProceduralMeshes;
using ProceduralMeshes.Generators;
using ProceduralMeshes.Streams;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour
{
	private static MeshJobScheduleDelegate[] jobs = {
		MeshJob<SquareGrid, SingleStream>.ScheduleParallel,
		MeshJob<SharedSquareGrid, SingleStream>.ScheduleParallel
	};

	public enum MeshType
	{
		SquareGrid, SharedSquareGrid
	};

	[SerializeField] private MeshType meshType;

	[SerializeField, Range(1, 50)] private int resolution = 1;
    private Mesh mesh;

	private void Awake()
    {
		mesh = new Mesh
        {
			name = "Procedural Mesh"
		};
		GetComponent<MeshFilter>().mesh = mesh;
	}

	void OnValidate() => enabled = true;

	void Update()
	{
		GenerateMesh();
		enabled = false;
	}
	
	private void GenerateMesh()
    {
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
		Mesh.MeshData meshData = meshDataArray[0];

		jobs[(int)meshType](mesh, meshData, resolution, default).Complete();

		Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
    }
}
