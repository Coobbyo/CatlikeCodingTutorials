using System.Collections.Generic;
using UnityEngine;

public class HexMapGenerator : MonoBehaviour
{
	[SerializeField] private HexGrid grid;
    [SerializeField] private bool useFixedSeed;
    [SerializeField] private int seed;
    [SerializeField, Range(0f, 0.5f)] private float jitterProbability = 0.25f;
    [SerializeField, Range(20, 200)] private int chunkSizeMin = 30;
	[SerializeField, Range(20, 200)] private int chunkSizeMax = 100;
    [SerializeField, Range(0f, 1f)] private float highRiseProbability = 0.25f;
    [SerializeField, Range(0f, 0.4f)] private float sinkProbability = 0.2f;
    [SerializeField, Range(5, 95)] private int landPercentage = 50;
    [SerializeField, Range(1, 5)] private int waterLevel = 3;
    [SerializeField, Range(-4, 0)] private int elevationMinimum = -2;
	[SerializeField, Range(6, 10)] private int elevationMaximum = 8;
	[SerializeField, Range(0, 10)] public int mapBorderX = 5;
	[SerializeField, Range(0, 10)] private int mapBorderZ = 5;
	[SerializeField, Range(0, 10)] private int regionBorder = 5;
	[SerializeField, Range(1, 4)] private int regionCount = 1;
	[SerializeField, Range(0, 100)] private int erosionPercentage = 50;

    private HexCellPriorityQueue searchFrontier;
    private int searchFrontierPhase;
    private int cellCount;

	private struct MapRegion
	{
		public int xMin, xMax, zMin, zMax;
	}
	List<MapRegion> regions;


	public void GenerateMap(int x, int z)
    {
        Random.State originalRandomState = Random.state;
        if(!useFixedSeed)
        {
            seed = Random.Range(0, int.MaxValue);
            seed ^= (int)System.DateTime.Now.Ticks;
            seed ^= (int)Time.unscaledTime;
            seed &= int.MaxValue;
        }
		Random.InitState(seed);

        cellCount = x * z;
		grid.CreateMap(x, z);
        if(searchFrontier == null)
			searchFrontier = new HexCellPriorityQueue();

        for(int i = 0; i < cellCount; i++)
        {
			grid.GetCell(i).WaterLevel = waterLevel;
		}

		CreateRegions();
        CreateLand();
		ErodeLand();
        SetTerrainType();

        for(int i = 0; i < cellCount; i++)
        {
			grid.GetCell(i).SearchPhase = 0;
		}

        Random.state = originalRandomState;
	}

    private void CreateLand()
    {
		int landBudget = Mathf.RoundToInt(cellCount * landPercentage * 0.01f);
        for(int guard = 0; guard < 10000; guard++)
        {
			bool sink = Random.value < sinkProbability;
			for(int i = 0; i < regions.Count; i++)
			{
				MapRegion region = regions[i];
				int chunkSize = Random.Range(chunkSizeMin, chunkSizeMax - 1);
				if(sink)
				{
					landBudget = SinkTerrain(chunkSize, landBudget, region);
				}
				else
				{
					landBudget = RaiseTerrain(chunkSize, landBudget, region);
					if(landBudget == 0)
						return;
				}
			}
		}

		if(landBudget > 0)
			Debug.LogWarning("Failed to use up " + landBudget + " land budget.");
	}

