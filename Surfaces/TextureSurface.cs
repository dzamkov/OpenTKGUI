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
    /// A surface that displays a texture.
    /// </summary>
    public class TextureSurface : Surface
    {
        public TextureSurface(int ID)
        {
            this._ID = ID;
        }

        /// <summary>
        /// Creates a texture representation of the given surface (using render to texture).
        /// </summary>
        public static TextureSurface Create(Surface Surface, int Width, int Height)
        {
            int texid = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texid);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Modulate);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);

            int renid; GL.GenRenderbuffers(1, out renid);
            GL.BindRenderbuffer(RenderbufferTarget.RenderbufferExt, renid);
            GL.RenderbufferStorage(RenderbufferTarget.RenderbufferExt, RenderbufferStorage.DepthComponent16, Width, Height);
            GL.BindRenderbuffer(RenderbufferTarget.RenderbufferExt, 0);

            int frameid; GL.GenFramebuffers(1, out frameid);
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, frameid);
            GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texid, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachment, RenderbufferTarget.RenderbufferExt, renid);

            Point viewsize = new Point(Width, Height);
            GUIRenderContext rc = new GUIRenderContext(viewsize);
            rc.Setup();
            rc.DrawSurface(Surface, new Rectangle(viewsize));

            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
            GL.DeleteFramebuffers(1, ref frameid);
            GL.DeleteRenderbuffers(1, ref renid);

            return new TextureSurface(texid);
        }

        /// <summary>
        /// Creates a texture representation of the given fixed surface.
        /// </summary>
        public static TextureSurface Create(FixedSurface Surface)
        {
            Point size = Surface.Size;
            return Create(Surface, (int)size.X, (int)size.Y);
        }

        /// <summary>
        /// Gets the id for this surface. Note that the texture is delete when this surface is disposed.
        /// </summary>
        public int ID
        {
            get
            {
                return this._ID;
            }
        }

        public override void Render(Rectangle Area, GUIRenderContext Context)
        {
            Context.DrawTexture(this._ID, Area);
        }

        public override void OnDispose()
        {
            GL.DeleteTexture(this._ID);
        }

        private int _ID;
    }
}