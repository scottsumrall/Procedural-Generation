using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class WorldGrid : MonoBehaviour 
{
    //Serialized constants
    protected float falloffValueA;
    protected float falloffValueB;
    protected float noiseScale;
    protected TerrainType[] regions;


    public int size { get; private set; }
    public int worldSize { get; private set; }
    public float squareSize { get; private set; }

    protected Cell[,] grid;

    public WorldGrid(int size, int resolution)
    {
        this.size = resolution * size;
        this.squareSize = 1 / resolution;
    }

    public WorldGrid(int size, int resolution, float noiseScale, TerrainType[] regions, float falloffValueA, float falloffValueB)
    {
        this.size = resolution * size;
        this.worldSize = size;
        this.squareSize = 1f / resolution;

        //serialized constants
        this.noiseScale = noiseScale;
        this.regions = regions; 
        this.falloffValueA = falloffValueA;
        this.falloffValueB = falloffValueB;
    }

    public void GenerateGrid()
    {
        float[,] terrainNoiseMap = NoiseMaps.GenerateNoiseMap(noiseScale, size);
        float[,] fallOffMap = GenerateFallOffMap();

        grid = new Cell[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 worldCoordinate = new Vector2(x*squareSize, y*squareSize);


                Cell cell = new Cell(x, y, worldCoordinate);

                float noiseValue = terrainNoiseMap[x, y];
                noiseValue -= fallOffMap[x, y];

                SetCellRegion(noiseValue, cell);
                grid[x, y] = cell;
            }
        }
        
    }

    public Cell getCell(int xPos, int yPos)
    {
        return grid[xPos, yPos];
    }

    public Cell getCell(Vector2 worldCoordinate)
    {
        int xPos = (int)(worldCoordinate.x / squareSize);
        int yPos = (int)(worldCoordinate.y / squareSize);

        return grid[xPos, yPos];
    }

    protected void SetCellRegion(float noiseValue, Cell cell)
    {
        for (int z = 0; z < regions.Length; z++)
        {

            if (noiseValue <= regions[z].height)
            {
                cell.region = regions[z];
                break;
            }
        }
        //Debug.Log(cell.region);
    }
    private float[,] GenerateFallOffMap()
    {
        float[,] fallOffMap = new float[size, size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float xv = x / (float)size * 2 - 1;
                float yv = y / (float)size * 2 - 1;
                float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));

                //not sure what this is about, test in the future?
                fallOffMap[x, y] = Mathf.Pow(v, falloffValueA) / (Mathf.Pow(v, falloffValueA) + Mathf.Pow(falloffValueB - falloffValueB * v, falloffValueA));
            }
        }

        return fallOffMap;
    }
}
