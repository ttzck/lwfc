using System.Collections.Generic;

// TODO: optimize SubsumptionRules (abstractTile -> List{concreteTiles})
// TODO: shift grid when extracting constraints
namespace LayeredWaveFunctionCollapse
{
    public class Layer
    { 
        public int[,] IDs { get; set; }

        public int Width => IDs.GetLength(0);
        public int Height => IDs.GetLength(1);

        public int this[int i, int j] => IDs[i, j];

        public Layer(int[,] iDs)
        {
            IDs = iDs;
        }

        public int GetBiggestID()
        {
            var biggestID = int.MinValue;
            IDs.ForEach((i, j) =>
            {
                if (IDs[i, j] > biggestID) biggestID = IDs[i, j];
            });
            return biggestID;
        }

        public bool IndicesAreInRange(int i, int j) => 
            i >= 0 && i < Width &&
            j >= 0 && j < Height;

        public HashSet<int> GetUniqueTiles()
        {
            var uniqueTiles = new HashSet<int>();
            IDs.ForEach((i, j) => uniqueTiles.Add(IDs[i, j]));
            return uniqueTiles;
        }
    }
}