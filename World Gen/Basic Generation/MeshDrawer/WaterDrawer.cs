using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDrawer : MeshDrawer
{
    public WaterDrawer(WorldGrid grid) : base(grid) { }

    protected override Vector2[] CalculateUVCoordinates(float xPos, float yPos)
    {
        float triangleSize = grid.squareSize / 2;

        Vector2 uvA = new Vector2((xPos - triangleSize) / (float)size, (yPos + triangleSize) / (float)size);
        Vector2 uvB = new Vector2((xPos + triangleSize) / (float)size, (yPos + triangleSize) / (float)size);
        Vector2 uvC = new Vector2((xPos - triangleSize) / (float)size, (yPos - triangleSize) / (float)size);
        Vector2 uvD = new Vector2((xPos + triangleSize) / (float)size, (yPos - triangleSize) / (float)size);


        return new Vector2[] { uvA, uvB, uvC, uvB, uvD, uvC };
    }

    protected override bool IsValidRegion(TerrainType cellRegion)
    {
        if (!cellRegion.name.Equals("Water"))
        {
            return false;
        }

        return true;
    }
}
