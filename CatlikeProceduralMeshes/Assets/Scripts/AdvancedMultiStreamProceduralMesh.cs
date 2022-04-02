using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AdvancedMultiStreamProceduralMesh : MonoBehaviour
{

	private void OnEnable()
    {
		var mesh = new Mesh
        {
			name = "Procedural Mesh"
		};

		GetComponent<MeshFilter>().mesh = mesh;
	}
}
