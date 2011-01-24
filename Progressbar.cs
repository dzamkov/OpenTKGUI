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
            : this(new ProgressbarStyle())
        {

        }

        public Progressbar(ProgressbarStyle Style)
        {
            this._Style = Style;
            this._Value = 0.01;
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


        public override void Render(GUIRenderContext Context)
        {
            Skin skin = this._Style.Skin;
			Surface surfback = skin.GetSurface(this._Style.Backing, this.Size);
			Context.DrawSurface(surfback);
			
			double width = this.Size.X * this._Value;
			Rectangle clip = new Rectangle(0, 0, width, this.Size.Y);
			Context.PushClip(clip);
			Surface surfprog = skin.GetSurface(this._Style.Progress, this.Size);
            Context.DrawSurface(surfprog);
			Context.Pop();
            
        }
		
        public override void Update(GUIControlContext Context, double Time)
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
        public Skin Skin = Skin.Default;
        public SkinArea Backing = new SkinArea(64, 48, 32, 32);
        public SkinArea Progress = new SkinArea(96, 48, 32, 32);
    }
}