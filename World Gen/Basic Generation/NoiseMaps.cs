using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public static class NoiseMaps
{
    public static float[,] GenerateNoiseMap(float scale, int size)
    {
        float xOffset = Random.Range(-10000f, 10000f);
        float yOffset = Random.Range(-10000f, 10000f);

        float[,] noiseMap = new float[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noiseValue = Mathf.PerlinNoise(x * scale + xOffset, y * scale + yOffset);
                noiseMap[x, y] = noiseValue;
            }
        }

        return noiseMap;
    }

    public static float[,,] GenerateNoiseMap3D(int resolution, float terraceHeight, float scale, int width, int height)
    {
        float xOffset = Random.Range(-10000f, 10000f);
        float zOffset = Random.Range(-10000f, 10000f);

        float[,,] noiseMap = new float[width + 1, height + resolution, width + 1];
        for (int x = 0; x < width + 1; x++)
        {
            for (int z = 0; z < width + 1; z++)
            {
                for(int y = 0; y < height + resolution; y++)
                {
                    float noiseValue = (float)height * Mathf.PerlinNoise(x * scale + xOffset, z * scale + zOffset);
                    noiseMap[x, y, z] = (float)y - noiseValue + (float)y % terraceHeight;
                }
            }
        }

        return noiseMap;
    }
}
