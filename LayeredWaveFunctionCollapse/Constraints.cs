using System.Linq;
using System.Collections.Generic;
using System;

// TODO: rename 'bucket' to 'chunk'
namespace LayeredWaveFunctionCollapse
{
    public class AdjacencyList : Dictionary<(int firstTile, (int x, int y) dir), HashSet<int>> { }
    public class SubsumptionList : Dictionary<int, HashSet<int>> { }

    public class Constraints
    {
        public HashSet<int> StartingTiles { get; private set; } = new();
        public AdjacencyList AdjacencyConstraints { get; private set; } = new();
        public SubsumptionList SubsumptionConstraints { get; private set; } = new();
        public List<(HashSet<int> components, int id)> ChunkIDs { get; private set; } = new();

        private int unusedID = 0;

        public Constraints(int[,] input, int[] bucketSizes) 
        {
            var source = new Layer(input);
            var bucketsQueue = new Queue<int>(bucketSizes);

            unusedID = source.GetBiggestID() + 1;

            InferConstraintsRecursively(source, bucketsQueue);
            InferSubsumptionConstraints();
        }

        private void InferConstraintsRecursively(Layer source, Queue<int> bucketsQueue)
        {
            InferAdjacencyConstraints(source);

            if (!bucketsQueue.TryDequeue(out var bucketSize))
                foreach (var tile in source.GetUniqueTiles())
                    StartingTiles.Add(tile);
            else
                Utils.ForEachIn2DCartesianProductOf(bucketSize, (xOffset, yOffset) =>
                {
                    var nextLayer = CondenseIntoChunks(source, xOffset, yOffset, bucketSize);
                    InferConstraintsRecursively(nextLayer, new Queue<int>(bucketsQueue));
                });
        }

        private Layer CondenseIntoChunks(Layer source, int xOffset, int yOffset, int chunkSize)
        {
            // create next Layer
            var nextWidth = (source.Width - xOffset) / chunkSize;
            var nextHeight = (source.Height - yOffset) / chunkSize;
            var chunks = new int[nextWidth, nextHeight];
            chunks.ForEach((i, j) =>
            {
                var tilesInChunk = new HashSet<int>();

                // collect all tiles in one chunk
                Utils.ForEachIn2DCartesianProductOf(chunkSize, (chunkX, chunkY) =>
                    tilesInChunk.Add(source[(i * chunkSize) + chunkX + xOffset, (j * chunkSize) + chunkY + yOffset]));

                // set ids in the next layer
                var matchIndex = ChunkIDs.FindIndex(e => e.components.SetEquals(tilesInChunk));
                if (matchIndex != -1)
                {
                    chunks[i, j] = ChunkIDs[matchIndex].id; 
                }
                else
                {
                    chunks[i, j] = unusedID;
                    ChunkIDs.Add((tilesInChunk, unusedID));
                    unusedID++;
                }
            });
            return new Layer(chunks);
        }

        private void InferAdjacencyConstraints(Layer source)
        {
            var uniqueTiles = source.GetUniqueTiles();
            foreach (var tile in uniqueTiles)
                foreach (var dir in Utils.CardinalDirections)
                    AdjacencyConstraints.TryAdd((tile, dir), new HashSet<int>());

            source.IDs.ForEach((i, j) =>
            {
                foreach (var dir in Utils.CardinalDirections)
                    if (source.IndicesAreInRange(i + dir.x, j + dir.y))
                        AdjacencyConstraints[(source[i, j], dir)].Add(source[i + dir.x, j + dir.y]);
            });
        }

        public void InferSubsumptionConstraints()
        {
            foreach (var (components, id) in ChunkIDs)
                SubsumptionConstraints.Add(id, components);
        }
    }
}