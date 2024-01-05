using System.Drawing;

namespace OreTuner
{
    public class Program
    {
        const int gens = 6;
        const byte keenNoOre = 255;

        static void Main(string[] args)
        {
            var oreStrategy = new BreakTypesAndSmooth();
            foreach (string arg in args)
            {
                using Bitmap originalBitmap = new(arg);
                OreMap map = new(originalBitmap);
                map.ContextlessPass(oreStrategy);
                for (int g = 0; g < gens; g++)
                {
                    map.RunGeneration(oreStrategy);
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
            readonly List<byte> newOreData;
            readonly Bitmap originalBitmap;
            readonly List<byte> localData = new();

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
                    oreData[i] = oreStrategy.ContextlessPass(oreData[i]);
                }
            }

            public void RunGeneration(IOreStrategy oreStrategy)
            {
                for (int y = 1; y <= originalBitmap.Height; y++)
                {
                    for (int x = 1; x <= originalBitmap.Width; x++)
                    {
                        int index = x + y * w;
                        localData.Clear();
                        localData.Add(oreData[index - 1-w]);
                        localData.Add(oreData[index-w]);
                        localData.Add(oreData[index + 1 - w]);
                        localData.Add(oreData[index - 1]);
                        localData.Add(oreData[index]);
                        localData.Add(oreData[index + 1]);
                        localData.Add(oreData[index-1+w]);
                        localData.Add(oreData[index+w]);
                        localData.Add(oreData[index+1+w]);
                        newOreData[index] = oreStrategy.ContextualPass(localData);
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