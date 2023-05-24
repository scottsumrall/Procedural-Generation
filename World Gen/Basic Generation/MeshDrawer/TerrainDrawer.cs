using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainDrawer : MeshDrawer
{
    public TerrainDrawer(WorldGrid grid) : base(grid) { }



    protected override bool IsValidRegion(TerrainType cellRegion)
    {
        if(cellRegion.name.Equals("Water"))
        {
            return false;
        }

        return true;
    }
}
