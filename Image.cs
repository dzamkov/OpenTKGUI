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
    /// Image-related functions.
    /// </summary>
    public static class Image
    {


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