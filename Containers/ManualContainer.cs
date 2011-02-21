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
            this.ResizeChild(Child, Area.Size);
            this._Children.Add(new _Child()
            {
                Control = Child,
                Offset = Area.Location
            });
        }


        public override void Render(GUIRenderContext Context)
        {
            Rectangle inner = new Rectangle(new Point(), this.Size);
            using (Context.Clip(inner))
            {
                foreach (_Child c in this._Children)
                {
                    using (Context.Translate(c.Offset))
                    {
                        c.Control.Render(Context);
                    }
                }
            }
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            foreach (_Child c in this._Children)
            {
                c.Control.Update(Context.CreateChildContext(c.Control, c.Offset), Time);
            }
        }

        protected override void OnDispose()
        {
            foreach (_Child c in this._Children)
            {
                c.Control.Dispose();
            }
        }

        private struct _Child
        {
            public Control Control;
            public Point Offset;
        }

        private List<_Child> _Children;
    }
}