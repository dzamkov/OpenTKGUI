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
    /// A static 2D image that can be rendered with a render context without causing interference to future render calls.
    /// </summary>
    public abstract class Surface : IDisposable
    {
        /// <summary>
        /// Renders the surface to an area on the given context.
        /// </summary>
        public abstract void Render(Rectangle Area, GUIRenderContext Context);

        /// <summary>
        /// Creates a control that displays this surface.
        /// </summary>
        public SurfaceControl CreateControl()
        {
            return new SurfaceControl(this);
        }

        /// <summary>
        /// Called when the surface is disposed. The surface may not be used after this.
        /// </summary>
        public virtual void OnDispose()
        {

        }

        /// <summary>
        /// Creates a texture surface representation of this surface.
        /// </summary>
        public TextureSurface AsTexture(int Width, int Height)
        {
            return TextureSurface.Create(this, Width, Height);
        }

        public void Dispose()
        {
            this.OnDispose();
        }
    }

    /// <summary>
    /// A surface with a fixed a size.
    /// </summary>
    public abstract class FixedSurface : Surface
    {
        public override void Render(Rectangle Area, GUIRenderContext Context)
        {
            Point tsize = Area.Size;
            Point csize = this.Size;
            if (tsize.X > csize.X || tsize.Y > csize.Y)
            {
                Context.PushClip(Area);
                this.Render(Area.Location, Context);
                Context.Pop();
            }
            else
            {
                this.Render(Area.Location, Context);
            }
        }

        /// <summary>
        /// Creates a resizable surface by aliging this fixed surface in the resizable area.
        /// </summary>
        public AlignSurface WithAlign(Align Horizontal, Align Vertical)
        {
            return new AlignSurface(this, Horizontal, Vertical);
        }

        /// <summary>
        /// Creates a texture surface representation of this surface.
        /// </summary>
        public TextureSurface AsTexture()
        {
            return TextureSurface.Create(this);
        }

        /// <summary>
        /// Gets the size of the surface.
        /// </summary>
        public abstract Point Size { get; }

        /// <summary>
        /// Renders the surface to a context with the given point at the upper-left corner of the rendered surface.
        /// </summary>
        public abstract void Render(Point Point, GUIRenderContext Context);
    }

    /// <summary>
    /// A control that draws a resizable surface.
    /// </summary>
    public class SurfaceControl : Control
    {
        public SurfaceControl(Surface Surface)
        {
            this._Surface = Surface;
        }

        /// <summary>
        /// Gets the surface drawn by the surface control.
        /// </summary>
        public Surface Surface
        {
            get
            {
                return this._Surface;
            }
        }

        public override void Render(GUIRenderContext Context)
        {
            Context.DrawSurface(this._Surface, new Rectangle(this.Size));
        }

        private Surface _Surface;
    }
}