using System.Collections.Generic;

[System.Serializable]
public class PopulationData
{
    public List<GenomeData> genomes;

    public PopulationData(List<GenomeData> genomes)
    {
        this.genomes = genomes;
    }
}