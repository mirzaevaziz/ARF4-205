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

        var sphereList = new BlockingCollection<Models.Sphere>();

        Parallel.ForEach(
            set.Objects.Keys,
            (objectIndex) =>
            {
                var sphere = new Models.Sphere();
                var objectNeighborList = neighborhood.GetObjectNeigborList(objectIndex);
                sphere.Radius = objectNeighborList
                    .Where(w => w.ObjectClassValue != set.Objects[objectIndex].ClassValue)
                    .Min(m => m.Distance);
                sphere.EnemyList = objectNeighborList.Where(
                    w =>
                        w.Distance == sphere.Radius
                        && w.ObjectClassValue != set.Objects[objectIndex].ClassValue
                );

                sphere.RelativeList = objectNeighborList.Where(w => w.Distance < sphere.Radius);

                logger.WriteLine(
                    "FindGroupByAcquaintance",
                    $"Enemy list for Object[{objectIndex}] is\n\t{string.Join("\n\t", sphere.EnemyList)}",
                    true
                );

                var objectIndexForEnemyCoverage = sphere.RelativeList
                    .Select(s => s.ObjectIndex)
                    .Append(objectIndex)
                    .ToList();
                sphere.CoverageList = new List<Models.Neighborhood.ObjectNeighborInfo>();

                foreach (var enemy in sphere.EnemyList)
                {
                    var enemyNeighborList = neighborhood.GetObjectNeigborList(
                        enemy.ObjectIndex,
                        objectIndexForEnemyCoverage
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
                        sphere.CoverageList = sphere.CoverageList.Append(coverage);
                    }
                }
                sphereList.Add(sphere);
            }
        );
        sphereList.CompleteAdding();

        System.Console.WriteLine($"Found {sphereList.Count} spheres");

        var allCoverageObjectIndexList = sphereList
            .SelectMany(s => s.CoverageList?.Select(ss => ss.ObjectIndex) ?? Array.Empty<int>())
            .ToHashSet();

        logger.WriteLine(
            "FindGroupByAcquaintance",
            $"Found {allCoverageObjectIndexList.Count} coverage objects",
            true
        );
        logger.WriteLine(
            "FindGroupByAcquaintance",
            string.Join(", ", allCoverageObjectIndexList.OrderBy(o => o)),
            true
        );

        return result;
    }
}
