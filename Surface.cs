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
        /// Gets the size of the surface.
        /// </summary>
        public abstract Point Size { get; }

        /// <summary>
        /// Renders the surface with the given context and offset.
        /// </summary>
        public abstract void Render(Point Offset, GUIRenderContext Context);

        /// <summary>
        /// Renders the surface with the given context with no additional translation.
        /// </summary>
        public void Render(GUIRenderContext Context)
        {
            this.Render(new Point(0.0, 0.0), Context);
        }
    }
}