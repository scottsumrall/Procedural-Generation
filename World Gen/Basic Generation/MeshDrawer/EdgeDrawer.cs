using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class EdgeDrawer : MeshDrawer
{
    public EdgeDrawer(WorldGrid grid) : base(grid) { }


    protected override bool IsValidRegion(TerrainType cellRegion)
    {
        return true;
    }

    /// <summary>
    /// Check if each adjacent cell is lower than the current cell then calculate the verticies of that edge
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    protected override List<Vector3[]> CalculateVertices(Cell cell)
    {
        int x = cell.xPos;
        int y = cell.yPos;

        float currentRegionHeight = cell.region.worldHeight;
        Cell adjacentCell;

        List<Vector3[]> cellVertices = new List<Vector3[]>();

        // Left cell
        if (x > 0)
        {
            adjacentCell = grid.getCell(x - 1, y);
            if (adjacentCell.region.worldHeight < currentRegionHeight)
            {
                cellVertices.Add(CalculateEdgeByDirection(EdgeDirection.Left, cell, adjacentCell));

            }
        }

        // Right cell
        if (x < size - 1)
        {
            adjacentCell = grid.getCell(x + 1, y);
            if (adjacentCell.region.worldHeight < currentRegionHeight)
            {
                cellVertices.Add(CalculateEdgeByDirection(EdgeDirection.Right, cell, adjacentCell));
            }
        }

        // Down cell
        if (y > 0)
        {
            adjacentCell = grid.getCell(x, y - 1);
            if (adjacentCell.region.worldHeight < currentRegionHeight)
            {
                cellVertices.Add(CalculateEdgeByDirection(EdgeDirection.Down, cell, adjacentCell));
            }
        }

        // Up cell
        if (y < size - 1)
        {
            adjacentCell = grid.getCell(x, y + 1);
            if (adjacentCell.region.worldHeight < currentRegionHeight)
            {
                cellVertices.Add(CalculateEdgeByDirection(EdgeDirection.Up, cell, adjacentCell));
            }
        }
        return cellVertices;
    }

    /// <summary>
    /// Stores pre-set values for edge values in each direction
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="primaryCell">The current cell being computed</param>
    /// <param name="lowerCell">The adjacent cell that is lower</param>
    /// <returns></returns>
    /// <exception cref="InvalidEnumArgumentException"></exception>
    private Vector3[] CalculateEdgeByDirection(EdgeDirection direction, Cell primaryCell, Cell lowerCell)
    {
        float x = primaryCell.worldCoordinate.x;
        float y = primaryCell.worldCoordinate.y;
        float terrainHeightUpper = primaryCell.region.worldHeight;
        float terrainHeightLower = lowerCell.region.worldHeight;

        if (lowerCell.region.name.Equals("Water"))
        {
            terrainHeightLower -= 1;
        }

        Vector3 a;
        Vector3 b;
        Vector3 c;
        Vector3 d;

        float triangleSize = grid.squareSize / 2;

        switch (direction)
        {
            case EdgeDirection.Left:
                {
                    a = new Vector3(x - triangleSize, terrainHeightUpper, y + triangleSize);
                    b = new Vector3(x - triangleSize, terrainHeightUpper, y - triangleSize);
                    c = new Vector3(x - triangleSize, terrainHeightLower, y + triangleSize);
                    d = new Vector3(x - triangleSize, terrainHeightLower, y - triangleSize);
                    break;
                }
            case EdgeDirection.Right:
                {
                    a = new Vector3(x + triangleSize, terrainHeightUpper, y - triangleSize);
                    b = new Vector3(x + triangleSize, terrainHeightUpper, y + triangleSize);
                    c = new Vector3(x + triangleSize, terrainHeightLower, y - triangleSize);
                    d = new Vector3(x + triangleSize, terrainHeightLower, y + triangleSize);
                    break;
                }
            case EdgeDirection.Down:
                {
                    a = new Vector3(x - triangleSize, terrainHeightUpper, y - triangleSize);
                    b = new Vector3(x + triangleSize, terrainHeightUpper, y - triangleSize);
                    c = new Vector3(x - triangleSize, terrainHeightLower, y - triangleSize);
                    d = new Vector3(x + triangleSize, terrainHeightLower, y - triangleSize);
                    break;
                }
            case EdgeDirection.Up:
                {
                    a = new Vector3(x + triangleSize, terrainHeightUpper, y + triangleSize);
                    b = new Vector3(x - triangleSize, terrainHeightUpper, y + triangleSize);
                    c = new Vector3(x + triangleSize, terrainHeightLower, y + triangleSize);
                    d = new Vector3(x - triangleSize, terrainHeightLower, y + triangleSize);
                    break;
                }
            default:
                {
                    throw new InvalidEnumArgumentException();
                }

        }

        return new Vector3[] { a, b, c, b, d, c };
    }
}
enum EdgeDirection
{
    Left,
    Right,
    Down,
    Up
}