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
        /// Gets an image part of this skin. The part will be stretched in the middle to maintain its resolution.
        /// </summary>
        public SkinPart GetPart(int X, int Y, int Width, int Height)
        {
            return new SkinPart(this, X, Y, Width, Height);
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
    /// A part of a skin that may be drawn.
    /// </summary>
    public class SkinPart
    {
        internal SkinPart(Skin Skin, int X, int Y, int Width, int Height)
        {
            this._Skin = Skin;
            this._X = X;
            this._Y = Y;
            this._Width = Width;
            this._Height = Height;
        }

        /// <summary>
        /// Renders the part to the given rectangle.
        /// </summary>
        protected internal void Render(Rectangle Rect)
        {
            GL.BindTexture(TextureTarget.Texture2D, this._Skin.Texture);

            Point size = new Point(this._Width, this._Height);
            Point[] coords = new Point[]
            {
                Rect.Location,
                Rect.Location + size * 0.5 - new Point(1, 1),
                Rect.Location + Rect.Size - size * 0.5 + new Point(1, 1),
                Rect.Location + Rect.Size
            };
            int[] ucoords = new int[]
            {
                this._X,
                this._X + this._Width / 2 - 1,
                this._X + this._Width / 2 + 1,
                this._X + this._Width
            };
            int[] vcoords = new int[]
            {
                this._Y,
                this._Y + this._Height / 2 - 1,
                this._Y + this._Height / 2 + 1,
                this._Y + this._Height
            };

            double multu = 1.0 / (double)this._Skin.Width;
            double multv = 1.0 / (double)this._Skin.Height;
            GL.Begin(BeginMode.Quads);
            for (int tx = 0; tx < 3; tx++)
            {
                for (int ty = 0; ty < 3; ty++)
                {
                    double x = coords[tx].X;
                    double y = coords[ty].Y;
                    double xx = coords[tx + 1].X;
                    double yy = coords[ty + 1].Y;
                    double u = ucoords[tx] * multu;
                    double v = vcoords[ty] * multv;
                    double uu = ucoords[tx + 1] * multu;
                    double vv = vcoords[ty + 1] * multv;
                    GL.TexCoord2(u, v); GL.Vertex2(x, y);
                    GL.TexCoord2(uu, v); GL.Vertex2(xx, y);
                    GL.TexCoord2(uu, vv); GL.Vertex2(xx, yy);
                    GL.TexCoord2(u, vv); GL.Vertex2(x, yy);
                }
            }
            GL.End();
        }

        private Skin _Skin;
        private int _X;
        private int _Y;
        private int _Width;
        private int _Height;
    }
}