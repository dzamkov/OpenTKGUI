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
    public abstract class Surface
    {
        /// <summary>
        /// Renders the surface to an area on the given context.
        /// </summary>
        public abstract void Render(Rectangle Area, GUIRenderContext Context);
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
        /// Gets the size of the surface.
        /// </summary>
        public abstract Point Size { get; }

        /// <summary>
        /// Renders the surface to a context with the given point at the upper-left corner of the rendered surface.
        /// </summary>
        public abstract void Render(Point Point, GUIRenderContext Context);
    }
}