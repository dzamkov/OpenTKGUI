using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

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
        public void AddChild(GUIContext Context, Control Child, Rectangle Area)
        {
            Child.Resize(Context, Area.Size);
            this._Children.Add(new _Child()
            {
                Control = Child,
                Position = Area.Location
            });
        }

        public override void Render(GUIRenderContext Context)
        {
            Context.PushClip(new Rectangle(new Point(), this.Size));
            foreach (_Child c in this._Children)
            {
                Context.PushTranslate(c.Position);
                c.Control.Render(Context);
                Context.Pop();
            }
            Context.Pop();
        }

        private struct _Child
        {
            public Control Control;
            public Point Position;
        }

        private List<_Child> _Children;
    }
}