using System.Collections.Generic;
using System.Linq;
using System;

public class WaveFunction<T>
{
    private List<T>[,] superPositions;

    private readonly List<(T tileA, (int x, int y) dir, T tileB)> adjacencyRules;

    private readonly Random random;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public WaveFunction(int width, int height, IEnumerable<T>[,] startingState, List<(T tileA, (int, int) dir, T tileB)> adjacencyRules, int seed)
    {
        this.adjacencyRules = adjacencyRules;
        random = new Random(seed);

        Width = width;
        Height = height;

        superPositions = new List<T>[width, height];
        ForEachSuperPosition((i, j, _) =>
        {
            superPositions[i, j] = new List<T>(startingState[i, j]);
        });

        // HACK: make sure every constraint is met at start
        ForEachSuperPosition((i, j, _) =>
        {
            Propagate(i, j);
        });
    }

    public void Collapse(int x, int y, T tile)
        => superPositions[x, y] = new List<T> { tile };

    public void Propagate(int originX, int originY)
    {
        var stack = new Stack<(int, int)>();
        stack.Push((originX, originY));

        while (stack.Any())
        {
            (int x, int y) = stack.Pop();

            foreach ((int dirX, int dirY) in Helper.CardinalDirections)
            {
                (int adjX, int adjY) = (x + dirX, y + dirY);

                if (IsOutOfBounds(adjX, adjY)) continue;

                var numberOfBannedTiles = superPositions[adjX, adjY]
                    .RemoveAll(adjTile => !adjacencyRules
                        .Any(rule =>
                            superPositions[x, y].Contains(rule.tileA) &&
                            rule.dir == (dirX, dirY) &&
                            Equals(rule.tileB, adjTile)));

                if (numberOfBannedTiles > 0) stack.Push((adjX, adjY));
            }
        }
    }

    private bool IsOutOfBounds(int x, int y) => x < 0 || x >= Width || y < 0 || y >= Height;

    public void Run()
    {
        int minimalCardinality;
        while ((minimalCardinality = GetLowestEntropy()) != int.MaxValue)
        {
            if (minimalCardinality == 0) return;

            // find all superpositions with the minimal cardinality
            var candidates = new List<(int x, int y)>();
            ForEachSuperPosition((i, j, superPosition) =>
            {
                if (superPosition.Count == minimalCardinality)
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

    public T[,] ExtractState()
    {
        var state = new T[Width, Height];
        ForEachSuperPosition((i, j, superPosition) => state[i, j] = superPosition.First());
        return state;
    }

    public void ForEachSuperPosition(Action<int, int, List<T>> action)
        => superPositions.ForEach(action);

    public int GetLowestEntropy()
    {
        var minCard = int.MaxValue;
        ForEachSuperPosition((i, j, superPosition) =>
        {
            var card = superPositions[i, j].Count;
            if (card < minCard && card != 1) minCard = card;
        });

        return minCard;
    }
}
