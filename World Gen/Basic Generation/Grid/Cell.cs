using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public bool isWater;
    public TerrainType region;
    public int xPos { get; private set; }
    public int yPos { get; private set; }

    public Vector2 worldCoordinate;

    public Cell(int xPos, int yPos, Vector2 worldCoordinate)
    {
        this.xPos = xPos;
        this.yPos = yPos;
        this.worldCoordinate = worldCoordinate;
    }
}

