using System.Collections.Generic;
using System.Linq;
using System;

namespace LayeredWaveFunctionCollapse
{
    public class WaveFunction
    {
        private readonly List<int>[,] superPositions;

        private readonly AdjacencyList adjacencyConstraints;

        private readonly Random random;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public WaveFunction(int width, int height, List<int>[,] startingState, AdjacencyList adjacencyConstraints, int seed)
        {
            this.adjacencyConstraints = adjacencyConstraints;
            random = new Random(seed);

            Width = width;
            Height = height;

            superPositions = new List<int>[width, height];
            ForEachSuperPosition((i, j) =>
                superPositions[i, j] = new List<int>(startingState[i, j]));

            // HACK: make sure every constraint is met at start
            ForEachSuperPosition((i, j) =>
                Propagate(i, j));
        }

        public void Collapse(int x, int y, int tile)
            => superPositions[x, y] = new List<int> { tile };

        public void Propagate(int originX, int originY)
        {
            var stack = new Stack<(int, int)>();
            stack.Push((originX, originY));

            while (stack.Any())
            {
                (int x, int y) = stack.Pop();

                foreach ((int dirX, int dirY) in Utils.CardinalDirections)
                {
                    (int adjX, int adjY) = (x + dirX, y + dirY);

                    if (IsOutOfBounds(adjX, adjY)) continue;

                    var numberOfBannedTiles = superPositions[adjX, adjY].RemoveAll(adjTile => 
                            !superPositions[x, y].Any(tile =>
                                adjacencyConstraints[(tile, (dirX, dirY))].Contains(adjTile)));

                    if (superPositions[adjX, adjY].Count is 0) return;

                    if (numberOfBannedTiles > 0) stack.Push((adjX, adjY));
                }
            }
        }

        private bool IsOutOfBounds(int x, int y) => x < 0 || x >= Width || y < 0 || y >= Height;

        public void Run()
        {
            int minimalCardinality;
            while ((minimalCardinality = GetLowestEntropy()) is not int.MaxValue)
            {
                if (minimalCardinality is 0) return;

                // find all superpositions with the minimal cardinality
                var candidates = new List<(int x, int y)>();
                ForEachSuperPosition((i, j) =>
                {
                    if (superPositions[i, j].Count == minimalCardinality)
                    {
                        candidates.Add((i, j));
                    }
                });

                // and collapse a random one
                (var x, var y) = candidates[random.Next(candidates.Count)];
                var tile = superPositions[x, y][random.Next(minimalCardinality)];
                Collapse(x, y, tile);

                Propagate(x, y);
            }
        }

        // tiles with contradiction have ID = -1 and uncollapsed tiles ID = -2
        public int[,] ExtractState()
        {
            var state = new int[Width, Height];
            ForEachSuperPosition((i, j) =>
            {
                state[i, j] = superPositions[i, j].Count switch
                {
                    0 => -1,
                    1 => superPositions[i, j][0],
                    _ => -2,
                };
            });
            return state;
        }

        public void ForEachSuperPosition(Action<int, int> action)
            => superPositions.ForEach(action);

        public int GetLowestEntropy()
        {
            var minCard = int.MaxValue;
            ForEachSuperPosition((i, j) =>
            {
                var card = superPositions[i, j].Count;
                if (card < minCard && card != 1) minCard = card;
            });

            return minCard;
        }
    }
}