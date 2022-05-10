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
	[SerializeField, Range(0f, 1f)] private float evaporationFactor = 0.5f;
	[SerializeField, Range(0f, 1f)] private float precipitationFactor = 0.25f;
	[SerializeField, Range(0f, 1f)] private float runoffFactor = 0.25f;
	[SerializeField, Range(0f, 1f)] private float seepageFactor = 0.125f;
	[SerializeField] private HexDirection windDirection = HexDirection.NW;
	[SerializeField, Range(1f, 10f)] private float windStrength = 4f;
	[SerializeField, Range(0f, 1f)] private float startingMoisture = 0.1f;

    private HexCellPriorityQueue searchFrontier;
    private int searchFrontierPhase;
    private int cellCount;

	
	private struct MapRegion
	{
		public int xMin, xMax, zMin, zMax;
	}
	struct ClimateData
	{
		public float clouds, moisture;
	}
	private List<MapRegion> regions;
	private List<ClimateData> climate = new List<ClimateData>();
	private List<ClimateData> nextClimate = new List<ClimateData>();


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
		CreateClimate();
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

    private void SetTerrainType()
    {
		for(int i = 0; i < cellCount; i++)
		{
			HexCell cell = grid.GetCell(i);
			float moisture = climate[i].moisture;
			if (!cell.IsUnderwater)
			{
				if(moisture < 0.05f)
				{
					cell.TerrainTypeIndex = 4;
				}
				else if(moisture < 0.12f)
				{
					cell.TerrainTypeIndex = 0;
				}
				else if(moisture < 0.28f)
				{
					cell.TerrainTypeIndex = 3;
				}
				else if(moisture < 0.85f)
				{
					cell.TerrainTypeIndex = 1;
				}
				else
				{
					cell.TerrainTypeIndex = 2;
				}
			}
			else
			{
				cell.TerrainTypeIndex = 2;
			}
			cell.SetMapData(moisture);
		}
	}

	private void CreateClimate ()
	{
		climate.Clear();
		nextClimate.Clear();
		ClimateData initialData = new ClimateData();
		initialData.moisture = startingMoisture;
		ClimateData clearData = new ClimateData();
		for(int i = 0; i < cellCount; i++)
		{
			climate.Add(initialData);
			nextClimate.Add(clearData);
		}

		for(int cycle = 0; cycle < 40; cycle++)
		{
			for(int i = 0; i < cellCount; i++)
			{
				EvolveClimate(i);
			}
			List<ClimateData> swap = climate;
			climate = nextClimate;
			nextClimate = swap;
		}
	}

	private void EvolveClimate(int cellIndex)
	{
		HexCell cell = grid.GetCell(cellIndex);
		ClimateData cellClimate = climate[cellIndex];
		
		if(cell.IsUnderwater)
		{
			cellClimate.moisture = 1f;
			cellClimate.clouds += evaporationFactor;
		}
		else
		{
			float evaporation = cellClimate.moisture * evaporationFactor;
			cellClimate.moisture -= evaporation;
			cellClimate.clouds += evaporation;
		}

		float precipitation = cellClimate.clouds * precipitationFactor;
		cellClimate.clouds -= precipitation;
		cellClimate.moisture += precipitation;

		float cloudMaximum = 1f - cell.ViewElevation / (elevationMaximum + 1f);
		if(cellClimate.clouds > cloudMaximum)
		{
			cellClimate.moisture += cellClimate.clouds - cloudMaximum;
			cellClimate.clouds = cloudMaximum;
		}

		HexDirection mainDispersalDirection = windDirection.Opposite();
		float cloudDispersal = cellClimate.clouds * (1f / (5f + windStrength));
		float runoff = cellClimate.moisture * runoffFactor * (1f / 6f);
		float seepage = cellClimate.moisture * seepageFactor * (1f / 6f);
		for(HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
		{
			HexCell neighbor = cell.GetNeighbor(d);
			if(!neighbor)
				continue;
			
			ClimateData neighborClimate = nextClimate[neighbor.Index];
			if(d == mainDispersalDirection)
			{
				neighborClimate.clouds += cloudDispersal * windStrength;
			}
			else
			{
				neighborClimate.clouds += cloudDispersal;
			}

			int elevationDelta = neighbor.ViewElevation - cell.ViewElevation;
			if(elevationDelta < 0)
			{
				cellClimate.moisture -= runoff;
				neighborClimate.moisture += runoff;
			}
			else if(elevationDelta == 0)
			{
				cellClimate.moisture -= seepage;
				neighborClimate.moisture += seepage;
			}

			nextClimate[neighbor.Index] = neighborClimate;
		}

		ClimateData nextCellClimate = nextClimate[cellIndex];
		nextCellClimate.moisture += cellClimate.moisture;
		if(nextCellClimate.moisture > 1f)
		{
			nextCellClimate.moisture = 1f;
		}
		nextClimate[cellIndex] = nextCellClimate;
		climate[cellIndex] = new ClimateData();
	}

    private HexCell GetRandomCell(MapRegion region)
    {
		return grid.GetCell(
			Random.Range(region.xMin, region.xMax),
			Random.Range(region.zMin, region.zMax)
		);
	}
}
