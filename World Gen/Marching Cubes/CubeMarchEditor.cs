using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BasicMarchingCube))]
public class CubeMarchEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BasicMarchingCube mapGen = (BasicMarchingCube)target;

        if (DrawDefaultInspector())
        {
            if (mapGen.autoUpdate)
            {
                mapGen.GenerateMap();
            }
        }


        if (GUILayout.Button("Generate"))
        {
            mapGen.GenerateMap();
        }
    }
}
