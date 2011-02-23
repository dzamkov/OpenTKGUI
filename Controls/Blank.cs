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

        public override void Render(RenderContext Context)
        {
            Context.DrawSolid(this._Color, new Rectangle(this.Size));
        }

        private Color _Color;
    }
}