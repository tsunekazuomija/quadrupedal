using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Gene
{
    public List<float> angles;
    public List<float> legSizes;
    public List<float> bodySizes;
    public int name;

    public Gene(int numAngles, int numLegSizes)
    {
        angles = new List<float>(numAngles);
        for (int i = 0; i < numAngles; i++)
        {
            angles.Add(Random.Range(-90.0f, 90.0f));
        }
        legSizes = new List<float>(numLegSizes);
        for (int i = 0; i < numLegSizes; i++)
        {
            legSizes.Add(0.25f);
        }
        bodySizes = new List<float>(3);
        for (int i = 0; i < 3; i++)
        {
            bodySizes.Add(2.0f);
        }
    }
}