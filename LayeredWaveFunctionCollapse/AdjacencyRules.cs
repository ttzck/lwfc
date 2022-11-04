using System.Drawing;
using System.Text.RegularExpressions;
using System.Linq;

public class AdjacencyRules : List<(string firstTile, (int x, int y) dir, string secondTile)>
{
    public List<string> StartingTiles { get; private set; } = new();

    private AdjacencyRules() { }

    public static AdjacencyRules Infer(Bitmap bitmap, int[] buckets)
    {
        var result = new AdjacencyRules();
        var layer = new string[bitmap.Width, bitmap.Height];
        var bucketsQueue = new Queue<int>(buckets);

        // create first layer
        for (int i = 0; i < bitmap.Width; i++)
            for (int j = 0; j < bitmap.Height; j++)
                layer[i, j] = GetHtmlColor(bitmap, i, j);

        result.InferFromLayer(layer);


        while (bucketsQueue.Count > 0)
        {
            var bucketSize = bucketsQueue.Dequeue();
            int nextWidth = (int)Math.Ceiling((float)layer.GetLength(0) / bucketSize);
            int nextHeight = (int)Math.Ceiling((float)layer.GetLength(1) / bucketSize);
            // create buckets for next layer
            var nextLayerBuckets = new HashSet<string>[nextWidth, nextHeight];

            for (int i = 0; i < nextLayerBuckets.GetLength(0); i++)
                for (int j = 0; j < nextLayerBuckets.GetLength(1); j++)
                    nextLayerBuckets[i, j] = new HashSet<string>();

            // populate buckets
            for (int i = 0; i < layer.GetLength(0); i++)
                for (int j = 0; j < layer.GetLength(1); j++)
                    nextLayerBuckets[i / bucketSize, j / bucketSize].Add(layer[i, j]);

            // create layer from buckets
            var nextLayer = new string[nextLayerBuckets.GetLength(0), nextLayerBuckets.GetLength(1)];
            for (int i = 0; i < nextLayer.GetLength(0); i++)
                for (int j = 0; j < nextLayer.GetLength(1); j++)
                    nextLayer[i, j] = $"[{string.Join(", ", nextLayerBuckets[i, j].OrderBy(i => i))}]";

            // infer subsumption rules
            for (int i = 0; i < nextLayer.GetLength(0); i++)
            {
                for (int j = 0; j < nextLayer.GetLength(1); j++)
                {
                    foreach (var color in nextLayerBuckets[i, j])
                    {
                        var tuple = (nextLayer[i, j], (0, 0), color);
                        if (!result.Contains(tuple)) result.Add(tuple);
                    }
                }
            }

            layer = nextLayer;
            result.InferFromLayer(layer);
        }

        for (int i = 0; i < layer.GetLength(0); i++)
        {
            for (int j = 0; j < layer.GetLength(1); j++)
            {
                if (!result.StartingTiles.Contains(layer[i, j]))
                    result.StartingTiles.Add(layer[i, j]);
            }
        }

        return result;
    }

    private void InferFromLayer(string[,] layer)
    {
        var width = layer.GetLength(0);
        var height = layer.GetLength(1);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                foreach (var dir in Helper.CardinalDirections)
                {
                    if (i + dir.x >= 0 && i + dir.x < width
                        && j + dir.y >= 0 && j + dir.y < height)
                    {
                        var tuple = (layer[i, j], dir, layer[i + dir.x, j + dir.y]);
                        if (!Contains(tuple)) Add(tuple);
                    }
                }
            }
        }
    }

    private static string GetHtmlColor(Bitmap bitmap, int i, int j) => ColorTranslator.ToHtml(bitmap.GetPixel(i, j));

    public static AdjacencyRules Parse(string s)
    {
        var result = new AdjacencyRules();

        var rules = s.Replace("\n", string.Empty)
            .Replace("\r", string.Empty)
            .Replace(" ", string.Empty)
            .Split(';');
        foreach (var rule in rules)
            result.ParseRule(rule);

        return result;
    }

    private void ParseRule(string rule)
    {
        if (rule == string.Empty) return;
        var r = Regex.Split(rule, @"(\-)|(\|)|(\+)|(\:)");
        ParseRule(r);
    }

    private void ParseRule(string[] r)
    {
        if (r.Length == 1) ParseStartingTiles(r[0]);
        else ParseRule(r[0], r[1], r[2].Split(","));
    }

    private void ParseStartingTiles(string r)
    {
        StartingTiles.AddRange(r.Split(","));
    }

    private void ParseRule(string a, string @operator, string[] bArr)
    {
        foreach(var b in bArr) ParseRule(a, @operator, b);
    }

    private void ParseRule(string a, string @operator, string b)
    {
        switch (@operator)
        {
            case ":":
                Add((a, (0, 0), b));
                break;

            case "-":
                Add((a, (1, 0), b));
                Add((b, (-1, 0), a));
                break;

            case "|":
                Add((a, (0, -1), b));
                Add((b, (0, 1), a));
                break;

            case "+":
                ParseRule(a, "-", b);
                ParseRule(a, "|", b);
                break;
        }
    }
}