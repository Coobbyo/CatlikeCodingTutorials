using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
    private Mesh deformingMesh;
	private Vector3[] originalVertices, displacedVertices;
    Vector3[] vertexVelocities;

    private void Start()
    {
		deformingMesh = GetComponent<MeshFilter>().mesh;
		originalVertices = deformingMesh.vertices;
		displacedVertices = new Vector3[originalVertices.Length];
		for(int i = 0; i < originalVertices.Length; i++)
        {
			displacedVertices[i] = originalVertices[i];
		}
        vertexVelocities = new Vector3[originalVertices.Length];
	}
}
