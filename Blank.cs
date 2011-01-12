using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTKGUI
{
    /// <summary>
    /// A solid-color control that may never receive input or focus.
    /// </summary>
    public class Blank : Control
    {
        public Blank(Color Color)
        {
            this._Color = Color;
        }

        public Blank()
        {
            this._Color = Color.RGB(1.0, 1.0, 1.0);
        }

        /// <summary>
        /// Gets or sets the color of the control.
        /// </summary>
        public Color Color
        {
            get
            {
                return this._Color;
            }
            set
            {
                this._Color = value;
            }
        }

        public override void Render(GUIRenderContext Context)
        {
            Render(this.Color, this.Size);   
        }

        /// <summary>
        /// Renders a solid-colored quad with the given size in pixels.
        /// </summary>
        public static void Render(Color Color, Point Size)
        {
            GL.Color4(Color);
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(0.0, 0.0);
            GL.Vertex2(Size.X, 0.0);
            GL.Vertex2(Size.X, Size.Y);
            GL.Vertex2(0.0, Size.Y);
            GL.End();
        }

        private Color _Color;
    }
}