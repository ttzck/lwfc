using System.Drawing;
using System.Text.Json;

// TODO: generalize 2d array iteration

const int width = 4, height = 4;
int[] buckets = { 2, 2, 2 };

// infer adjacency rules from example
AdjacencyRules adjacencyRules;
using (var bitmap = new Bitmap($@"C:\Users\jnttz\OneDrive\Desktop\BA\LayeredWaveFunctionCollapse\LayeredWaveFunctionCollapse\ex3.png"))
{
    adjacencyRules = AdjacencyRules.Infer(bitmap, buckets);
}

// get a seed
int seed;
do { Console.WriteLine("Enter a seed:"); }
while (!int.TryParse(Console.ReadLine(), out seed));

// parse adjacency rules
//string? adjRulesPath = @"..\LayeredWaveFunctionCollapse\HighRiseConstraints.txt";
//var adjRulesText = File.ReadAllText(adjRulesPath ?? "");
//var adjacencyRules = AdjacencyRules.Parse(adjRulesText);

// create color table
//var allTiles = adjacencyRules.Select(r => r.firstTile).Union(adjacencyRules.Select(r => r.secondTile)).ToHashSet().ToArray();
//var tileColor = new Dictionary<string, Color>();
//for (int i = 0; i < allTiles.Length; i++)
//    tileColor.Add(allTiles[i], ColorFromHSV(360 / allTiles.Length * i, 1, 1));

// create first state to start with
var startingState = new List<string>[width, height];
for (int i = 0; i < width; i++)
    for (int j = 0; j < height; j++)
        startingState[i, j] = new List<string>(adjacencyRules.StartingTiles);

var wf = new WaveFunction<string>(width, height, startingState, adjacencyRules, seed);

var bucketsQueue = new Queue<int>(buckets);

while (true) 
{
    wf.Run();
    var state = wf.ExtractState();

    Console.WriteLine(wf);

    // create a bitmap from collapsed wave function and exit loop
    if (bucketsQueue.Count == 0)
    {
        var bitmap = new Bitmap(wf.Width, wf.Height);
        for (int i = 0; i < wf.Width; i++)
            for (int j = 0; j < wf.Height; j++)
                bitmap.SetPixel(i, j, ColorTranslator.FromHtml(state[i, j]));

        bitmap.Save($@"C:\Users\jnttz\OneDrive\Desktop\BA\LayeredWaveFunctionCollapse\LayeredWaveFunctionCollapse\out.png");
        break;
    }

    var bucketSize = bucketsQueue.Dequeue();
    // prepare next wave function
    var nextWidth = wf.Width * bucketSize;
    var nextHeight = wf.Height * bucketSize;
    var nextState = new List<string>[nextWidth, nextHeight];
    for (int i = 0; i < nextWidth; i++)
        for (int j = 0; j < nextHeight; j++)
            nextState[i, j] = new List<string>(adjacencyRules
                .Where(r => r.firstTile == state[i / bucketSize, j / bucketSize] && r.dir == (0, 0))
                .Select(r => r.secondTile));

    wf = new WaveFunction<string>(nextWidth, nextHeight, nextState, adjacencyRules, seed);
}

// ranges are 0 - 360 for hue, and 0 - 1 for saturation or value
static Color ColorFromHSV(double hue, double saturation, double value)
{
    int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
    double f = hue / 60 - Math.Floor(hue / 60);

    value *= 255;
    int v = Convert.ToInt32(value);
    int p = Convert.ToInt32(value * (1 - saturation));
    int q = Convert.ToInt32(value * (1 - f * saturation));
    int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

    if (hi == 0)
        return Color.FromArgb(255, v, t, p);
    else if (hi == 1)
        return Color.FromArgb(255, q, v, p);
    else if (hi == 2)
        return Color.FromArgb(255, p, v, t);
    else if (hi == 3)
        return Color.FromArgb(255, p, q, v);
    else if (hi == 4)
        return Color.FromArgb(255, t, p, v);
    else
        return Color.FromArgb(255, v, p, q);
}