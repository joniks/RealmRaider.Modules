namespace RealmRaiders.Modules.WorldSurfaceValidation
{
    /// <summary>One raw, unpremultiplied RGBA sample.</summary>
    public readonly struct Rgba32
    {
        public Rgba32(byte red, byte green, byte blue, byte alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public byte Red { get; }
        public byte Green { get; }
        public byte Blue { get; }
        public byte Alpha { get; }

        internal double NormalizedAbsoluteDelta(Rgba32 other)
        {
            var total = AbsoluteDifference(Red, other.Red)
                + AbsoluteDifference(Green, other.Green)
                + AbsoluteDifference(Blue, other.Blue)
                + AbsoluteDifference(Alpha, other.Alpha);
            return total / 1020d;
        }

        private static int AbsoluteDifference(byte first, byte second)
        {
            return first >= second ? first - second : second - first;
        }
    }
}
