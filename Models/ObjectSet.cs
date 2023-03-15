using System.Globalization;

namespace GenomExperiment.Models
{
    public class ObjectSet
    {
        public Dictionary<int, ObjectInfo> Objects { get; }

        public Feature[] Features { get; }

        public int ClassValue { get; }

        public string Name { get; }

        public ObjectSet(
            string name,
            Dictionary<int, ObjectInfo> objects,
            Feature[] features,
            int classValue = 1
        )
        {
            Name = name;
            Objects = objects;
            Features = features;
            ClassValue = classValue;

            if (objects.Count == 0)
                throw new System.ArgumentException("Objects weren't given.");

            if (features.Length == 0)
                throw new System.ArgumentException("Features weren't given.");

            if (objects.Any(a => !a.Value.ClassValue.HasValue))
                throw new System.ArgumentException("Objects class weren't given.");

            foreach (var item in objects)
            {
                if (features.Length != item.Value.Data.Length)
                    throw new System.ArgumentException(
                        $"Length of objects #{item.Key} columns doesn't match to features length."
                    );
            }
        }

        public override string ToString()
        {
            return $@"ObjectSet = ""{Name}""
               , Objects count = {Objects.Count} 
               , Features count = {Features.Length} 
               , Class value = {ClassValue}
               , Class objects = {ClassObjectCount}
               , Non Class objects = {NonClassObjectCount}
               , ClassValues = {string.Join(", ", GetClassValues())}";
        }

        public int ClassObjectCount
        {
            get { return Objects.Count(w => w.Value.ClassValue == ClassValue); }
        }
        public int NonClassObjectCount
        {
            get { return Objects.Count(w => w.Value.ClassValue != ClassValue); }
        }

        public IEnumerable<int> GetClassValues()
        {
            return Objects.Select(s => s.Value.ClassValue.GetValueOrDefault(-1)).Distinct();
        }

        public void ChangeToOnly2Class()
        {
            foreach (var item in this.Objects)
            {
                if (item.Value.ClassValue != this.ClassValue)
                    item.Value.ClassValue = this.ClassValue + 1;
            }
        }

        public void RemoveObject(IEnumerable<int> objectIndexList)
        {
            foreach (var i in this.Objects.Keys)
            {
                if (objectIndexList.Contains(i))
                    this.Objects.Remove(i);
            }
        }

        public void RemoveDuplicates(out HashSet<int> notUniqueIndexes)
        {
            notUniqueIndexes = new HashSet<int>();
            foreach (var i in this.Objects.Keys)
            {
                if (notUniqueIndexes.Contains(i))
                    continue;
                foreach (var j in this.Objects.Keys)
                {
                    if (i == j || notUniqueIndexes.Contains(j))
                        continue;

                    if (this.Objects[i].EqualsByValues(this.Objects[j]))
                    {
                        if (this.Objects[i].ClassValue != this.Objects[j].ClassValue)
                        {
                            notUniqueIndexes.Add(i);
                        }
                        notUniqueIndexes.Add(j);
                        this.Objects.Remove(j);
                    }
                }
                if (notUniqueIndexes.Contains(i))
                    this.Objects.Remove(i);
            }
        }

        internal static ObjectSet? FromFileData(string path, int classValue = 1)
        {
            using (var file = new StreamReader(path))
            {
                var fline = file.ReadLine()?.Split('\t');
                if (fline is null)
                    return null;
                var objectsCount = int.Parse(fline[0]);
                var featuresCount = int.Parse(fline[1]);
                var objects = new Dictionary<int, ObjectInfo>();
                for (int i = 0; i < objectsCount; i++)
                {
                    var line = file.ReadLine()?.Split('\t');
                    if (line is null || line.Any() == false)
                        continue;
                    objects[i] = new ObjectInfo
                    {
                        Index = i,
                        Data = line.Take(featuresCount)
                            .Select(
                                s =>
                                    decimal.Parse(
                                        s,
                                        NumberStyles.Float,
                                        CultureInfo.InvariantCulture
                                    )
                            )
                            .ToArray(),
                        ClassValue = int.Parse(line[featuresCount])
                    };
                }
                int ind = 0;
                var features = file.ReadLine()
                    ?.Split('\t')
                    .Take(featuresCount)
                    .Select(s => new Feature { IsContinuous = s == "1", Name = $"Ft {ind++}" })
                    .ToArray();
                return new ObjectSet(path, objects, features ?? Array.Empty<Feature>(), classValue);
            }
        }

        internal void ToFileData(string path)
        {
            using (var file = new StreamWriter(path))
            {
                file.WriteLine($"{Objects.Count}\t{Features.Length}\t{GetClassValues().Count()}");
                for (int i = 0; i < Objects.Count; i++)
                {
                    file.WriteLine(
                        $"{string.Join('\t', Objects[i].Data)}\t{Objects[i].ClassValue}"
                    );
                }
                foreach (var ft in Features)
                {
                    if (ft.IsContinuous)
                        file.Write("1\t");
                    else
                        file.Write("0\t");
                }
                file.Write("0");
            }
        }

        internal void ToFileData(string path, HashSet<int> deletedObjects, List<int> activeFeatures)
        {
            using (var file = new StreamWriter(path + ".features"))
            {
                file.WriteLine("IsContinuous|Name");
                file.WriteLine("5|Class feature");
                for (int i = 0; i < Features.Length; i++)
                {
                    if (activeFeatures.Contains(i))
                    {
                        var ft = Features[i];
                        file.WriteLine($"{ft.IsContinuous}|{ft.Name}");
                    }
                }
            }

            using (var indexes = new StreamWriter(path + ".indexes"))
            using (var file = new StreamWriter(path))
            {
                file.WriteLine(
                    $"{Objects.Count - deletedObjects?.Count ?? 0}\t{activeFeatures.Count}\t{GetClassValues().Count()}"
                );

                for (int i = 0; i < Objects.Count; i++)
                {
                    if (deletedObjects?.Contains(i) == true)
                        continue;

                    indexes.WriteLine(i);
                    for (int ft = 0; ft < this.Features.Length; ft++)
                    {
                        if (activeFeatures.Contains(ft))
                        {
                            file.Write($"{Objects[i][ft]}\t");
                        }
                    }
                    file.WriteLine(Objects[i].ClassValue);
                }

                for (int i = 0; i < Features.Length; i++)
                {
                    if (activeFeatures.Contains(i))
                    {
                        var ft = Features[i];
                        file.Write($"{(ft.IsContinuous ? 1 : 0)}\t");
                    }
                }
            }
        }
    }
}
