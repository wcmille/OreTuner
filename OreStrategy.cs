namespace OreTuner
{
    public interface IOreStrategy
    {
        byte ContextlessPass(byte v);
        byte ContextualPass(List<byte> localData);
    }

    public class CreateVariation : IOreStrategy
    {
        const int colorDepth = 3;
        readonly Random rnd = new();

        public byte ContextlessPass(byte v)
        {
            return v += (byte)rnd.Next(colorDepth);
        }

        public byte ContextualPass(List<byte> localData)
        {
            return localData[4];
        }
    }

    public class BreakTypesAndSmooth : IOreStrategy
    {
        readonly Dictionary<byte, int> neighbors = new();
        readonly Random rnd = new();
        const byte keenNoOre = 255;
        readonly Dictionary<byte, List<byte>> oreGroups = new();
        readonly byte iron = 1;
        readonly byte nickel = 24;
        readonly byte silicon = 48;
        readonly byte cobalt = 72;
        readonly byte magnesium = 120;
        readonly byte silver = 96;
        readonly byte gold = 168;
        readonly byte uranium = 144;
        readonly byte platinum = 192;

        public BreakTypesAndSmooth()
        {
            oreGroups.Add(iron, new List<byte>() { iron, nickel, silicon});
            oreGroups.Add(cobalt, new List<byte>() { cobalt, cobalt, iron, nickel });
            oreGroups.Add(magnesium, new List<byte>() { magnesium, magnesium, iron, silicon});
            oreGroups.Add(gold, new List<byte>() { gold, gold, silver, silver, nickel });
            oreGroups.Add(uranium, new List<byte>() { uranium, uranium, magnesium, silicon});
            oreGroups.Add(platinum, new List<byte>() { platinum, platinum, nickel, iron });
        }

        public byte ContextlessPass(byte v)
        {
            if (v != keenNoOre && oreGroups.ContainsKey(v))
            {
                return oreGroups[v][rnd.Next(oreGroups[v].Count)];
            }
            return keenNoOre;
        }

        public byte ContextualPass(List<byte> localData)
        {
            if (localData[4] != keenNoOre)
            {
                neighbors.Clear();
                for (int i = 0; i < 9; i++)
                {
                    var v = localData[i];
                    if (neighbors.TryGetValue(v, out int amt))
                    {
                        neighbors[v] = amt + 1;
                    }
                    else neighbors.Add(v, 1);
                }
                if (neighbors.ContainsKey(keenNoOre)) neighbors[keenNoOre] = 0;
                byte result = neighbors.Keys.OrderBy(x => neighbors[x]).Last();
                return result;
            }
            return keenNoOre;
        }
    }
}