	void CreateRegions()
	{
		if(regions == null)
		{
			regions = new List<MapRegion>();
		}
		else
		{
			regions.Clear();
		}

		MapRegion region;
		switch(regionCount)
		{
			//default: CreateOneRegion(); break;
			//case 2: CreateTwoRegions(); break;
			//case 3: CreateThreeRegions(); break;
			//case 4: CreateFourRegions(); break;
			default:
				region.xMin = mapBorderX;
				region.xMax = grid.cellCountX - mapBorderX;
				region.zMin = mapBorderZ;
				region.zMax = grid.cellCountZ - mapBorderZ;
				regions.Add(region);
				break;
			case 2:
				if(Random.value < 0.5f)
				{
					region.xMin = mapBorderX;
					region.xMax = grid.cellCountX / 2 - regionBorder;
					region.zMin = mapBorderZ;
					region.zMax = grid.cellCountZ - mapBorderZ;
					regions.Add(region);
					region.xMin = grid.cellCountX / 2 + regionBorder;
					region.xMax = grid.cellCountX - mapBorderX;
					regions.Add(region);
				}
				else
				{
					region.xMin = mapBorderX;
					region.xMax = grid.cellCountX - mapBorderX;
					region.zMin = mapBorderZ;
					region.zMax = grid.cellCountZ / 2 - regionBorder;
					regions.Add(region);
					region.zMin = grid.cellCountZ / 2 + regionBorder;
					region.zMax = grid.cellCountZ - mapBorderZ;
					regions.Add(region);
				}
				break;
			case 3:
				region.xMin = mapBorderX;
				region.xMax = grid.cellCountX / 3 - regionBorder;
				region.zMin = mapBorderZ;
				region.zMax = grid.cellCountZ - mapBorderZ;
				regions.Add(region);
				region.xMin = grid.cellCountX / 3 + regionBorder;
				region.xMax = grid.cellCountX * 2 / 3 - regionBorder;
				regions.Add(region);
				region.xMin = grid.cellCountX * 2 / 3 + regionBorder;
				region.xMax = grid.cellCountX - mapBorderX;
				regions.Add(region);
				break;
			case 4:
				region.xMin = mapBorderX;
				region.xMax = grid.cellCountX / 2 - regionBorder;
				region.zMin = mapBorderZ;
				region.zMax = grid.cellCountZ / 2 - regionBorder;
				regions.Add(region);
				region.xMin = grid.cellCountX / 2 + regionBorder;
				region.xMax = grid.cellCountX - mapBorderX;
				regions.Add(region);
				region.zMin = grid.cellCountZ / 2 + regionBorder;
				region.zMax = grid.cellCountZ - mapBorderZ;
				regions.Add(region);
				region.xMin = mapBorderX;
				region.xMax = grid.cellCountX / 2 - regionBorder;
				regions.Add(region);
				break;
		}
	}

    private int RaiseTerrain(int chunkSize, int budget, MapRegion region)
    {
        searchFrontierPhase += 1;
		HexCell firstCell = GetRandomCell(region);
		firstCell.SearchPhase = searchFrontierPhase;
		firstCell.Distance = 0;
		firstCell.SearchHeuristic = 0;
		searchFrontier.Enqueue(firstCell);
        HexCoordinates center = firstCell.coordinates;

        int rise = Random.value < highRiseProbability ? 2 : 1;
        int size = 0;
		while(size < chunkSize && searchFrontier.Count > 0)
        {
			HexCell current = searchFrontier.Dequeue();
            int originalElevation = current.Elevation;
            int newElevation = originalElevation + rise;
			if(newElevation > elevationMaximum)
				continue;

			current.Elevation = newElevation;
            if(originalElevation < waterLevel &&
				newElevation >= waterLevel && --budget == 0)
				break;
            
			size += 1;

			for(HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
				HexCell neighbor = current.GetNeighbor(d);
				if(neighbor && neighbor.SearchPhase < searchFrontierPhase)
                {
					neighbor.SearchPhase = searchFrontierPhase;
					neighbor.Distance = neighbor.coordinates.DistanceTo(center);
					neighbor.SearchHeuristic = Random.value < jitterProbability ? 1: 0; //Higher Huristics can be used for more ribony terrain
					searchFrontier.Enqueue(neighbor);
				}
			}
		}

		searchFrontier.Clear();
        return budget;
    }

