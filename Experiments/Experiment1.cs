namespace GenomExperiment.Experiments;

public class Experiment1
{
    public static void Run()
    {
        var logger = new Helpers.Logger("Experiment1");

        var set = Models.ObjectSet.FromFileData(Path.Combine("Data", "ARF4-205.dat"), 0);
        if (set is null)
        {
            logger.WriteLine("SetInfo", "Set is null.", true);
            return;
        }

        set.ChangeToOnly2Class();

        var removedObjectIndexList = new HashSet<int>();
        set.RemoveDuplicates(out removedObjectIndexList);

        logger.WriteLine("SetInfo", set.ToString(), true);

        var distFunc = Metrics.MetricFunctionGetter.GetMetric(set, "Experiment1");

        var neighborhood = new Models.Neighborhood();
        var activeFeatures = Enumerable.Range(0, set.Features.Length);
        foreach (var i in set.Objects.Keys)
        {
            foreach (var j in set.Objects.Keys)
            {
                if (i <= j)
                    continue;
                neighborhood.Add(
                    new Models.Neighborhood.Neighbor(
                        set.Objects[i].Index,
                        set.Objects[j].Index,
                        set.Objects[i].ClassValue ?? -1,
                        set.Objects[j].ClassValue ?? -1,
                        distFunc(set.Objects[i], set.Objects[j], set.Features, activeFeatures)
                    )
                );
            }
        }
        logger.WriteLine("Neighborhood", $"Neighborhood count is {neighborhood.Count}", true);
        logger.WriteLine("Neighborhood", neighborhood.ToString(), true);

        var noisyObjectIndexList = Methods.FindNiosyObjectByDensity.Run(logger, set, neighborhood);
        set.RemoveObject(noisyObjectIndexList);
        int counter = 0;
        do
        {
            var item = neighborhood[counter];
            if (
                !set.Objects.Keys.Contains(item.Object1Index)
                || !set.Objects.Keys.Contains(item.Object2Index)
            )
            {
                neighborhood.Remove(item);
            }
            else
            {
                counter++;
            }
        } while (counter < neighborhood.Count);

        foreach (var item in neighborhood)
        {
            if (
                !set.Objects.Keys.Contains(item.Object1Index)
                || !set.Objects.Keys.Contains(item.Object2Index)
            )
                neighborhood.Remove(item);
        }

        var groups = Methods.FindGroupByCoverage.Run(logger, set, neighborhood);
    }
}
