using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GeneData
{
    public List<float> angles;
    public List<float> legSizes;
    public List<float> bodySizes;
    public int name;

    public GeneData(Gene gene)
    {
        angles = new List<float>(gene.angles);
        legSizes = new List<float>(gene.legSizes);
        bodySizes = new List<float>(gene.bodySizes);
        name = gene.name;
    }
}

[System.Serializable]
public class GeneDataList
{
    public List<GeneData> geneDatas;

    public GeneDataList(List<GeneData> geneDatas)
    {
        this.geneDatas = geneDatas;
    }
}

