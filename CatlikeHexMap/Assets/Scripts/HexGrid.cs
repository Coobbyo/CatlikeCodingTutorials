using UnityEngine;
using TMPro;
public class HexGrid : MonoBehaviour
{
	[SerializeField] private int width = 6;
	[SerializeField] private int height = 6;

	[SerializeField] private HexCell cellPrefab;
	[SerializeField] private Texture2D noiseSource;
    private HexCell[] cells;

    [SerializeField] private TMP_Text cellLabelPrefab;
	private Canvas gridCanvas;

	private HexMesh hexMesh;
	private MeshCollider meshCollider;

	[SerializeField] private Color defaultColor = Color.white;

	private void Awake()
    {
		HexMetrics.noiseSource = noiseSource;
		meshCollider = gameObject.AddComponent<MeshCollider>();
        gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh>();
		cells = new HexCell[height * width];

		for(int z = 0, i = 0; z < height; z++)
        {
			for(int x = 0; x < width; x++)
            {
				CreateCell(x, z, i++);
			}
		}
	}

	private void Start()
	{
		hexMesh.Triangulate(cells);
	}

	void OnEnable()
	{
		HexMetrics.noiseSource = noiseSource;
	}
	private void CreateCell(int x, int z, int i)
    {
		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
		position.y = 0f;
		position.z = z * (HexMetrics.outerRadius * 1.5f);

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.color = defaultColor;

		if(x > 0)
		{
			cell.SetNeighbor(HexDirection.W, cells[i - 1]);
		}
		if(z > 0)
		{
			if((z & 1) == 0)
			{
				cell.SetNeighbor(HexDirection.SE, cells[i - width]);
				if(x > 0)
					cell.SetNeighbor(HexDirection.SW, cells[i - width - 1]);
			}
			else
			{
				cell.SetNeighbor(HexDirection.SW, cells[i - width]);
				if(x < width - 1)
					cell.SetNeighbor(HexDirection.SE, cells[i - width + 1]);
			}
		}
		
		TMP_Text label = Instantiate<TMP_Text>(cellLabelPrefab);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition =
			new Vector2(position.x, position.z);
		label.text = cell.coordinates.ToStringOnSeparateLines();
		cell.uiRect = label.rectTransform;
	}

	public HexCell GetCell (Vector3 position)
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
		return cells[index];
	}

	public void Refresh()
	{
		hexMesh.Triangulate(cells);
	}
}
