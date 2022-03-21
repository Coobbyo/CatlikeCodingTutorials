using UnityEngine;
using TMPro;
public class HexGrid : MonoBehaviour
{
	[SerializeField] private int width = 6;
	[SerializeField] private int height = 6;

	[SerializeField] private HexCell cellPrefab;
    private HexCell[] cells;

    [SerializeField] private TMP_Text cellLabelPrefab;
	private Canvas gridCanvas;

	private HexMesh hexMesh;
	private MeshCollider meshCollider;

	[SerializeField] private Color defaultColor = Color.white;
	[SerializeField] private Color touchedColor = Color.magenta;

	private void Awake()
    {
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
	private void Update()
	{
		if(Input.GetMouseButton(0))
		{
			HandleInput();
		}
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
		
		TMP_Text label = Instantiate<TMP_Text>(cellLabelPrefab);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition =
			new Vector2(position.x, position.z);
		label.text = cell.coordinates.ToStringOnSeparateLines();
	}
	void HandleInput()
	{
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if(Physics.Raycast(inputRay, out hit))
		{
			TouchCell(hit.point);
		}
	}
	void TouchCell(Vector3 position)
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
		HexCell cell = cells[index];
		cell.color = touchedColor;
		hexMesh.Triangulate(cells);
	}
}
