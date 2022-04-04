using ProceduralMeshes;
using ProceduralMeshes.Generators;
using ProceduralMeshes.Streams;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour
{
	[SerializeField, Range(1, 10)] private int resolution = 1;
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

		MeshJob<SquareGrid, MultiStream>.ScheduleParallel(
			mesh, meshData, resolution, default
		).Complete();

		Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
    }
}
