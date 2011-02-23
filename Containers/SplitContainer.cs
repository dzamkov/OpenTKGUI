using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// A container that splits an area into two rectangular parts that together make up the whole area of the container.
    /// </summary>
    public class SplitContainer : Control
    {
        public SplitContainer(Axis Direction, Control Near, Control Far)
        {
            this._Direction = Direction;
            this._Near = Near;
            this._Far = Far;
        }

        /// <summary>
        /// Gets or sets the size of the near control in the direction of the split container.
        /// </summary>
        public double NearSize
        {
            get
            {
                return this._NearSize;
            }
            set
            {
                this._NearSize = value;
            }
        }

        public override void Render(RenderContext Context)
        {
            this._Near.Render(Context);
            using (Context.Translate(new Point(this._NearSize, 0.0).SwapIf(this._Direction == Axis.Vertical)))
            {
                this._Far.Render(Context);
            }
        }

        public override void Update(InputContext Context)
        {
            this._Near.Update(Context);
            using (Context.Translate(new Point(this._NearSize, 0.0).SwapIf(this._Direction == Axis.Vertical)))
            {
                this._Far.Update(Context);
            }
        }

        protected override void OnResize(Point Size)
        {
            Point asize = Size.SwapIf(this._Direction == Axis.Vertical);
            double psize = asize.X;
            if (this._NearSize > psize)
            {
                this._NearSize = psize;
            }
            this.ResizeChild(this._Near, new Point(this._NearSize, asize.Y).SwapIf(this._Direction == Axis.Vertical));
            this.ResizeChild(this._Far, new Point(psize - this._NearSize, asize.Y).SwapIf(this._Direction == Axis.Vertical));
        }

        protected override void OnDispose()
        {
            this._Near.Dispose();
            this._Far.Dispose();
        }

        private double _NearSize;
        private Axis _Direction;
        private Control _Near;
        private Control _Far;
    }
}