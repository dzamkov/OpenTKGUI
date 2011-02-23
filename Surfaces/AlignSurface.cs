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

        public override void Render(Rectangle Area, RenderContext Context)
        {
            Context.DrawSurface(this._Source, this._Horizontal, this._Vertical, Area);
        }

        private Align _Horizontal;
        private Align _Vertical;
        private FixedSurface _Source;
    }
}