namespace GenomExperiment.Models;

public class Group
{
    public Group()
    {
        CoverageObjectIndexList = new HashSet<int>();
        ObjectIndexList = new HashSet<int>();
    }

    public HashSet<int> CoverageObjectIndexList { get; set; }
    public HashSet<int> ObjectIndexList { get; set; }
}
