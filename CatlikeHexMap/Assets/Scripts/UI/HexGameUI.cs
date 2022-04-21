using UnityEngine;
using UnityEngine.EventSystems;

public class HexGameUI : MonoBehaviour
{
    private HexCell currentCell;
    private HexUnit selectedUnit;

	[SerializeField] private HexGrid grid;


    private void Update()
    {
		if(!EventSystem.current.IsPointerOverGameObject())
        {
			if(Input.GetMouseButtonDown(0))
            {
				DoSelection();
			}
            else if(selectedUnit)
            {
				if(Input.GetMouseButtonDown(1))
                {
					DoMove();
				}
				else
                {
					DoPathfinding();
				}
			}
		}
	}

    private void DoSelection()
    {
        grid.ClearPath();
		UpdateCurrentCell();
		if(currentCell)
        {
			selectedUnit = currentCell.Unit;
		}
	}

    private void DoPathfinding()
    {
		if(UpdateCurrentCell())
        {
			if(currentCell && selectedUnit.IsValidDestination(currentCell))
            {
				grid.FindPath(selectedUnit.Location, currentCell, 24);
			}
			else
            {
				grid.ClearPath();
			}
		}
	}

    private void DoMove()
    {
		if(grid.HasPath)
        {
			selectedUnit.Location = currentCell;
			grid.ClearPath();
		}
	}

    private bool UpdateCurrentCell()
    {
		HexCell cell =
			grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
		if(cell != currentCell)
        {
			currentCell = cell;
			return true;
		}
		return false;
	}

    public void SetEditMode(bool toggle)
    {
		enabled = !toggle;
		grid.ShowUI(!toggle);
        grid.ClearPath();
	}
}
