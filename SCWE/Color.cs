namespace SCWE
{
    public struct Color
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;

        public static Color white => new Color(255, 255, 255, 255);

        public Color(byte r, byte g, byte b) : this(r, g, b, 255)
        {
        }

        public Color(byte r, byte g, byte b, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public override bool Equals(object obj)
        {
            if (obj is Color)
            {
                return Equals((Color)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return r + g << 8 + b << 16 + a << 24;
        }

        public bool Equals(Color c)
        {
            return c.r == r && c.g == g && c.b == b && c.a == a;
        }

        public static Color Lerp(Color a, Color b, float f)
        {
            return new Color(
                (byte)(a.r + (b.r - a.r) * f),
                (byte)(a.g + (b.g - a.g) * f),
                (byte)(a.b + (b.b - a.b) * f),
                (byte)(a.a + (b.a - a.a) * f));
        }

        public static bool operator ==(Color a, Color b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Color a, Color b)
        {
            return !a.Equals(b);
        }
    }
}
