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

    private HexCellPriorityQueue searchFrontier;
    private int searchFrontierPhase;
    private int cellCount;


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
        CreateLand();
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
        while(landBudget > 0)
        {
			int chunkSize = Random.Range(chunkSizeMin, chunkSizeMax - 1);
			if(Random.value < sinkProbability)
            {
				landBudget = SinkTerrain(chunkSize, landBudget);
			}
			else
            {
				landBudget = RaiseTerrain(chunkSize, landBudget);
			}
		}
	}

    private int RaiseTerrain(int chunkSize, int budget)
    {
        searchFrontierPhase += 1;
		HexCell firstCell = GetRandomCell();
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

    private int SinkTerrain(int chunkSize, int budget)
    {
        searchFrontierPhase += 1;
		HexCell firstCell = GetRandomCell();
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

    private HexCell GetRandomCell()
    {
		return grid.GetCell(Random.Range(0, cellCount));
	}
}
