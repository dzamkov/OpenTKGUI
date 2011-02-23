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
        public Checkbox() 
            : this(false, "Checkbox")
        {
        }

        public Checkbox(bool Checked, string Text)
            : this(CheckboxStyle.Default, Checked, Text)
        {

        }

        public Checkbox(CheckboxStyle Style, bool Checked, string Text)
        {
            this._Style = Style;
            this._Checked = Checked;
            this._Text = Text;
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

        public override void Render(RenderContext Context)
        {
			CheckboxStyle style = this._Style;
			Context.DrawSurface(style.Box, new Rectangle(style.BoxSize));
			
			if(this.Checked)
			{
                Context.DrawSurface(style.Tick, new Rectangle(style.BoxSize));
			}
			

            Rectangle textrect = new Rectangle(style.BoxSize.X + style.Padding, 0, this.Size.X - style.BoxSize.X, this.Size.Y);
			if(this._Sample == null)
				this._Sample = style.TextFont.CreateSample(this._Text, textrect.Size, TextAlign.Left, TextAlign.Top, TextWrap.Wrap);
			Context.DrawText(style.TextColor, this._Sample, textrect);
        }
		
        public override void Update(InputContext Context)
        {
			MouseState ms = Context.MouseState;
            if (ms != null)
            {
                if (ms.HasReleasedButton(MouseButton.Left))
                {
                    this._Click();
                }
            }
        }

        protected override void OnResize(Point Size)
        {
            if (this._Sample != null)
            {
                this._Sample.Dispose();
                this._Sample = null;
            }
        }

        protected override void OnDispose()
        {
            if (this._Sample != null)
            {
                this._Sample.Dispose();
            }
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
        private bool _Checked;
		private string _Text;
		
		public CheckboxClickHandler Click;

        private CheckboxStyle _Style;
    }

    /// <summary>
    /// Gives styling options for a checkbox.
    /// </summary>
    public class CheckboxStyle
    {
        public CheckboxStyle()
        {

        }

        public CheckboxStyle(Skin Skin)
        {
            this.Tick = Skin.GetStretchableSurface(new SkinArea(64, 80, 16, 16));
            this.Box = Skin.GetStretchableSurface(new SkinArea(80, 80, 16, 16));
        }

        public static readonly CheckboxStyle Default = new CheckboxStyle(Skin.Default);

        public Surface Tick;
        public Surface Box;
        public Point BoxSize = new Point(16.0, 16.0);
        public double Padding = 5.0;
        public Font TextFont = Font.Default;
        public Color TextColor = Color.RGB(0.0, 0.0, 0.0);
    }
}