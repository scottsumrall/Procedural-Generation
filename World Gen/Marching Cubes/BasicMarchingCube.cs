using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class BasicMarchingCube : MonoBehaviour
{
    [Range(1, 10)]
    [SerializeField] int resolution;
    [SerializeField] bool smoothTerrain;
    [SerializeField] private float terrainSurface = 0.5f;
    [SerializeField] private int width = 32;
    [Range(1, 100)]
    [SerializeField] private int worldHeight = 8;
    [Range(1, 4)]
    [SerializeField] private float terraceHeight;
    [SerializeField] private float worldNoiseScale = .2f;
    public bool autoUpdate = false;


    const int MAX_TRIANGLES_PER_MESH = 5;
    const int VERTS_PER_TRIANGLE = 3;

    public Material terrainMaterial;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    MeshFilter meshFilter;


    private float[,,] _terrainMap;
    private int height;

    public void GenerateMap()
    {
        ClearMeshData();
        Start();
    }

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        height = worldHeight * resolution;

        _terrainMap = new float[width + 1, height + resolution, width + 1];

        //PopulateTerrainMap();
        _terrainMap = NoiseMaps.GenerateNoiseMap3D(resolution, terraceHeight, worldNoiseScale, width, height);
        CreateMeshData();
        
    }

    void PopulateTerrainMap()
    {
        for(int x = 0; x < width + 1; x++)
        {
            for(int z = 0; z < width + 1; z++)
            {
                for (int y = 0; y < height + 1; y++)
                {
                    float curHeight = (float)height * (Mathf.PerlinNoise((float)x / 16f * 1.5f + 0.001f, (float)z / 16f * 1.5f + 0.001f));

                    _terrainMap[x, y, z] = (float)y - curHeight //+ (float)y % terraceHeight
                        ;
                }
            }
        }
    }

    private void CreateMeshData()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < width; z++)
                {
                    MarchCubeTest(new Vector3Int(x, y, z));
                }
            }
        }
        BuildMesh();
    }

    int GetCubeConfiguration(float[] cube)
    {
        int configurationIndex = 0;

        for(int i = 0; i < 8; i++)
        {
            if (cube[i] > terrainSurface)
            {
                configurationIndex |= 1 << i;
            }
        }
        return configurationIndex;
    }

    float SampleTerrain(Vector3Int point)
    {
        return _terrainMap[point.x, point.y, point.z];
    }

    void MarchCube(Vector3Int position)
    {
        float[] cube = new float[8];

        for (int i = 0; i < 8; i++)
        {
            cube[i] = SampleTerrain(position + MarchTables.CornerTable[i]);
        }

        int configIndex = GetCubeConfiguration(cube);

        if (configIndex == 0 || configIndex == 255) return;


        int edgeIndex = 0;

        for(int triangleCount = 0; triangleCount < MAX_TRIANGLES_PER_MESH; triangleCount++)
        {
            for(int triangleVerts = 0; triangleVerts <  VERTS_PER_TRIANGLE; triangleVerts++)
            {
                int indice = MarchTables.TriangleTable[configIndex, edgeIndex];

                if (indice == -1) return;


                Vector3 vert1 = position + MarchTables.CornerTable[MarchTables.EdgeIndices[indice, 0]];
                Vector3 vert2 = position + MarchTables.CornerTable[MarchTables.EdgeIndices[indice, 1]];

                vert1 = new Vector3(vert1.x, vert1.y / (float)resolution, vert1.z);
                vert2 = new Vector3(vert2.x, vert2.y / (float)resolution, vert2.z);

                Vector3 vertPosition;
                if (smoothTerrain)
                {
                    //calculate height of terrain in both vertices
                    float vert1Sample = cube[MarchTables.EdgeIndices[indice, 0]];
                    float vert2Sample = cube[MarchTables.EdgeIndices[indice, 1]];

                    float difference = vert2Sample - vert1Sample;

                    if(difference == 0)
                    {
                        difference = terrainSurface;
                    }
                    else
                    {
                        difference = (terrainSurface - vert1Sample) / difference;
                    }

                    vertPosition = vert1 + ((vert2 - vert1) * difference);
                }
                else
                {
                    vertPosition = (vert1 + vert2) / 2f;
                }

                vertices.Add(vertPosition);
                triangles.Add(vertices.Count - 1);
                edgeIndex++;
            }
        }
    }

    void MarchCubeTest(Vector3Int position)
    {
        float[] cube = new float[8];

        for (int i = 0; i < 8; i++)
        {
            cube[i] = SampleTerrain(position + MarchTables.CornerTable[i]);
        }

        int configIndex = GetCubeConfiguration(cube);

        if (configIndex == 0 || configIndex == 255) return;

        int[] triangulation = Enumerable.Range(0, MarchTables.TriangleTable.GetLength(1))
                .Select(x => MarchTables.TriangleTable[configIndex, x])
                .ToArray();
        
        foreach(int edgeIndex in triangulation)
        {
            if (edgeIndex == -1) return;

            Vector3 vert1 = position + MarchTables.CornerTable[MarchTables.EdgeIndices[edgeIndex, 0]];
            Vector3 vert2 = position + MarchTables.CornerTable[MarchTables.EdgeIndices[edgeIndex, 1]];
            /*
            float vert1Sample = cube[MarchTables.EdgeIndices[edgeIndex, 0]];
            float vert2Sample = cube[MarchTables.EdgeIndices[edgeIndex, 1]];

            float difference = vert2Sample - vert1Sample;

            if (difference == 0)
            {
                difference = terrainSurface;
            }
            else
            {
                difference = (terrainSurface - vert1Sample) / difference;
            }
            Vector3 vertPosition = vert1 + ((vert2 - vert1) * difference);
            */
            Vector3 vertPosition;
            if (smoothTerrain)
            {
                //calculate height of terrain in both vertices
                float vert1Sample = cube[MarchTables.EdgeIndices[edgeIndex, 0]];
                float vert2Sample = cube[MarchTables.EdgeIndices[edgeIndex, 1]];

                float difference = vert2Sample - vert1Sample;

                if (difference == 0)
                {
                    difference = terrainSurface;
                }
                else
                {
                    difference = (terrainSurface - vert1Sample) / difference;
                }

                vertPosition = vert1 + ((vert2 - vert1) * (difference - 0.01f));
            }
            else
            {
                vertPosition = (vert1 + vert2) / 2f;
            }
                vertices.Add(vertPosition);
            triangles.Add(vertices.Count - 1);
        }
    }

    void ClearMeshData()
    {
        vertices.Clear();
        triangles.Clear();

        if (TryGetComponent<MeshFilter>(out MeshFilter filter))
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
    }

    void BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = vertices.ToArray();
        /*
        List<int> reverseTriangles = triangles;
        reverseTriangles.Reverse();
        int[] forwardAndBack = (triangles.ToArray()).Concat((reverseTriangles.ToArray())).ToArray();
        mesh.triangles = forwardAndBack;
        */
        mesh.triangles = triangles.ToArray();


        mesh.RecalculateNormals();

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = terrainMaterial;

        MeshCollider collider = gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;
    }
}
