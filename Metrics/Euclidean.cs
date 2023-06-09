using GenomExperiment.Interfaces;
using GenomExperiment.Models;

namespace GenomExperiment.Metrics
{
    public class Euclidean : IMetric
    {
        public decimal Calculate(ObjectInfo obj1, ObjectInfo obj2, Feature[] features, IEnumerable<int> activeFeaturesIndexes)
        {
            var result = 0M;

            foreach (var i in activeFeaturesIndexes)
            {
                if (features[i].IsContinuous)
                {
                    var r = obj1[i] - obj2[i];
                    result += r * r;
                }
                else
                    throw new NotImplementedException();
            }

            return (decimal)Math.Sqrt((double)result);
        }

        public bool CanCalculate(Feature[] features)
        {
            return features.All(f => f.IsContinuous);
        }
    }

    // public class Hamming : IMetric
    // {
    //     public decimal Calculate(ObjectInfo obj1, ObjectInfo obj2, Feature[] features, IEnumerable<int> activeFeaturesIndexes)
    //     {
    //         var result = 0M;

    //         foreach (var i in activeFeaturesIndexes)
    //         {
    //             if (features[i].IsContinuous)
    //             {
    //                 var r = obj1[i] - obj2[i];
    //                 result += r * r;
    //             }
    //             else
    //                 throw new NotImplementedException();
    //         }

    //         return (decimal)Math.Sqrt((double)result);
    //     }

    //     public bool CanCalculate(Feature[] features)
    //     {
    //         return features.All(f => f.IsContinuous);
    //     }
    // }
}