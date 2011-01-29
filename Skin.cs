using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTKGUI
{
    /// <summary>
    /// An image containing many related, flexible, subimages that can be used to draw controls.
    /// </summary>
    public class Skin
    {
        public Skin(Bitmap Source)
        {
            this._Width = Source.Width;
            this._Height = Source.Height;

            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexEnv(TextureEnvTarget.TextureEnv,
                TextureEnvParameter.TextureEnvMode,
                (float)TextureEnvMode.Modulate);

            BitmapData bd = Source.LockBits(
                new System.Drawing.Rectangle(0, 0, Source.Width, Source.Height), 
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D,
                0, PixelInternalFormat.Rgba,
                Source.Width, Source.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bd.Scan0);

            Source.UnlockBits(bd);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            this._Texture = id;
        }

        /// <summary>
        /// Gets the default skin.
        /// </summary>
        public static Skin Default
        {
            get
            {
                if (_Default == null)
                {
                    _Default = new Skin(Res.SkinDefault);
                }
                return _Default;
            }
        }

        private static Skin _Default;

        /// <summary>
        /// Gets a skin surface for the given region in the skin. No stretching or resizing is applied to the surface.
        /// </summary>
        public SkinSurface GetSurface(SkinArea Rect)
        {
            List<SkinSurface.Stop> xstops = new List<SkinSurface.Stop>();
            List<SkinSurface.Stop> ystops = new List<SkinSurface.Stop>();
            xstops.Add(new SkinSurface.Stop(Rect.X, 0.0));
            xstops.Add(new SkinSurface.Stop(Rect.X + Rect.Width, Rect.Width));
            ystops.Add(new SkinSurface.Stop(Rect.Y, 0.0));
            ystops.Add(new SkinSurface.Stop(Rect.Y + Rect.Height, Rect.Height));
            return this.GetSurface(xstops, ystops);
        }

        /// <summary>
        /// Gets a skin surface for the given region in the skin. The surface will be stretched at the midline to get the target size.
        /// </summary>
        public SkinSurface GetSurface(SkinArea Rect, Point TargetSize)
        {
            return this.GetSurface(Rect, Rect.Width / 2, Rect.Height / 2, TargetSize);
        }

        /// <summary>
        /// Gets a skin surface for the given region in the skin. The surface will be stretched at the stretch lines to get the target size.
        /// </summary>
        public SkinSurface GetSurface(SkinArea Rect, int XStretchLine, int YStretchLine, Point TargetSize)
        {
            List<SkinSurface.Stop> xstops = new List<SkinSurface.Stop>();
            List<SkinSurface.Stop> ystops = new List<SkinSurface.Stop>();
            xstops.Add(new SkinSurface.Stop(Rect.X, 0.0));
            if (TargetSize.X > Rect.Width)
            {
                xstops.Add(new SkinSurface.Stop(Rect.X + XStretchLine - 1, XStretchLine - 1));
                xstops.Add(new SkinSurface.Stop(Rect.X + XStretchLine + 1, TargetSize.X + XStretchLine - Rect.Width + 1));
            }
            xstops.Add(new SkinSurface.Stop(Rect.X + Rect.Width, TargetSize.X));

            ystops.Add(new SkinSurface.Stop(Rect.Y, 0.0));
            if (TargetSize.Y > Rect.Height)
            {
                ystops.Add(new SkinSurface.Stop(Rect.Y + YStretchLine - 1, YStretchLine - 1));
                ystops.Add(new SkinSurface.Stop(Rect.Y + YStretchLine + 1, TargetSize.Y + YStretchLine - Rect.Height + 1));
            }
            ystops.Add(new SkinSurface.Stop(Rect.Y + Rect.Height, TargetSize.Y));
            return this.GetSurface(xstops, ystops);
        }

        /// <summary>
        /// Gets a skin surface for the given region in the skin. Stretching and resizing is to be manually specified with stops. Lists
        /// must be discarded after being supplied.
        /// </summary>
        public SkinSurface GetSurface(List<SkinSurface.Stop> XStops, List<SkinSurface.Stop> YStops)
        {
            return new SkinSurface(this, XStops, YStops);
        }

        /// <summary>
        /// Gets the id for the texture of the skin.
        /// </summary>
        public int Texture
        {
            get
            {
                return this._Texture;
            }
        }

        /// <summary>
        /// Gets the width of the texture for the skin.
        /// </summary>
        public int Width
        {
            get
            {
                return this._Width;
            }
        }

        /// <summary>
        /// Gets the height of the texture for the skin.
        /// </summary>
        public int Height
        {
            get
            {
                return this._Height;
            }
        }

        private int _Width;
        private int _Height;
        private int _Texture;
    }

    /// <summary>
    /// A source area for a skin.
    /// </summary>
    public struct SkinArea
    {
        public SkinArea(int X, int Y, int Width, int Height)
        {
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;
        }

        public int X;

        public int Y;

        public int Width;

        public int Height;
    }

    /// <summary>
    /// A surface that renders part of a skin, with optional modifications.
    /// </summary>
    public class SkinSurface : Surface
    {
        internal SkinSurface(Skin Skin, List<Stop> XStops, List<Stop> YStops)
        {
            _MultiplyTextureOffsets(XStops, 1.0 / (double)Skin.Width);
            _MultiplyTextureOffsets(YStops, 1.0 / (double)Skin.Height);
            this._Skin = Skin;
            this._XStops = XStops;
            this._YStops = YStops;
        }

        public override void Render(Point Offset, GUIRenderContext Context)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, this._Skin.Texture);
            GL.Color4(1.0, 1.0, 1.0, 1.0);

            int xs = this._XStops.Count - 1;
            int ys = this._YStops.Count - 1;

            GL.Begin(BeginMode.Quads);
            for (int tx = 0; tx < xs; tx++)
            {
                for (int ty = 0; ty < ys; ty++)
                {
                    double x = this._XStops[tx].RenderOffset + Offset.X;
                    double y = this._YStops[ty].RenderOffset + Offset.Y;
                    double xx = this._XStops[tx + 1].RenderOffset + Offset.X;
                    double yy = this._YStops[ty + 1].RenderOffset + Offset.Y;
                    double u = this._XStops[tx].TextureOffset;
                    double v = this._YStops[ty].TextureOffset;
                    double uu = this._XStops[tx + 1].TextureOffset;
                    double vv = this._YStops[ty + 1].TextureOffset;
                    GL.TexCoord2(u, v); GL.Vertex2(x, y);
                    GL.TexCoord2(uu, v); GL.Vertex2(xx, y);
                    GL.TexCoord2(uu, vv); GL.Vertex2(xx, yy);
                    GL.TexCoord2(u, vv); GL.Vertex2(x, yy);
                }
            }
            GL.End();
        }

        /// <summary>
        /// Applies the texture offsets in a list of stops by applying an offset and a multipler.
        /// </summary>
        private static void _MultiplyTextureOffsets(List<Stop> Stops, double Multipler)
        {
            for (int t = 0; t < Stops.Count; t++)
            {
                Stop st = Stops[t];
                Stops[t] = new Stop(st.TextureOffset * Multipler, st.RenderOffset);
            }
        }

        /// <summary>
        /// Represents a line on an axis that correlates an offset when rendered to an offset from the source texture. Using multiple stops can create
        /// stretching effects.
        /// </summary>
        public struct Stop
        {
            public Stop(double TextureOffset, double RenderOffset)
            {
                this.TextureOffset = TextureOffset;
                this.RenderOffset = RenderOffset;
            }

            /// <summary>
            /// The offset of the stop from the source texture region.
            /// </summary>
            public double TextureOffset;

            /// <summary>
            /// The offset of the stop from the begining of the rendered surface.
            /// </summary>
            public double RenderOffset;
        }

        public override Point Size
        {
            get
            {
                return new Point(
                    this._XStops[this._XStops.Count - 1].RenderOffset,
                    this._YStops[this._XStops.Count - 1].RenderOffset);
            }
        }

        private Skin _Skin;
        private List<Stop> _XStops;
        private List<Stop> _YStops;
    }
}