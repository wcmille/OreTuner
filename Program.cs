using System.Drawing;

namespace OreTuner
{
    public class Program
    {
        const byte keenNoOre = 255;
        const int gens = 6;

        static void Main(string[] args)
        {
            var oreStrategy = new OreStrategy();
            foreach (string arg in args)
            {
                using Bitmap originalBitmap = new(arg);
                OreMap map = new(originalBitmap);
                map.ContextlessPass(oreStrategy);
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
            List<byte> oreData = new();
            readonly int w;
            readonly Dictionary<byte, int> neighbors = new();
            readonly List<byte> newOreData;
            readonly Bitmap originalBitmap;

            public OreMap(Bitmap originalBitmap)
            {
                this.originalBitmap = originalBitmap;
                var count = (originalBitmap.Height + 2) * (originalBitmap.Width + 2);
                oreData = new(count);
                oreData.AddRange(Enumerable.Repeat(keenNoOre, count));

                for (int y = 0; y < originalBitmap.Height; y++)
                {
                    for (int x = 0; x < originalBitmap.Width; x++)
                    {
                        oreData[x + 1 + (y + 1) * (originalBitmap.Width + 2)] = originalBitmap.GetPixel(x, y).B;
                    }
                }
                w = originalBitmap.Width + 2;
                newOreData = new(oreData.Count);
                newOreData.AddRange(oreData);
            }

            public void ContextlessPass(IOreStrategy oreStrategy)
            {
                //Randomize the Blue Channel
                for (int i = 0; i < oreData.Count; ++i)
                {
                    if (oreData[i] != keenNoOre)
                    {
                        oreData[i] = oreStrategy.ContextlessPass(oreData[i]);
                    }
                }
            }

            public void RunGeneration()
            {
                for (int y = 1; y <= originalBitmap.Height; y++)
                {
                    for (int x = 1; x <= originalBitmap.Width; x++)
                    {
                        int index = x + y * w;
                        if (oreData[index] != keenNoOre)
                        {
                            neighbors.Clear();
                            for (int i = -1; i < 2; i++)
                            {
                                for (int j = -1; j < 2; j++)
                                {
                                    var v = oreData[index + i + j * w];
                                    if (neighbors.TryGetValue(v, out int amt))
                                    {
                                        neighbors[v] = amt + 1;
                                    }
                                    else neighbors.Add(v, 1);
                                }
                            }
                            if (neighbors.ContainsKey(keenNoOre)) neighbors[keenNoOre] = 0;
                            var result = neighbors.Keys.OrderBy(x => neighbors[x]).Last();
                            newOreData[index] = result;
                        }
                        else newOreData[index] = keenNoOre;
                    }
                }
                oreData = newOreData;
            }

            public void SaveImage(string outputPath)
            {
                using Bitmap newBitmap = new(originalBitmap.Width, originalBitmap.Height);
                for (int y = 1; y <= originalBitmap.Height; y++)
                {
                    for (int x = 1; x <= originalBitmap.Width; x++)
                    {
                        var oldColor = originalBitmap.GetPixel(x - 1, y - 1);
                        var newColor = Color.FromArgb(oldColor.R, oldColor.G, oreData[x + y * w]);
                        newBitmap.SetPixel(x - 1, y - 1, newColor);
                    }
                }
                newBitmap.Save(outputPath);
            }
        }
    }
}