using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// A container that applies a border around its client. The border thickness may be specified in any direction seperately.
    /// </summary>
    public class BorderContainer : Control
    {
        public BorderContainer(Control Client)
        {
            this._Client = Client;
            this._Color = Color.RGB(0.0, 0.0, 0.0);
        }

        /// <summary>
        /// Gets or sets the border color.
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

        /// <summary>
        /// Sets the thickness of a uniform border.
        /// </summary>
        public double Uniform
        {
            set
            {
                this._Left = this._Right = this._Top = this._Bottom = value;
                this.OnResize(this.Size);
            }
        }

        /// <summary>
        /// Sets all the independant border parts at once to avoid recalculation.
        /// </summary>
        public void Set(double Left, double Top, double Right, double Bottom)
        {
            this._Left = Left;
            this._Top = Top;
            this._Right = Right;
            this._Bottom = Bottom;
            this.OnResize(this.Size);
        }

        /// <summary>
        /// Gets or sets the thickness of the left part of the border.
        /// </summary>
        public double Left
        {
            get
            {
                return this._Left;
            }
            set
            {
                this._Left = value;
                this.OnResize(this.Size);
            }
        }

        /// <summary>
        /// Gets or sets the thickness of the right part of the border.
        /// </summary>
        public double Right
        {
            get
            {
                return this._Right;
            }
            set
            {
                this._Right = value;
                this.OnResize(this.Size);
            }
        }

        /// <summary>
        /// Gets or sets the thickness of the top part of the border.
        /// </summary>
        public double Top
        {
            get
            {
                return this._Top;
            }
            set
            {
                this._Top = value;
                this.OnResize(this.Size);
            }
        }

        /// <summary>
        /// Gets or sets the thickness of the bottom part of the border.
        /// </summary>
        public double Bottom
        {
            get
            {
                return this._Bottom;
            }
            set
            {
                this._Bottom = value;
                this.OnResize(this.Size);
            }
        }

        public override void Render(RenderContext Context)
        {
            Point size = this.Size;
            if (this._Left > 0.0) Context.DrawSolid(this._Color, new Rectangle(0.0, 0.0, this._Left, size.Y));
            if (this._Top > 0.0) Context.DrawSolid(this._Color, new Rectangle(0.0, 0.0, size.X, this._Top));
            if (this._Right > 0.0) Context.DrawSolid(this._Color, new Rectangle(size.X - this._Right, 0.0, this._Right, size.Y));
            if (this._Bottom > 0.0) Context.DrawSolid(this._Color, new Rectangle(0.0, size.Y - this._Bottom, size.X, this._Bottom));
            using (Context.Translate(new Point(this._Left, this._Top)))
            {
                this._Client.Render(Context);
            }
        }

        public override void Update(InputContext Context)
        {
            using (Context.Translate(new Point(this._Left, this._Top)))
            {
                this._Client.Update(Context);
            }
        }

        protected override void OnDispose()
        {
            this._Client.Dispose();    
        }

        protected override void OnResize(Point Size)
        {
            Point clisize = this.Size;
            clisize.X -= this._Left + this._Right;
            clisize.Y -= this._Top + this._Bottom;
            this.ResizeChild(this._Client, clisize);
        }

        private double _Left;
        private double _Right;
        private double _Top;
        private double _Bottom;
        private Color _Color;
        private Control _Client;
    }
}