using System.Drawing;

namespace OreTuner
{
    public class Program
    {
        const int colorDepth = 3;
        const byte keenNoOre = 255;
        const int gens = 20;

        static void Main(string[] args)
        {
            Random rnd = new();
            foreach (string arg in args)
            {
                string inputPath = arg;

                using Bitmap originalBitmap = new(inputPath);
                //Init the blue channel.
                int count = (originalBitmap.Height + 2) * (originalBitmap.Width + 2);
                List<byte> blueChannel = new(count);
                blueChannel.AddRange(Enumerable.Repeat(keenNoOre, count));

                for (int y = 0; y < originalBitmap.Height; y++)
                {
                    for (int x = 0; x < originalBitmap.Width; x++)
                    {
                        blueChannel[x + 1+(y+1)*(originalBitmap.Width+2)] = originalBitmap.GetPixel(x, y).B;
                    }
                }
                var w = originalBitmap.Width + 2;

                //Randomize the Blue Channel
                for (int i = 0; i < count; ++i)
                { 
                    if (blueChannel[i] != keenNoOre) blueChannel[i] += (byte)(rnd.Next(colorDepth)*16);
                }

                //Run the generations
                Dictionary<byte, int> neighbors = new();
                List<byte> newBlue = new(count);
                newBlue.AddRange(blueChannel);

                for (int g = 0; g < gens; ++g)
                {
                    for (int y = 1; y <= originalBitmap.Height; y++)
                    {
                        for (int x = 1; x <= originalBitmap.Width; x++)
                        {
                            int index = x + y * w;
                            if (blueChannel[index] == keenNoOre) continue;
                            neighbors.Clear();
                            for (int i = -1; i < 2; i ++)
                            {
                                for (int j = -1; j < 2; j ++)
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

                    string outputPath = arg + $".Output{g}.png";
                    using var newBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);
                    //Write out.
                    for (int y = 1; y <= originalBitmap.Height; y++)
                    {
                        for (int x = 1; x <= originalBitmap.Width; x++)
                        {
                            var oldColor = originalBitmap.GetPixel(x-1, y-1);
                            var newColor = Color.FromArgb(oldColor.R, oldColor.G, blueChannel[x+y*w]);
                            newBitmap.SetPixel(x-1, y-1, newColor);
                        }
                    }
                    newBitmap.Save(outputPath);
                    Console.WriteLine($"{outputPath} written.");
                }
            }
            Console.WriteLine("Done.");
        }
    }
}