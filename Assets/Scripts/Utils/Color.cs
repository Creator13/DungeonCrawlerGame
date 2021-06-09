using System;

namespace Utils
{
    public struct Color
    {
        public byte r, g, b, a;

        public Color(int r, int g, int b, int a)
        {
            this.r = (byte) r;
            this.g = (byte) g;
            this.b = (byte) b;
            this.a = (byte) a;
        }

        public Color(float r, float g, float b, float a)
        {
            this.r = Convert.ToByte(r * 255);
            this.g = Convert.ToByte(g * 255);
            this.b = Convert.ToByte(b * 255);
            this.a = Convert.ToByte(a * 255);
        }

        public Color(UnityEngine.Color c) : this(c.r, c.g, c.b, c.a) { }

        public static implicit operator Color(UnityEngine.Color c)
        {
            return new Color(c);
        }

        public static implicit operator UnityEngine.Color(Color c)
        {
            return new UnityEngine.Color {
                r = c.r / 255f,
                g = c.g / 255f,
                b = c.b / 255f,
                a = c.a / 255f
            };
        }

        public static explicit operator uint(Color c)
        {
            return c.ToUint();
        }

        public static explicit operator Color(uint c)
        {
            return new Color {
                r = (byte) ((c & 0xFF000000) >> 24),
                g = (byte) ((c & 0x00FF0000) >> 16),
                b = (byte) ((c & 0x0000FF00) >> 8),
                a = (byte) (c & 0x000000FF)
            };
        }

        private uint ToUint()
        {
            // var r = (uint) this.r & 0xFF;
            // var g = (uint) this.g & 0xFF;
            // var b = (uint) this.b & 0xFF;
            // var a = (uint) this.a & 0xFF;

            return ((uint) r << 24) | ((uint) g << 16) | ((uint) b << 8) | (uint) a;
        }

        public override string ToString()
        {
            return $"RGBA({r}, {g}, {b}, {a})";
        }
    }
}
