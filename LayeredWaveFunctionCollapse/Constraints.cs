using System.Linq;
using System.Collections.Generic;
using System;

// TODO: optimize SubsumptionRules (abstractTile -> List{concreteTiles})

namespace LayeredWaveFunctionCollapse
{
    public class AdjacencyList : List<(int firstTile, (int x, int y) dir, int secondTile)> { }
    public class SubsumptionList : List<(int abstractTile, int concreteTile)> { }

    public class Constraints
    {
        public List<int> StartingTiles { get; private set; } = new();
        public AdjacencyList AdjacencyConstraints { get; private set; } = new();
        public SubsumptionList SubsumptionConstraints { get; private set; } = new();

        private int unusedID = 0;

        public Constraints(int[,] source, int[] bucketSizes) 
        {
            var bucketsQueue = new Queue<int>(bucketSizes);
            source.ForEach((i, j) =>
            {
                if (source[i, j] >= unusedID)
                    unusedID = source[i, j] + 1;
            });

            InferAdjacencyRules(source);

            while (bucketsQueue.TryDequeue(out var bucketSize))
            {
                source = InferSubsumptionRules(source, bucketSize);
                InferAdjacencyRules(source);
            }

            source.ForEach((i, j) =>
            {
                if (!StartingTiles.Contains(source[i, j]))
                    StartingTiles.Add(source[i, j]);
            });
        }

        private int[,] InferSubsumptionRules(int[,] layer, int bucketSize)
        {
            int nextWidth = (int)Math.Ceiling((float)layer.GetLength(0) / bucketSize);
            int nextHeight = (int)Math.Ceiling((float)layer.GetLength(1) / bucketSize);
            // create buckets for next layer
            var bucketsLayer = new HashSet<int>[nextWidth, nextHeight];
            bucketsLayer.ForEach((i, j) =>
                bucketsLayer[i, j] = new HashSet<int>());

            // populate buckets
            layer.ForEach((i, j) =>
                bucketsLayer[i / bucketSize, j / bucketSize].Add(layer[i, j]));
            
            // assign IDs to buckets
            var bucketIDs = new List<(HashSet<int> bucket, int id)>();
            bucketsLayer.ForEach((i, j) =>
            {
                if (!bucketIDs.Any((tuple) => tuple.bucket.SetEquals(bucketsLayer[i, j])))
                    bucketIDs.Add((bucketsLayer[i, j], unusedID++));
            });

            // create layer from buckets
            var nextLayer = new int[bucketsLayer.GetLength(0), bucketsLayer.GetLength(1)];
            nextLayer.ForEach((i, j) =>
                nextLayer[i, j] = bucketIDs
                    .First((tuple) => tuple.bucket.SetEquals(bucketsLayer[i, j]))
                        .id);

            // infer subsumption rules
            nextLayer.ForEach((i, j) =>
            {
                foreach (var color in bucketsLayer[i, j])
                {
                    var tuple = (nextLayer[i, j], color);
                    if (!SubsumptionConstraints.Contains(tuple))
                        SubsumptionConstraints.Add(tuple);
                }
            });
            return nextLayer;
        }

        private void InferAdjacencyRules(int[,] layer)
        {
            var width = layer.GetLength(0);
            var height = layer.GetLength(1);
            layer.ForEach((i, j) =>
            {
                foreach (var dir in Helper.CardinalDirections)
                {
                    if (i + dir.x >= 0 && i + dir.x < width
                        && j + dir.y >= 0 && j + dir.y < height)
                    {
                        var tuple = (layer[i, j], dir, layer[i + dir.x, j + dir.y]);
                        if (!AdjacencyConstraints.Contains(tuple))
                            AdjacencyConstraints.Add(tuple);
                    }
                }
            });
        }
    }
}