using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;

namespace OpenTKGUI
{
    /// <summary>
    /// Represents a color including alpha. Contains methods for color manipulation.
    /// </summary>
    public struct Color
    {

        /// <summary>
        /// Creates a color from its RGBA representation. Values should be between
        /// 0.0 and 1.0.
        /// </summary>
        public static Color RGBA(double R, double G, double B, double A)
        {
            Color c = new Color();
            c.R = R;
            c.G = G;
            c.B = B;
            c.A = A;
            return c;
        }

        /// <summary>
        /// Creates a color from its RGB reprsentation with a completely
        /// opaque alpha.
        /// </summary>
        public static Color RGB(double R, double G, double B)
        {
            return RGBA(R, G, B, 1.0);
        }

        public static Color Alpha(double A)
        {
            return RGBA(1.0, 1.0, 1.0, A);
        }

        /// <summary>
        /// Mixes two colors based on the specified amount. If the amount is 0.0,
        /// the resulting color will be A. If the amount is 1.0, the resulting color
        /// will be B. Values in between will cause the color to be interpolated.
        /// </summary>
        public static Color Mix(Color A, Color B, double Amount)
        {
            double rd = B.R - A.R;
            double gd = B.G - A.G;
            double bd = B.B - A.B;
            double ad = B.A - A.A;
            return RGBA(
                A.R + (rd * Amount),
                A.G + (gd * Amount),
                A.B + (bd * Amount),
                A.A + (ad * Amount));
        }

        /// <summary>
        /// Creates a color from its HLSA representation.
        /// </summary>
        /// <param name="H">Hue in degrees.</param>
        /// <param name="L">Lumination between 0.0 and 1.0.</param>
        /// <param name="S">Saturation between 0.0 and 1.0.</param>
        /// <param name="A">Alpha between 0.0 and 1.0.</param>
        public static Color HLSA(double H, double L, double S, double A)
        {
            // Find color based on hue.
            H = H % 360.0;
            double delta = (H % 60.0) / 60.0;
            Color hue = RGB(1.0, 0.0, 0.0);
            if (H < 60) hue = RGB(1.0, delta, 0.0);
            else if (H < 120) hue = RGB(1.0 - delta, 1.0, 0.0);
            else if (H < 180) hue = RGB(0.0, 1.0, delta);
            else if (H < 240) hue = RGB(0.0, 1.0 - delta, 1.0);
            else if (H < 300) hue = RGB(delta, 0.0, 1.0);
            else if (H < 360) hue = RGB(1.0, 0.0, 1.0 - delta);

            // Saturation
            Color sat = Mix(hue, RGB(0.5, 0.5, 0.5), 1.0 - S);

            // Lumination
            Color lum = sat;
            if (L > 0.5)
            {
                lum = Mix(lum, RGB(1.0, 1.0, 1.0), (L - 0.5) * 2.0);
            }
            else
            {
                lum = Mix(lum, RGB(0.0, 0.0, 0.0), (0.5 - L) * 2.0);
            }

            // Alpha
            lum.A = A;
            return lum;
        }

        /// <summary>
        /// Converts this color into a color usable by opentk.
        /// </summary>
        public static implicit operator Color4(Color Color)
        {
            return new Color4((float)Color.R, (float)Color.G, (float)Color.B, (float)Color.A);
        }

        public static implicit operator System.Drawing.Color(Color Color)
        {
            return System.Drawing.Color.FromArgb(
                (int)(Color.A * 255.0),
                (int)(Color.R * 255.0),
                (int)(Color.G * 255.0),
                (int)(Color.B * 255.0));
        }

        /// <summary>
        /// Sends the color to the current GL context.
        /// </summary>
        public void Render()
        {
            OpenTK.Graphics.OpenGL.GL.Color4(this.R, this.G, this.B, this.A);
        }

        /// <summary>
        /// Brings all color units to the specified value in the color.
        /// </summary>
        public Color Desaturate(double Min, double Max)
        {
            double dis = Max - Min;
            return Color.RGBA(dis * this.R + Min, dis * this.G + Min, dis * this.B + Min, this.A);
        }

        /// <summary>
        /// Gets a completely transparent color.
        /// </summary>
        public static readonly Color Transparent = RGBA(0.0, 0.0, 0.0, 0.0);

        public double R;
        public double G;
        public double B;
        public double A;
    }

    /// <summary>
    /// A mapping of values between 0.0 and 1.0 to colors.
    /// </summary>
    public class Gradient
    {
        public Gradient(Color First, Color Last)
        {
            this._First = First;
            this._Last = Last;
            this._Stops = new LinkedList<Stop>();
        }

        public Gradient(Color First, Color Last, IEnumerable<Stop> OrderedStops)
        {
            this._First = First;
            this._Last = Last;
            this._Stops = new LinkedList<Stop>(OrderedStops);
        }

        /// <summary>
        /// Gets a color for the specified value.
        /// </summary>
        public Color GetColor(double Value)
        {
            Stop prev = new Stop(this._First, 0.0);
            foreach (Stop s in this._Stops)
            {
                if (Value < s.Value)
                {
                    return Color.Mix(prev.Color, s.Color, (Value - prev.Value) / (s.Value - prev.Value));
                }
                prev = s;
            }
            return Color.Mix(prev.Color, this._Last, (Value - prev.Value) / (1.0 - prev.Value));
        }

        /// <summary>
        /// A sample point of the gradient.
        /// </summary>
        public struct Stop
        {
            public Stop(Color Color, double Value)
            {
                this.Color = Color;
                this.Value = Value;
            }

            /// <summary>
            /// The color at the stop.
            /// </summary>
            public Color Color;

            /// <summary>
            /// The value the stop is at.
            /// </summary>
            public double Value;
        }

        private LinkedList<Stop> _Stops;
        private Color _First;
        private Color _Last;
    }
}