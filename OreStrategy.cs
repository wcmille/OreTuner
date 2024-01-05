namespace OreTuner
{
    public interface IOreStrategy
    {
        byte ContextlessPass(byte v);
        byte Smooth(List<byte> oreData, int index, int w);
    }

    public class OreStrategy : IOreStrategy
    {
        readonly Dictionary<byte, int> neighbors = new();
        readonly Random rnd = new();
        const int colorDepth = 3;
        const byte keenNoOre = 255;

        public byte ContextlessPass(byte v)
        {
            if (v != keenNoOre)
            {
                return (byte)(v + rnd.Next(colorDepth) * 16);
            }
            return keenNoOre;
        }

        public byte Smooth(List<byte> oreData, int index, int w)
        {
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
                byte result = neighbors.Keys.OrderBy(x => neighbors[x]).Last();
                return result;
            }
            return keenNoOre;
        }
    }
}
