using System.Collections.Generic;
using System.Linq;


namespace LayeredWaveFunctionCollapse
{
    public class LayeredWaveFunction
    {
        private readonly int seed;
        public Constraints Constraints { get; private set; }
        private WaveFunction wf;
        private readonly int[] bucketSizes;

        // width and height refer to the dimensions of most top level layer, the end result will have a different size
        public LayeredWaveFunction(int[,] source, int seed, int width, int height, int[] bucketSizes)
        {
            this.seed = seed;
            Constraints = new Constraints(source, bucketSizes);
            this.bucketSizes = bucketSizes;

            // create first state to start the wfc with
            var startupState = new List<int>[width, height];
            startupState.ForEach((i, j) =>
                startupState[i, j] = new List<int>(Constraints.StartingTiles));

            wf = new WaveFunction(width, height, startupState, Constraints.AdjacencyConstraints, seed);
        }

        public List<int[,]> Run()
        {
            var bucketsQueue = new Queue<int>(bucketSizes.Reverse());
            var results = new List<int[,]>();

            wf.Run();
            var state = wf.ExtractState();
            results.Add(state);
            
            while (bucketsQueue.TryDequeue(out var bucketSize))
            {
                // prepare next wave function
                var nextWidth = wf.Width * bucketSize;
                var nextHeight = wf.Height * bucketSize;
                var nextState = new List<int>[nextWidth, nextHeight];

                var successful = true;
                nextState.ForEach((i, j) =>
                {
                    if (Constraints.SubsumptionConstraints
                        .TryGetValue(state[i / bucketSize, j / bucketSize], out var concreteTiles))
                        nextState[i, j] = new List<int>(concreteTiles);
                    else
                        successful = false;
                });
                if (!successful) break;
                wf = new WaveFunction(nextWidth, nextHeight, nextState, Constraints.AdjacencyConstraints, seed);

                wf.Run();
                state = wf.ExtractState();
                results.Add(state);
            }

            return results;
        }
    }
}