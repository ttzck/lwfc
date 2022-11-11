using System;
using System.Drawing;

namespace LayeredWaveFunctionCollapse
{
    public static class Utils
    {
        public static readonly (int x, int y)[] CardinalDirections = new (int, int)[] { (1, 0), (0, 1), (-1, 0), (0, -1) };

        public static void ForEach<T>(this T[,] array, Action<int, int> action)
        {
            for (var i = 0; i < array.GetLength(0); i++)
                for (var j = 0; j < array.GetLength(1); j++)
                    action(i, j);
        }

        public static void ForEachIn2DCartesianProductOf(int x, Action<int, int> action)
        {
            for (var i = 0; i < x; i++)
                for (var j = 0; j < x; j++)
                    action(i, j);
        }
    }
}