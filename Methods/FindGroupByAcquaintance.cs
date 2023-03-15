using System.Collections.Concurrent;

namespace GenomExperiment.Methods;

public class FindGroupByAcquaintance
{
    public static List<HashSet<int>> Run(
        Helpers.Logger logger,
        Models.ObjectSet set,
        Models.Neighborhood neighborhood
    )
    {
        var result = new List<HashSet<int>>();

        var coverageObjectIndexList = new BlockingCollection<int>();

        Parallel.ForEach(
            set.Objects.Keys,
            (objectIndex) =>
            {
                var objectNeighborList = neighborhood.GetObjectNeigborList(objectIndex);
                var radius = objectNeighborList
                    .Where(w => w.ObjectClassValue != set.Objects[objectIndex].ClassValue)
                    .Min(m => m.Distance);
                var enemyList = objectNeighborList.Where(
                    w =>
                        w.Distance == radius
                        && w.ObjectClassValue != set.Objects[objectIndex].ClassValue
                );

                var relativeList = objectNeighborList
                    .Where(w => w.Distance < radius)
                    .Select(s => s.ObjectIndex);

                logger.WriteLine(
                    "FindGroupByAcquaintance",
                    $"Enemy list for Object[{objectIndex}] is\n\t{string.Join("\n\t", enemyList)}",
                    true
                );

                foreach (var enemy in enemyList)
                {
                    var enemyNeighborList = neighborhood.GetObjectNeigborList(
                        enemy.ObjectIndex,
                        relativeList.Append(objectIndex)
                    );
                    logger.WriteLine(
                        "FindGroupByAcquaintance",
                        $"Enemy neighbor list for Object[{enemy}] is \n\t{string.Join("\n\t", enemyNeighborList)}",
                        true
                    );
                    var enemyMinRadius = enemyNeighborList.Min(m => m.Distance);
                    var coverageList = enemyNeighborList.Where(w => w.Distance == enemyMinRadius);
                    logger.WriteLine(
                        "FindGroupByAcquaintance",
                        $"Coverage list for Object[{enemy}] with radius {enemyMinRadius} is\n\t{string.Join("\n\t", coverageList)}",
                        true
                    );
                    foreach (var coverage in coverageList)
                    {
                        coverageObjectIndexList.Add(coverage.ObjectIndex);
                    }
                }
            }
        );
        coverageObjectIndexList.CompleteAdding();
        logger.WriteLine(
            "FindGroupByAcquaintance",
            $"Found {coverageObjectIndexList.ToHashSet().Count} coverage objects",
            true
        );
        logger.WriteLine(
            "FindGroupByAcquaintance",
            string.Join(", ", coverageObjectIndexList.ToHashSet().OrderBy(o => o)),
            true
        );

        return result;
    }
}
