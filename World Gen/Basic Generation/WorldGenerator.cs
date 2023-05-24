using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.AI;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] public bool autoUpdate;
    [Header("Generation Controls")]
    [SerializeField] private int size = 200;
    [Range(1,4)]
    [SerializeField] private int  resolution= 1;
    //GOOD SCALE: -0.05
    [Range(0, 0.2f)]
    [SerializeField] private float worldNoiseScale = .1f;
    [Range(0, 0.1f)]
    [SerializeField] private float treeNoiseScale = .05f;
    [Range(0, 1f)]
    [SerializeField] private float treeDensity = .5f;
    [Range(0, 1f)]
    [SerializeField] private float waterLevel = .4f;
    
    [Header("Falloff Map")]
    [SerializeField] private float a = 3f;
    [SerializeField] private float b = 2.2f;

    [Header("Materials")]
    [SerializeField] private Material terrainMaterial;
    [SerializeField] private Material edgeMaterial;
    [SerializeField] private Material waterMaterial;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private GameObject waterPrefab;

    [Header("Reigons")]
    [SerializeField] private TerrainType[] regions;
    [Range(0, 2f)]
    [SerializeField] private float regionHeightScale = 0.5f;

    [Header("Walkable Layer")]
    [SerializeField] private LayerMask LayerMask;



    WorldGrid grid;
    WorldGrid worldGrid;

    public void GenerateMap()
    {
        this.Start();
    }

    void Start()
    {
        SetTerrainTypeHeights();
        
        ResetEditor();
        grid = new WorldGrid(size, resolution, worldNoiseScale, regions, a, b);
        grid.GenerateGrid();

        GenerateWater();
        GenerateTerrain();
        //GenerateTrees(grid);

        NavMeshSurface navMesh = gameObject.GetComponent<NavMeshSurface>();
        navMesh.layerMask = LayerMask;
        navMesh.BuildNavMesh();

    }

    private void SetTerrainTypeHeights()
    {
        for(int i = 0; i < regions.Length; i++)
        {
            regions[i].setWorldHeight(i-1, regionHeightScale);
        }
    }


    void GenerateTerrain()
    {
        MeshDrawer edgeDrawer = new EdgeDrawer(grid);
        MeshDrawer terrainDrawer = new TerrainDrawer(grid);
        
        //Generate terrain
        Mesh terrainMesh = terrainDrawer.DrawMesh();
        InitializeMesh(gameObject, terrainMesh);



        Mesh terrainEdges = edgeDrawer.DrawMesh();

        GameObject terrainEdgeObject = GenerateChild("Terrain Edges", 0);
        InitializeMesh(terrainEdgeObject, terrainEdges);

        DrawTexture(grid, false, terrainMaterial, terrainEdgeObject);

        //Generate water edges (TODO)
        //DrawEdgeMesh(grid);

        //Color the terrain
        DrawTexture(grid, false, terrainMaterial, gameObject);
    }


    
    void GenerateWater()
    {
        MeshDrawer waterDrawer = new WaterDrawer(grid);

        GameObject waterObject = GenerateChild("Water", -0.5f);
        Mesh terrainMesh = waterDrawer.DrawMesh();
        InitializeMesh(waterObject, terrainMesh);


        DrawTexture(grid, true, waterMaterial, waterObject);
    }

    private GameObject GenerateChild(string name, float yPos)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(transform);
        Vector3 currentPosition = gameObject.transform.position;
        gameObject.transform.position = new Vector3(currentPosition.x, yPos, currentPosition.z);
        return gameObject;
    }

    void InitializeMesh(GameObject targetObject, Mesh mesh)
    {
        MeshFilter meshFilter = targetObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = targetObject.AddComponent<MeshRenderer>();
        MeshCollider collider = targetObject.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;
    }

    void GenerateTrees(WorldGrid grid)
    {

        GameObject trees = new GameObject("Environment Trees");
        trees.transform.SetParent(transform);

        float[,] noiseMap = NoiseMaps.GenerateNoiseMap(treeNoiseScale, size);

        for (int y = 0; y < size; y=2)
        {
            for (int x = 0; x < size; x+=2)
            {
                Cell cell = grid.getCell(x, y);
                if (cell.region.worldHeight > regions[1].worldHeight)
                {
                    float v = Random.Range(0f, treeDensity);
                    if (noiseMap[x, y] < v)
                    {
                        GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                        GameObject tree = Instantiate(prefab, trees.transform);

                        tree.transform.position = new Vector3(cell.worldCoordinate.x, cell.region.height, cell.worldCoordinate.y);
                        tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                        tree.transform.localScale = Vector3.one * Random.Range(.8f, 1.2f);
                    }
                }
            }
        }
    }

  
    void DrawTexture(WorldGrid grid, bool isWater, Material material, GameObject targetObject)
    {
        Texture2D texture = new Texture2D(grid.size, grid.size);
        Color[] colorMap = new Color[grid.size * grid.size];

        for (int y = 0; y < grid.size; y++)
        {
            for (int x = 0; x < grid.size; x++)
            {
                Cell cell = grid.getCell(x, y);
                if (cell.isWater)
                {
                    colorMap[y * grid.size + x] = Color.red;
                }
                else
                {

                    colorMap[y * grid.size + x] = cell.region.color;
                }
            }
        }
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colorMap);
        texture.Apply();

        MeshRenderer meshRenderer = targetObject.GetComponent<MeshRenderer>();
       
        meshRenderer.sharedMaterial = material;
        if (!isWater)
        {
            meshRenderer.sharedMaterial.mainTexture = texture;
        }

        
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
                fallOffMap[x, y] = Mathf.Pow(v, a) / (Mathf.Pow(v, a) + Mathf.Pow(b - b * v, a));
            }
        }

        return fallOffMap;
    }

    private void ResetEditor()
    {
        if(TryGetComponent<MeshFilter>(out MeshFilter filter))
        {
            DestroyImmediate(filter);
        }
        if (TryGetComponent<MeshRenderer>(out MeshRenderer renderer))
        {
            DestroyImmediate(renderer);
        }
        if (TryGetComponent<MeshCollider>(out MeshCollider collider))
        {
            DestroyImmediate(collider);
        }

        /*
        for (int y = 0; y < transform.childCount; y++)
        {
            DestroyImmediate(transform.GetChild(y).gameObject);
        }
        */
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
    }

}

[System.Serializable]
public class TerrainType
{
    public string name;
    public float height;
    public Color color;
    public float worldHeight { get; private set; }

    /// <summary>
    /// Determine the world height of the given region
    /// </summary>
    /// <param name="heightLayer">Relative ordering to other region layers</param>
    /// <param name="layerScale">The actual difference in height between layers</param>
    public void setWorldHeight(int heightLayer, float layerScale)
    {
        this.worldHeight = heightLayer * layerScale;
    }
}
