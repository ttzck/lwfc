using System.Drawing;
using System.Text.Json;

const int width = 8, height = 8;
int[] buckets = { 2, 2, 2 };

// TODO: refactor for generalized usage (i.e. Unity), abstract away bitmaps, string arrays only, namespaces

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

// create first state to start with
var startingState = new List<string>[width, height];
startingState.ForEach((i, j, l) =>
    startingState[i, j] = new List<string>(adjacencyRules.StartingTiles));

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
        state.ForEach((i, j, s) =>
            bitmap.SetPixel(i, j, ColorTranslator.FromHtml(s)));

        bitmap.Save($@"C:\Users\jnttz\OneDrive\Desktop\BA\LayeredWaveFunctionCollapse\LayeredWaveFunctionCollapse\out.png");
        break;
    }

    var bucketSize = bucketsQueue.Dequeue();
    // prepare next wave function
    var nextWidth = wf.Width * bucketSize;
    var nextHeight = wf.Height * bucketSize;
    var nextState = new List<string>[nextWidth, nextHeight];

    nextState.ForEach((i, j, l) =>
        nextState[i, j] = new List<string>(adjacencyRules
            .Where(r => r.firstTile == state[i / bucketSize, j / bucketSize] && r.dir == (0, 0))
            .Select(r => r.secondTile)));

    wf = new WaveFunction<string>(nextWidth, nextHeight, nextState, adjacencyRules, seed);
}