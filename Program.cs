using System.Drawing;

namespace OreTuner
{
    public class Program
    {
        const int gens = 6;
        const byte keenNoOre = 255;
        const byte iron = 1;
        const byte nickel = 24;
        const byte silicon = 48;
        const byte cobalt = 72;
        const byte magnesium = 120;
        const byte silver = 96;
        const byte gold = 168;
        const byte uranium = 144;
        const byte platinum = 192;

        static void Main(string[] args)
        {
            var oreStrategy = new BreakTypesAndSmooth();
            var depthStrategy = new CreateVariation();
            Dictionary<byte, Color> scheme = new()
            {
                { iron, Color.FromArgb(unchecked((int)0xFFAA2222)) },
                { iron+1, Color.FromArgb(unchecked((int)0xFFBB2222)) },
                { iron+2, Color.FromArgb(unchecked((int)0xFFCC2222)) },
                { nickel, Color.FromArgb(unchecked((int)0xFF00410c)) },
                { nickel+1, Color.FromArgb(unchecked((int)0xFF00520c)) },
                { nickel+2, Color.FromArgb(unchecked((int)0xFF00630c)) },
                { silicon, Color.FromArgb(unchecked((int)0xFF43004f)) },
                { silicon+1, Color.FromArgb(unchecked((int)0xFF540060)) },
                { silicon+2, Color.FromArgb(unchecked((int)0xFF650071)) },
                { cobalt, Color.FromArgb(unchecked((int)0xFF3030BB)) },
                { cobalt+1, Color.FromArgb(unchecked((int)0xFF3030CC)) },
                { cobalt+2, Color.FromArgb(unchecked((int)0xFF3030DD)) },
                { magnesium, Color.FromArgb(unchecked((int)0xFF00e099)) },
                { magnesium+1, Color.FromArgb(unchecked((int)0xFF00e0AA)) },
                { magnesium+2, Color.FromArgb(unchecked((int)0xFF00e0BB)) },
                { silver, Color.FromArgb(unchecked((int)0xFFe0dfb4)) },
                { silver+1, Color.FromArgb(unchecked((int)0xFFe0dfc5)) },
                { silver+2, Color.FromArgb(unchecked((int)0xFFe0dfd6)) },
                { gold, Color.FromArgb(unchecked((int)0xFFf0ff00)) },
                { gold+1, Color.FromArgb(unchecked((int)0xFFf1ff00)) },
                { gold+2, Color.FromArgb(unchecked((int)0xFFf2ff00)) },
                { uranium, Color.FromArgb(unchecked((int)0xFF8aff00)) },
                { uranium+1, Color.FromArgb(unchecked((int)0xFF9aff00)) },
                { uranium+2, Color.FromArgb(unchecked((int)0xFFaaff00)) },
                { platinum, Color.FromArgb(unchecked((int)0xFFCCCCFF)) },
                { platinum+1, Color.FromArgb(unchecked((int)0xFFDDDDFF)) },
                { platinum+2, Color.FromArgb(unchecked((int)0xFFEEEEFF)) }
            };
            foreach (string arg in args)
            {
                var trimTail = arg.Replace(".png", "");
                using Bitmap originalBitmap = new(trimTail+"_mat.png");
                using Bitmap heightMap = new(arg);
                OreMap map = new(originalBitmap);
                map.ContextlessPass(oreStrategy);
                for (int g = 0; g < gens; g++)
                {
                    map.RunGeneration(oreStrategy);
                }

                map.ContextlessPass(depthStrategy);


                var outputPath = $"{trimTail}_mat.Output{gens}.png";
                map.SaveImage(outputPath);
                Console.WriteLine($"{outputPath} written.");

                HeightMap hmap = new(heightMap);
                hmap.Blend(map, scheme);
                hmap.Save($"{trimTail}.Blended.png");
            }
            Console.WriteLine("Done.");
        }

        public class HeightMap
        {
            readonly Bitmap bitmap;
            public HeightMap(Bitmap input)
            {
                this.bitmap = input;
            }

            public void Blend(OreMap map, Dictionary<byte, Color> scheme)
            {
                for (int y = 0; y < bitmap.Height; ++y)
                    for (int x = 0; x < bitmap.Width; ++x)
                    {
                        var v = map.GetValue(x, y);
                        if (v != keenNoOre) bitmap.SetPixel(x, y, scheme[v]);
                    }
            }

            public void Save(string path)
            { 
                bitmap.Save(path);
            }
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

            public byte GetValue(int x, int y)
            {
                return oreData[x + 1 + (y + 1) * w];
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