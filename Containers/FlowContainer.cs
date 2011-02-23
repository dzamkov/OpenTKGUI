using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// A container of controls that arranges child controls one by one in a line.
    /// </summary>
    public class FlowContainer : Control
    {
        public FlowContainer(Axis Direction)
            : this(0.0, Direction)
        {

        }

        public FlowContainer(double Seperation, Axis Direction)
        {
            this._Seperation = Seperation;
            this._Direction = Direction;
            this._Children = new List<_Child>();
        }

        /// <summary>
        /// Adds a child to the end of the flow container. Size is the size of the child on the axis of the flow container.
        /// </summary>
        public void AddChild(Control Child, double Size)
        {
            this._Children.Add(new _Child()
            {
                Control = Child,
                Size = Size
            });
        }

        /// <summary>
        /// Gets a suggestion for the length (size along the direction of the flow container) of the container such that all children
        /// will fit.
        /// </summary>
        public double SuggestLength
        {
            get
            {
                double d = 0.0;
                if (this._Children.Count > 1)
                {
                    d += this._Seperation * (this._Children.Count - 1);
                }
                foreach (_Child c in this._Children)
                {
                    d += c.Size;
                }
                return d;
            }
        }

        public override void Render(RenderContext Context)
        {
            using (Context.Clip(new Rectangle(this.Size)))
            {
                double d = 0.0;
                foreach (_Child c in this._Children)
                {
                    using (Context.Translate(new Point(d, 0.0).SwapIf(this._Direction == Axis.Vertical)))
                    {
                        c.Control.Render(Context);
                    }
                    d += c.Size + this._Seperation;
                }
            }
        }

        public override void Update(InputContext Context)
        {
            double d = 0.0;
            foreach (_Child c in this._Children)
            {
                using (Context.Translate(new Point(d, 0.0).SwapIf(this._Direction == Axis.Vertical)))
                {
                    c.Control.Update(Context);
                }
                d += c.Size + this._Seperation;
            }
        }

        protected override void OnResize(Point Size)
        {
            if (this._Direction == Axis.Horizontal)
            {
                double height = Size.Y;
                foreach (_Child c in this._Children)
                {
                    this.ResizeChild(c.Control, new Point(c.Size, height));
                }
            }
            else
            {
                double width = Size.X;
                foreach (_Child c in this._Children)
                {
                    this.ResizeChild(c.Control, new Point(width, c.Size));
                }
            }
        }

        private struct _Child
        {
            public Control Control;
            public double Size;
        }

        private List<_Child> _Children;
        private double _Seperation;
        private Axis _Direction;
    }
}