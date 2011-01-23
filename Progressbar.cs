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
        {
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
			
			
			Skin skin = Skin.Default;
			
			Surface surfback = skin.GetSurface(this._Backing, this.Size);
			Context.DrawSurface(surfback);
			
			double width = this.Size.X * this._Value;
			Rectangle clip_size = new Rectangle(0, 0,
			                                    width, this.Size.Y);
			Context.PushClip(clip_size);
			
			Surface surfprog = skin.GetSurface(this._Progress, this.Size);
            Context.DrawSurface(surfprog);
			
			Context.Pop();
            
        }
		
        public override void Update(GUIControlContext Context, double Time)
        {
        }

        protected override void OnResize(Point OldSize, Point NewSize)
        {
            
        }
        private double _Value;
		private SkinArea _Backing = new SkinArea(64, 48, 32, 32);
    	private SkinArea _Progress = new SkinArea(96, 48, 32, 32);
    }
}