using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class MeshDrawer
{
    protected WorldGrid grid;
    protected int size;
    protected float squareSize;

    protected List<Vector3> vertices;
    protected List<int> triangles;
    protected List<Vector2> uvs;

    public MeshDrawer(WorldGrid grid)
    {
        this.grid = grid;
        this.size = grid.size;
    }



    public Mesh DrawMesh()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        
        CalculateMeshValuesFromGrid();

        mesh = GenerateMesh(mesh);

        return mesh;
    }

    /// <summary>
    /// Initialize mesh value lists for grid, update based on values in the cell
    /// </summary>
    protected void CalculateMeshValuesFromGrid()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();



        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid.getCell(x, y);

                if (IsValidRegion(cell.region))
                {
                    GenerateMeshFromCell(cell);
                }
                
            }
        }
    }

    /// <summary>
    /// Determine which regions this mesh generator will need to include/exclude
    /// </summary>
    /// <param name="cellRegion"></param>
    /// <returns></returns>
    protected abstract bool IsValidRegion(TerrainType cellRegion);

    /// <summary>
    /// The general steps for calculating each mesh value from a given cell
    /// </summary>
    /// <param name="cell"></param>
    protected void GenerateMeshFromCell(Cell cell)
    {
        float xPos = cell.worldCoordinate.x;
        float yPos = cell.worldCoordinate.y;

        Vector2[] uvCoordinates = CalculateUVCoordinates(xPos, yPos);
        List<Vector3[]> vertexCoordinates = CalculateVertices(cell);

        AddCellValuesToMesh(vertexCoordinates, uvCoordinates);
    }

    /// <summary>
    /// Adds each given vertex coordinate set to the list,
    /// uv values are repeated if there are multiple vertices per cell
    /// </summary>
    /// <param name="vertexCoordinates">Some cells may have multiple coordinates</param>
    /// <param name="uv"></param>
    protected void AddCellValuesToMesh(List<Vector3[]> vertexCoordinates, Vector2[] uv)
    {
        foreach(Vector3[] square in vertexCoordinates)
        {

            for (int k = 0; k < square.Length; k++)
            {
                vertices.Add(square[k]);
                //
                //triangles.Add(triangles.Count);

                triangles.Add(triangles.Count);
                uvs.Add(uv[k]);
            }
        }
    }

    protected virtual List<Vector3[]> CalculateVertices(Cell cell)
    {
        float xPos = cell.worldCoordinate.x;
        float yPos = cell.worldCoordinate.y;
        float triangleSize = grid.squareSize/2;

        float layerHeight = cell.region.worldHeight;

        List<Vector3[]> vertices = new List<Vector3[]>();

        Vector3 a = new Vector3(xPos - triangleSize, layerHeight, yPos + triangleSize);
        Vector3 b = new Vector3(xPos + triangleSize, layerHeight, yPos + triangleSize);
        Vector3 c = new Vector3(xPos - triangleSize, layerHeight, yPos - triangleSize);
        Vector3 d = new Vector3(xPos + triangleSize, layerHeight, yPos - triangleSize);


        vertices.Add(new Vector3[] { a, b, c, b, d, c });
        return vertices;
    }

    protected virtual Vector2[] CalculateUVCoordinates(float xPos, float yPos)
    {
        Vector2 uvA = new Vector2(xPos / (float)grid.worldSize, yPos / (float)grid.worldSize);
        Vector2 uvB = new Vector2((xPos + grid.squareSize) / (float)grid.worldSize, yPos / (float)grid.worldSize);
        Vector2 uvC = new Vector2(xPos / (float)grid.worldSize, (yPos + grid.squareSize) / (float)grid.worldSize);
        Vector2 uvD = new Vector2((xPos + grid.squareSize) / (float)grid.worldSize, (yPos + grid.squareSize) / (float)grid.worldSize);

        return new Vector2[] { uvA, uvB, uvC, uvB, uvD, uvC };
    }

    

    /// <summary>
    /// Use all generated values to create a mesh
    /// </summary>
    /// <param name="mesh"></param>
    /// <returns></returns>
    protected Mesh GenerateMesh(Mesh mesh)
    {

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }
}
