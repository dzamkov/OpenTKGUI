using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;


namespace OpenTKGUI
{
    /// <summary>
    /// A static two-dimensional array of data.
    /// </summary>
    /// <typeparam name="T">The data type encoded into a pixel of the image.</typeparam>
    public abstract class Image<T>
    {
        /// <summary>
        /// Gets the size of the image.
        /// </summary>
        public abstract ImageSize Size { get; }

        /// <summary>
        /// Gets a pixel at the specified location.
        /// </summary>
        public abstract T Get(int X, int Y);

        /// <summary>
        /// Creates a static copy of this image.
        /// </summary>
        public virtual StaticImage<T> MakeStatic()
        {
            return new StaticImage<T>(this);
        }
    }

    /// <summary>
    /// An image, with 32 bit ARGB pixels.
    /// </summary>
    public abstract class ColorImage : Image<Color>
    {
        /// <summary>
        /// Writes the specified region to the given buffer in 32 bit ARGB format.
        /// </summary>
        public virtual unsafe void Write(byte* Buffer, int X, int Y, ImageSize Size)
        {
            for (int y = 0; y < Size.Height; y++)
            {
                for (int x = 0; x < Size.Width; x++)
                {
                    Color c = this.Get(x + X, y + Y);
                    Buffer[0] = (byte)(c.B * 255.0);
                    Buffer[1] = (byte)(c.G * 255.0);
                    Buffer[2] = (byte)(c.R * 255.0);
                    Buffer[3] = (byte)(c.A * 255.0);
                    Buffer += 4;
                }
            }
        }

        /// <summary>
        /// Creates a texture that displays this image.
        /// </summary>
        public unsafe int MakeTexture()
        {
            ImageSize size = this.Size;
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexEnv(TextureEnvTarget.TextureEnv,
                TextureEnvParameter.TextureEnvMode,
                (float)TextureEnvMode.Modulate);


            // Write texture data
            byte[] adata = new byte[size.Area * 4];
            fixed (byte* data = adata)
            {
                this.Write(data, 0, 0, size);
            }

            
            GL.TexImage2D(TextureTarget.Texture2D,
                0, PixelInternalFormat.Rgba,
                size.Width, size.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, adata);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            return id;
        }
    }

    /// <summary>
    /// A colored image derived from a scalar image.
    /// </summary>
    public sealed class MonochromeImage<TSource> : ColorImage
        where TSource : Image<double>
    {
        public MonochromeImage(TSource Source)
        {
            this._Source = Source;
        }

        public override ImageSize Size
        {
            get
            {
                return this._Source.Size;
            }
        }

        public override Color Get(int X, int Y)
        {
            double a = this._Source.Get(X, Y);
            return Color.RGB(a, a, a);
        }

        private TSource _Source;
    }

    /// <summary>
    /// An image where all data is statically stored in an array.
    /// </summary>
    public sealed class StaticImage<T> : Image<T>
    {
        public StaticImage(Image<T> Source)
        {
            this.SourceSize = Source.Size;
            this.Data = new T[this.SourceSize.Area];
            int i = 0;
            for (int y = 0; y < this.SourceSize.Height; y++)
            {
                for (int x = 0; x < this.SourceSize.Width; x++)
                {
                    this.Data[i] = Source.Get(x, y);
                    i++;
                }
            }
        }

        public override T Get(int X, int Y)
        {
            return this.Data[X + (Y * this.SourceSize.Width)];
        }

        public override ImageSize Size
        {
            get
            {
                return this.SourceSize;
            }
        }

        /// <summary>
        /// The size of the image.
        /// </summary>
        public ImageSize SourceSize;

        /// <summary>
        /// The data for the image.
        /// </summary>
        public T[] Data;
    }

    /// <summary>
    /// An image representing lighting values produced from a heightmap.
    /// </summary>
    public sealed class LightImage : Image<double>
    {
        public LightImage(Point LightOffset, Image<double> Source)
        {
            this._LightOffset = LightOffset;
            this._LightMultipler = 1.0 / Math.Sqrt(LightOffset.SquareLength + 1.0);
            this._Source = Source;
        }

        public override ImageSize Size
        {
            get
            {
                return this._Source.Size;
            }
        }

        public override double Get(int X, int Y)
        {
            // Get sample points needed for a normal.
            ImageSize size = this._Source.Size;
            double ym = Y > 0 ? this._Source.Get(X, Y - 1) : 0.0;
            double xm = X > 0 ? this._Source.Get(X - 1, Y) : 0.0;
            double yp = Y < size.Height - 1 ? this._Source.Get(X, Y + 1) : 0.0;
            double xp = X < size.Width - 1 ? this._Source.Get(X + 1, Y) : 0.0;
            double ss = this._Source.Get(X, Y);
            return _CalculateLight((xp - xm) / 2.0, (yp - ym) / 2.0, this._LightOffset, this._LightMultipler);
        }

        private static double _CalculateLight(double XDelta, double YDelta, Point LightOffset, double LightMultipler)
        {
            double xdis = 1.0 / Math.Sqrt(1.0 + XDelta * XDelta);
            double ydis = 1.0 / Math.Sqrt(1.0 + YDelta * YDelta);
            double xm = -XDelta * ydis;
            double ym = -YDelta * xdis;
            double zm = xdis * ydis;
            return (xm * LightOffset.X + ym * LightOffset.Y + zm * 1.0) * LightMultipler;
        }

        private Point _LightOffset;
        private double _LightMultipler;
        private Image<double> _Source;
    }

    /// <summary>
    /// Image-related functions.
    /// </summary>
    public static class Image
    {
        /// <summary>
        /// Converts a scalar image to a monochromatic color image.
        /// </summary>
        public static MonochromeImage<TSource> ToMonochrome<TSource>(TSource Source)
            where TSource : Image<double>
        {
            return new MonochromeImage<TSource>(Source);
        }
    }

    /// <summary>
    /// A possible size for an image.
    /// </summary>
    public struct ImageSize
    {
        public ImageSize(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
        }

        public ImageSize(Point Size)
        {
            this.Width = (int)Size.X;
            this.Height = (int)Size.Y;
        }

        public static implicit operator ImageSize(Point Size)
        {
            return new ImageSize(Size);
        }

        /// <summary>
        /// Gets the total area of the described image.
        /// </summary>
        public int Area
        {
            get
            {
                return this.Width * this.Height;
            }
        }

        /// <summary>
        /// The width of the image in pixels.
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the iamge in pixels.
        /// </summary>
        public int Height;
    }
}