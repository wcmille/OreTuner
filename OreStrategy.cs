namespace OreTuner
{
    public interface IOreStrategy
    {
        byte ContextlessPass(byte v);
    }

    public class OreStrategy : IOreStrategy
    {
        readonly Random rnd = new();
        const int colorDepth = 3;

        public byte ContextlessPass(byte v)
        {
            return (byte)(v + rnd.Next(colorDepth) * 16);
        }
    }
}
