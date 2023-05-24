using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coordinate
{
    public float x { get; private set; }
    public float y { get; private set; }

    public Coordinate(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector3 get3DCoordinate(float height)
    {
        return new Vector3(x, height, y);
    }

}