    private int SinkTerrain(int chunkSize, int budget, MapRegion region)
    {
        searchFrontierPhase += 1;
		HexCell firstCell = GetRandomCell(region);
		firstCell.SearchPhase = searchFrontierPhase;
		firstCell.Distance = 0;
		firstCell.SearchHeuristic = 0;
		searchFrontier.Enqueue(firstCell);
        HexCoordinates center = firstCell.coordinates;

        int sink = Random.value < highRiseProbability ? 2 : 1;
        int size = 0;
		while(size < chunkSize && searchFrontier.Count > 0)
        {
			HexCell current = searchFrontier.Dequeue();
            int originalElevation = current.Elevation;
            int newElevation = current.Elevation - sink;
			if(newElevation < elevationMinimum)
				continue;

			current.Elevation = newElevation;
            if(originalElevation >= waterLevel &&
				newElevation < waterLevel)
				budget += 1;
            
			size += 1;

			for(HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
				HexCell neighbor = current.GetNeighbor(d);
				if(neighbor && neighbor.SearchPhase < searchFrontierPhase)
                {
					neighbor.SearchPhase = searchFrontierPhase;
					neighbor.Distance = neighbor.coordinates.DistanceTo(center);
					neighbor.SearchHeuristic = Random.value < jitterProbability ? 1: 0; //Higher Huristics can be used for more ribony terrain
					searchFrontier.Enqueue(neighbor);
				}
			}
		}

		searchFrontier.Clear();
        return budget;
    }

	private void ErodeLand()
	{
		List<HexCell> erodibleCells = ListPool<HexCell>.Get();
		for(int i = 0; i < cellCount; i++)
		{
			HexCell cell = grid.GetCell(i);
			if(IsErodible(cell))
			{
				erodibleCells.Add(cell);
			}
		}

		int targetErodibleCount =
			(int)(erodibleCells.Count * (100 - erosionPercentage) * 0.01f);

		while(erodibleCells.Count > targetErodibleCount)
		{
			int index = Random.Range(0, erodibleCells.Count);
			HexCell cell = erodibleCells[index];
			HexCell targetCell = GetErosionTarget(cell);

			cell.Elevation -= 1;
			targetCell.Elevation += 1;

			if(!IsErodible(cell))
			{
				erodibleCells[index] = erodibleCells[erodibleCells.Count - 1];
				erodibleCells.RemoveAt(erodibleCells.Count - 1);
			}

			for(HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
			{
				HexCell neighbor = cell.GetNeighbor(d);
				if(neighbor && neighbor.Elevation == cell.Elevation + 2 &&
					!erodibleCells.Contains(neighbor))
				{
					erodibleCells.Add(neighbor);
				}
			}

			if(IsErodible(targetCell) && !erodibleCells.Contains(targetCell))
				erodibleCells.Add(targetCell);
			
			for(HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
			{
				HexCell neighbor = targetCell.GetNeighbor(d);
				if(neighbor && neighbor != cell &&
					neighbor.Elevation == targetCell.Elevation + 1 &&
					!IsErodible(neighbor))
				{
					erodibleCells.Remove(neighbor);
				}
			}
		}

		ListPool<HexCell>.Add(erodibleCells);
	}

	private bool IsErodible(HexCell cell)
	{
		int erodibleElevation = cell.Elevation - 2;
		for(HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
		{
			HexCell neighbor = cell.GetNeighbor(d);
			if(neighbor && neighbor.Elevation <= erodibleElevation)
				return true;
		}
		return false;
	}

	private HexCell GetErosionTarget(HexCell cell)	
	{
		List<HexCell> candidates = ListPool<HexCell>.Get();
		int erodibleElevation = cell.Elevation - 2;
		for(HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
		{
			HexCell neighbor = cell.GetNeighbor(d);
			if(neighbor && neighbor.Elevation <= erodibleElevation)
			{
				candidates.Add(neighbor);
			}
		}
		HexCell target = candidates[Random.Range(0, candidates.Count)];
		ListPool<HexCell>.Add(candidates);
		return target;
	}

    void SetTerrainType()
    {
		for(int i = 0; i < cellCount; i++)
        {
			HexCell cell = grid.GetCell(i);
			if(!cell.IsUnderwater)
            {
				cell.TerrainTypeIndex = cell.Elevation - cell.WaterLevel;
			}
		}
	}

    private HexCell GetRandomCell(MapRegion region)
    {
		return grid.GetCell(
			Random.Range(region.xMin, region.xMax),
			Random.Range(region.zMin, region.zMax)
		);
	}
}
