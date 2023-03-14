using System.Collections.Concurrent;

namespace GenomExperiment.Methods;

public class FindNiosyObjectByDensity
{
    public static HashSet<int> Run(Helpers.Logger logger, Models.ObjectSet set, Models.Neighborhood neighborhood)
    {
        var noisyObjectIndexList = new BlockingCollection<int>();

        Parallel.ForEach(set.Objects.Keys, (objectIndex) =>
        {
            var objectNeighborList = neighborhood.GetObjectNeigborList(objectIndex);
            var radius = objectNeighborList.Where(w => w.ObjectClassValue != set.Objects[objectIndex].ClassValue).Min(m => m.Distance);
            var enemyList = objectNeighborList.Where(w => w.Distance == radius && w.ObjectClassValue != set.Objects[objectIndex].ClassValue);
            if (enemyList.Count() == 1)
            {
                var radius2 = objectNeighborList.Where(w => w.ObjectIndex != enemyList.ElementAt(0).ObjectIndex && w.ObjectClassValue != set.Objects[objectIndex].ClassValue).Min(m => m.Distance);
                var relativeList2 = objectNeighborList.Where(w => w.Distance < radius2 && w.ObjectClassValue == set.Objects[objectIndex].ClassValue);
                var relativeList = objectNeighborList.Where(w => w.Distance < radius);

                var sum = relativeList.Sum(s => 1 - s.Distance / radius);
                var sum2 = relativeList2.Sum(s => 1 - s.Distance / radius2);
                if (sum2 - sum > 0.1M)
                {
                    noisyObjectIndexList.Add(enemyList.ElementAt(0).ObjectIndex);
                }
            }
        });
        noisyObjectIndexList.CompleteAdding();
        var result = noisyObjectIndexList.ToHashSet();

        logger.WriteLine("FindNiosyObjectByDensity", $"Found {result.Count} noisy object(s)", true);
        foreach (var item in result)
        {
            logger.WriteLine("FindNiosyObjectByDensity", item.ToString(), true);
        }

        return result;
    }
}