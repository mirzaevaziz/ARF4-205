namespace GenomExperiment.Models;

public class Sphere
{
    public int ObjectIndex { get; set; }
    public decimal Radius { get; set; }
    public IEnumerable<Neighborhood.ObjectNeighborInfo>? RelativeList { get; set; }
    public IEnumerable<Neighborhood.ObjectNeighborInfo>? EnemyList { get; set; }
    public IEnumerable<Neighborhood.ObjectNeighborInfo>? CoverageList { get; set; }
}
