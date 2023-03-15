using System.Text;

namespace GenomExperiment.Models;

public class Neighborhood : List<Neighborhood.Neighbor>
{
    public class Neighbor
    {
        public Neighbor(
            int object1Index,
            int object2Index,
            int object1ClassValue,
            int object2ClassValue,
            decimal distance
        )
        {
            Object1Index = object1Index;
            Object2Index = object2Index;
            Object1ClassValue = object1ClassValue;
            Object2ClassValue = object2ClassValue;
            Distance = distance;
        }

        public int Object1Index { get; set; }
        public int Object2Index { get; set; }
        public int Object1ClassValue { get; set; }
        public int Object2ClassValue { get; set; }
        public decimal Distance { get; set; }

        public override string ToString()
        {
            return $"[{Object1Index} ({Object1ClassValue}), {Object2Index} ({Object2ClassValue})] {Distance: 0.000000}";
        }
    }

    public class ObjectNeighborInfo
    {
        public ObjectNeighborInfo(int objectIndex, int objectClassValue, decimal distance)
        {
            ObjectIndex = objectIndex;
            ObjectClassValue = objectClassValue;
            Distance = distance;
        }

        public int ObjectIndex { get; set; }
        public int ObjectClassValue { get; set; }
        public decimal Distance { get; set; }

        public override string ToString()
        {
            return $"{ObjectIndex} ({ObjectClassValue}) - {Distance: 0.000000}";
        }
    }

    public IEnumerable<ObjectNeighborInfo> GetObjectNeigborList(int objectIndex)
    {
        return this.Where(w => w.Object1Index == objectIndex || w.Object2Index == objectIndex)
            .Select(
                s =>
                    new ObjectNeighborInfo(
                        (s.Object1Index == objectIndex) ? s.Object2Index : s.Object1Index,
                        (s.Object1Index == objectIndex) ? s.Object2ClassValue : s.Object1ClassValue,
                        s.Distance
                    )
            );
    }

    public IEnumerable<ObjectNeighborInfo> GetObjectNeigborList(
        int objectIndex,
        IEnumerable<int> includedObjectIndexList
    )
    {
        var result = this.Where(
                w =>
                    (w.Object1Index == objectIndex || w.Object2Index == objectIndex)
                    && (
                        includedObjectIndexList.Contains(w.Object1Index)
                        || includedObjectIndexList.Contains(w.Object2Index)
                    )
            )
            .Select(
                s =>
                    new ObjectNeighborInfo(
                        (s.Object1Index == objectIndex) ? s.Object2Index : s.Object1Index,
                        (s.Object1Index == objectIndex) ? s.Object2ClassValue : s.Object1ClassValue,
                        s.Distance
                    )
            );

        return result;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var item in this)
        {
            sb.AppendLine(item.ToString());
        }
        return sb.ToString();
    }
}
