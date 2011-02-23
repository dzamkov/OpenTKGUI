using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// A control to display the progress of an operation
    /// </summary>
    public class Progressbar : Control
    {
        public Progressbar()
            : this(ProgressbarStyle.Default)
        {

        }

        public Progressbar(ProgressbarStyle Style)
        {
            this._Style = Style;
        }

        /// <summary>
        /// Gets or sets the value of the progreesbar, between 0.0 and 1.0.
        /// </summary>
        public double Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                this._Value = value;
                this._Value = Math.Max(0.0, Math.Min(1.0, this._Value));
            }
        }


        public override void Render(RenderContext Context)
        {
            ProgressbarStyle style = this._Style;
			Context.DrawSurface(style.Backing, new Rectangle(this.Size));
			
			double width = this.Size.X * this._Value;
			Rectangle clip = new Rectangle(0, 0, width, this.Size.Y);
            using (Context.Clip(clip))
            {
                Context.DrawSurface(style.Progress, new Rectangle(this.Size));
            }
        }
		
        public override void Update(InputContext Context)
        {

        }

        private double _Value;
        private ProgressbarStyle _Style;
    }

    /// <summary>
    /// Gives styling options for a progress bar.
    /// </summary>
    public class ProgressbarStyle
    {
        public ProgressbarStyle()
        {

        }

        public ProgressbarStyle(Skin Skin)
        {
            this.Backing = Skin.GetStretchableSurface(new SkinArea(64, 48, 32, 32));
            this.Progress = Skin.GetStretchableSurface(new SkinArea(96, 48, 32, 32));
        }

        public static readonly ProgressbarStyle Default = new ProgressbarStyle(Skin.Default);

        public Surface Backing;
        public Surface Progress;
    }
}