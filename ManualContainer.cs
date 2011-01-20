using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// A container of controls where the positions and sizes of child controls are static and specified
    /// manually.
    /// </summary>
    public class ManualContainer : Control
    {
        public ManualContainer()
        {
            this._Children = new List<_Child>();
        }

        /// <summary>
        /// Adds a child control at the specified position and size.
        /// </summary>
        public void AddChild(Control Child, Rectangle Area)
        {
            Child.Resize(Area.Size);
            this._Children.Add(new _Child()
            {
                Control = Child,
                Offset = Area.Location
            });
        }

        /// <summary>
        /// Gets or sets the background color for the container, or transparent for no background.
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
            Rectangle inner = new Rectangle(new Point(), this.Size);
            Context.PushClip(inner);
            if (this._Color.A > 0.0)
            {
                Context.DrawSolid(this._Color, inner);
            }
            foreach (_Child c in this._Children)
            {
                Context.PushTranslate(c.Offset);
                c.Control.Render(Context);
                Context.Pop();
            }
            Context.Pop();
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            foreach (_Child c in this._Children)
            {
                c.Control.Update(Context.CreateChildContext(c.Control, c.Offset), Time);
            }
        }

        private struct _Child
        {
            public Control Control;
            public Point Offset;
        }

        private Color _Color;
        private List<_Child> _Children;
    }
}