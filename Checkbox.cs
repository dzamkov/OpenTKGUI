
using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// A control to display the progress of an operation
    /// </summary>
    public class Checkbox : Control
    {
        public Checkbox() : this(false, "Checkbox")
        {
        }

		public Checkbox(bool chcked, string text)
        {
            this._Checked = chcked;
			this.Text = text;
			this._Color = Color.RGB(255,255,255);
			this._Box = new SkinArea(80, 80, this._BoxSize, this._BoxSize);
			this._Tick = new SkinArea(64, 80, this._BoxSize, this._BoxSize);
        }
		
        /// <summary>
        /// Gets or sets the value of the checkbox
        /// </summary>
        public bool Checked
        {
            get
            {
                return this._Checked;
            }
            set
            {
                this._Checked = value;
            }
        }

		/// <summary>
        /// Gets or sets the lables text on the checkbox
        /// </summary>
        public string Text
        {
            get
            {
                return this._Text;
            }
            set
            {
                this._Text = value;
				if(this._Sample != null)
					this._Sample.Dispose();
				this._Sample = null;
            }
        }

        public override void Render(GUIRenderContext Context)
        {
			Skin skin = Skin.Default;
			
			Surface surfbox = skin.GetSurface(this._Box, new Point(this._BoxSize, this._BoxSize));
			Context.DrawSurface(surfbox);
			
			if(this.Checked)
			{
				Surface surftick = skin.GetSurface(this._Tick, new Point(this._BoxSize, this._BoxSize));
				Context.DrawSurface(surftick);
			}
			
			if(this._Sample == null)
				this._Sample = Font.Default.CreateSample(this._Text);
			Context.DrawText(this._Color, this._Sample, new Rectangle(this._BoxSize + this._Padding, 0, this.Size.X - this._BoxSize, this.Size.Y));
        }
		
        public override void Update(GUIControlContext Context, double Time)
        {
			MouseState ms = Context.MouseState;
            if (ms != null)
            {
                if (ms.IsButtonDown(MouseButton.Left))
                {
                    this._MouseDown = true;
                }
                else
                {
                    if (this._MouseDown)
                    {
                        this._Click();
                        this._MouseDown = false;
                    }
                }
            }
            else
            {
                this._MouseDown = false;
            }
        }

        protected override void OnResize(Point OldSize, Point NewSize)
        {
            
        }
		
		private void _Click()
		{
			this.Checked = !this.Checked;
			if(this.Click != null)
			{
				this.Click(this.Checked);
			}
		}
		
		private TextSample _Sample;
		private Color _Color;
        private bool _Checked;
		private string _Text;
		
		private bool _MouseDown = false;
		
		public CheckboxClickHandler Click;
		
		private int _BoxSize = 16;
		private int _Padding = 5;
		
		private SkinArea _Tick;
    	private SkinArea _Box;
    }
}