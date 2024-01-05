using System.Drawing;

namespace OreTuner
{
    public class Program
    {
        const int colorDepth = 3;
        const byte keenNoOre = 255;
        const int gens = 100;

        static void Main(string[] args)
        {
            Random rnd = new();
            foreach (string arg in args)
            {
                using Bitmap originalBitmap = new(arg);
                OreMap map = new(originalBitmap);
                map.RandomizeTheChannel(rnd);
                for (int g = 0; g < gens; g++)
                {
                    map.RunGeneration();
                    var outputPath = arg + $".Output{g}.png";
                    map.SaveImage(outputPath);
                    Console.WriteLine($"{outputPath} written.");
                }
            }
            Console.WriteLine("Done.");
        }

        public class OreMap
        {
            List<byte> blueChannel = new();
            readonly int w;
            readonly Dictionary<byte, int> neighbors = new();
            readonly List<byte> newBlue;
            readonly Bitmap originalBitmap;

            public OreMap(Bitmap originalBitmap)
            {
                this.originalBitmap = originalBitmap;
                //Init the blue channel.
                var count = (originalBitmap.Height + 2) * (originalBitmap.Width + 2);
                blueChannel = new(count);
                blueChannel.AddRange(Enumerable.Repeat(keenNoOre, count));

                for (int y = 0; y < originalBitmap.Height; y++)
                {
                    for (int x = 0; x < originalBitmap.Width; x++)
                    {
                        blueChannel[x + 1 + (y + 1) * (originalBitmap.Width + 2)] = originalBitmap.GetPixel(x, y).B;
                    }
                }
                w = originalBitmap.Width + 2;
                newBlue = new(blueChannel.Count);
                newBlue.AddRange(blueChannel);
            }

            public void RandomizeTheChannel(Random rnd)
            {
                //Randomize the Blue Channel
                for (int i = 0; i < blueChannel.Count; ++i)
                {
                    if (blueChannel[i] != keenNoOre) blueChannel[i] += (byte)(rnd.Next(colorDepth) * 16);
                }
            }

            public void RunGeneration()
            {
                for (int y = 1; y <= originalBitmap.Height; y++)
                {
                    for (int x = 1; x <= originalBitmap.Width; x++)
                    {
                        int index = x + y * w;
                        if (blueChannel[index] == keenNoOre) continue;
                        neighbors.Clear();
                        for (int i = -1; i < 2; i++)
                        {
                            for (int j = -1; j < 2; j++)
                            {
                                var v = blueChannel[index + i + j * w];
                                if (neighbors.TryGetValue(v, out int amt))
                                {
                                    neighbors[v] = amt + 1;
                                }
                                else neighbors.Add(v, 1);
                            }
                        }
                        if (neighbors.ContainsKey(keenNoOre)) neighbors[keenNoOre] = 0;
                        newBlue[index] = neighbors.Keys.OrderBy(x => neighbors[x]).Last();
                    }
                }
                blueChannel = newBlue;
            }

            public void SaveImage(string outputPath)
            {
                using Bitmap newBitmap = new(originalBitmap.Width, originalBitmap.Height);
                //Write out.
                for (int y = 1; y <= originalBitmap.Height; y++)
                {
                    for (int x = 1; x <= originalBitmap.Width; x++)
                    {
                        var oldColor = originalBitmap.GetPixel(x - 1, y - 1);
                        var newColor = Color.FromArgb(oldColor.R, oldColor.G, blueChannel[x + y * w]);
                        newBitmap.SetPixel(x - 1, y - 1, newColor);
                    }
                }
                newBitmap.Save(outputPath);
            }
        }
    }
}