using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// A container that applies a margin to a single control.
    /// </summary>
    public class MarginContainer : Control
    {
        public MarginContainer(Control Child, double Margin)
        {
            this._Child = Child; this._Margin = Margin;
        }

        /// <summary>
        /// Gets the size this container needs to be for the child to have the specified size.
        /// </summary>
        public Point GetSize(Point ChildSize)
        {
            return ChildSize + new Point(this._Margin, this._Margin) * 2.0;
        }

        public override void Render(GUIRenderContext Context)
        {
            Context.PushTranslate(new Point(this._Margin, this._Margin));
            this._Child.Render(Context);
            Context.Pop();
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            this._Child.Update(Context.CreateChildContext(this._Child, new Point(this._Margin, this._Margin)), Time);
        }

        protected override void OnResize(Point Size)
        {
            this._Child.Resize(Size - new Point(this._Margin, this._Margin) * 2.0);
        }

        private Control _Child;
        private double _Margin;
    }
}