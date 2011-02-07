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

        /// <summary>
        /// Creates a control that displays this surface.
        /// </summary>
        public SurfaceControl CreateControl()
        {
            return new SurfaceControl(this);
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
        /// Gets the size of the surface.
        /// </summary>
        public abstract Point Size { get; }

        /// <summary>
        /// Renders the surface to a context with the given point at the upper-left corner of the rendered surface.
        /// </summary>
        public abstract void Render(Point Point, GUIRenderContext Context);
    }

    /// <summary>
    /// A surface that aligns a fixed surface to make a resizable surface.
    /// </summary>
    public class AlignSurface : Surface
    {
        public AlignSurface(FixedSurface Source)
        {
            this._Source = Source;
        }

        public AlignSurface(FixedSurface Source, Align Horizontal, Align Vertical)
        {
            this._Source = Source;
            this._Horizontal = Horizontal;
            this._Vertical = Vertical;
        }

        /// <summary>
        /// Gets the fixed surface drawn by this align surface.
        /// </summary>
        public FixedSurface Source
        {
            get
            {
                return this._Source;
            }
        }

        /// <summary>
        /// Gets or sets the horizontal alignment of the source surface.
        /// </summary>
        public Align Horizontal
        {
            get
            {
                return this._Horizontal;
            }
            set
            {
                this._Horizontal = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertical alignment of the source surface.
        /// </summary>
        public Align Vertical
        {
            get
            {
                return this._Vertical;
            }
            set
            {
                this._Vertical = value;
            }
        }

        public override void Render(Rectangle Area, GUIRenderContext Context)
        {
            Context.DrawSurface(this._Source, this._Horizontal, this._Vertical, Area);
        }

        private Align _Horizontal;
        private Align _Vertical;
        private FixedSurface _Source;
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