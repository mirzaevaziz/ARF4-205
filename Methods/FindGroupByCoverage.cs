namespace GenomExperiment.Methods;

public class FindGroupByCoverage
{
    public static List<HashSet<int>> Run(
        Helpers.Logger logger,
        Models.ObjectSet set,
        Models.Neighborhood neighborhood
    )
    {
        var result = new List<HashSet<int>>();

        var sphereList = Models.Sphere.Find(logger, set, neighborhood);
        System.Console.WriteLine($"Found {sphereList.Count()} spheres");

        var allCoverageObjectIndexList = sphereList
            .SelectMany(s => s.CoverageList?.Select(ss => ss.ObjectIndex) ?? Array.Empty<int>())
            .ToHashSet();

        logger.WriteLine(
            "FindGroupByCoverage",
            $"Found {allCoverageObjectIndexList.Count} coverage objects",
            true
        );
        logger.WriteLine(
            "FindGroupByCoverage",
            string.Join(", ", allCoverageObjectIndexList.OrderBy(o => o)),
            true
        );

        var groupList = new List<Models.Group>();
        while (allCoverageObjectIndexList.Count > 0)
        {
            var group = new Models.Group();
            group.CoverageObjectIndexList.Add(allCoverageObjectIndexList.ElementAt(0));
            var countBefore = group.ObjectIndexList.Count;
            do
            {
                group.ObjectIndexList.UnionWith(
                    sphereList
                        .Where(
                            w =>
                                group.CoverageObjectIndexList.Contains(w.ObjectIndex)
                                || (
                                    w.RelativeList?.Any(
                                        a => group.CoverageObjectIndexList.Contains(a.ObjectIndex)
                                    ) ?? false
                                )
                        )
                        .Select(s => s.ObjectIndex)
                );
                //ToDo: Coveragelarni groupga qo'shish
                // group.CoverageObjectIndexList.UnionWith();
            } while (countBefore != group.ObjectIndexList.Count);
        }

        return result;
    }
}
