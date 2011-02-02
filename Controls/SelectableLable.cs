using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// A control containing selectable text (with no background).
    /// </summary>
    public class SelectableLabel : Control
    {
        public SelectableLabel(string Text)
            : this(Text, Color.RGB(0.0, 0.0, 0.0), new SelectableLabelStyle())
        {

        }

        public SelectableLabel(string Text, Color Color)
            : this(Text, Color, new SelectableLabelStyle())
        {

        }

        public SelectableLabel(string Text, Color Color, SelectableLabelStyle Style)
        {
            this._Text = Text;
            this._Color = Color;
            this._Style = Style;
        }

        /// <summary>
        /// Gets or sets the text on the label.
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
                if (this._Sample != null)
                {
                    this._Sample.Dispose();
                    this._Sample = null;
                }
            }
        }

        /// <summary>
        /// Gets the wrapping mode of this label.
        /// </summary>
        public TextWrap Wrap
        {
            get
            {
                return this._Style.Wrap;
            }
        }

        /// <summary>
        /// Gets the horizontal align mode on the label.
        /// </summary>
        public TextAlign HorizontalAlign
        {
            get
            {
                return this._Style.HorizontalAlign;
            }
        }

        /// <summary>
        /// Gets the vertical align mode on the label.
        /// </summary>
        public TextAlign VerticalAlign
        {
            get
            {
                return this._Style.VerticalAlign;
            }
        }

        /// <summary>
        /// Suggests the size of the label so that no text overflows.
        /// </summary>
        public Point SuggestSize
        {
            get
            {
                if (this._Sample == null)
                {
                    this._CreateSample();
                }
                return this._Sample.Size;
            }
        }

        /// <summary>
        /// Gets the height required for the label if the width is known.
        /// </summary>
        public double GetHeight(double Width)
        {
            TextSample test = this._Style.Font.CreateSample(this.Text, new Point(Width, 4096.0), this._Style.HorizontalAlign, this._Style.VerticalAlign, this._Style.Wrap);
            double height = test.Size.Y;
            test.Dispose();
            return Math.Ceiling(height);
        }

        public override void Render(GUIRenderContext Context)
        {
            if (this._Text != null && this._Text != "")
            {
                if (this._Sample == null)
                {
                    this._CreateSample();
                }
                //Context.DrawText(this._Color, this._Sample, new Rectangle(this.Size));
				
				/*
				Skin s = this._Style.Skin;
	            Context.DrawSurface(s.GetSurface(this._Style.Textbox, this.Size));
				*/
	            Rectangle inner = new Rectangle(this.Size);
	            Context.PushClip(inner);
	
	            // Draw text
	            Point texloc = new Point(inner.Location.X, inner.Location.Y + inner.Size.Y / 2.0 - this._Sample.Size.Y / 2.0);
	            Context.DrawText(this._Color, this._Sample, texloc);
	
	            
	            // Draw selection
	            if (this._Selection != null)
	            {
	                int starti;
	                int endi;
	                this._Selection.Order(out starti, out endi);
	                Rectangle[] charbounds = this._Sample.CharacterBounds;
	                if (endi - starti > 0)
	                {
	                    double startx = inner.Location.X + Textbox.SelectionX(charbounds, starti);
	                    double endx = inner.Location.X + Textbox.SelectionX(charbounds, endi);
	                    Rectangle clip = new Rectangle(startx, inner.Location.Y, endx - startx, inner.Size.Y);
	                    Context.PushClip(clip);
	                    Context.DrawSolid(this._Style.SelectionBackgroundColor, clip);
	                    Context.DrawText(this._Style.SelectionTextColor, this._Sample, texloc);
	                    Context.Pop();
	                }
	
	            }
	
	            Context.Pop();
            }
        }

		public override void Update (GUIControlContext Context, double Time)
		{
			// Handle mouse selection.
            MouseState ms = Context.MouseState;
            if (ms != null)
            {
                if (this._MouseDrag)
                {
                    if(ms.IsButtonDown(MouseButton.Left))
                    {
                        this._Selection.End = this._SelectedIndex(ms.Position);
                    }
                    else
                    {
                        Context.ReleaseMouse();
                        this._MouseDrag = false;
                    }
                }
                if (ms.HasPushedButton(MouseButton.Left))
                {
                    Context.CaptureMouse();
                    Context.CaptureKeyboard();
                    this._Selection = new TextSelection(this._SelectedIndex(ms.Position));
                    this._MouseDrag = true;
                }
            }
            
        }

        /// <summary>
        /// Gets the index of the selected text at the specified relative position.
        /// </summary>
        private int _SelectedIndex(Point Point)
        {
            double x = Point.X;
            Rectangle[] charbounds = this._Sample.CharacterBounds;
            if (charbounds.Length == 0)
            {
                return 0;
            }
            else
            {
                return _SelectedIndex(charbounds, x, 0, charbounds.Length);
            }
        }

        /// <summary>
        /// Gets the selection index of a position with an x coordinate of X relative to the inner region of the textbox assuming that the selection is somewhere
        /// in the given text region.
        /// </summary>
        private static int _SelectedIndex(Rectangle[] CharacterBounds, double X, int Index, int Length)
        {
            int midi = Index + Length / 2;
            Rectangle midr = CharacterBounds[midi];
            double mid = midr.Location.X + midr.Size.X / 2.0;
            if (Length == 1)
            {
                if (X > mid)
                {
                    return Index + 1;
                }
                else
                {
                    return Index;
                }
            }
            else
            {
                if (X > mid)
                {
                    return _SelectedIndex(CharacterBounds, X, midi, Index + Length - midi);
                }
                else
                {
                    return _SelectedIndex(CharacterBounds, X, Index, midi - Index);
                }
            }
		}
		
        protected override void OnResize(Point Size)
        {
            this._Sample = null;
        }

        private void _CreateSample()
        {
            Font font = this._Style.Font;
            string text = this._Text;
            this._Sample = font.CreateSample(text, this.Size, this._Style.HorizontalAlign, this._Style.VerticalAlign, this._Style.Wrap);
        }

		
        protected override void OnDispose()
        {
            if (this._Sample != null)
            {
                this._Sample.Dispose();
            }
        }

        private SelectableLabelStyle _Style;
        private Color _Color;
        private string _Text;
		private bool _MouseDrag;
        private TextSelection _Selection;
        private TextSample _Sample;
    }

    /// <summary>
    /// Gives styling options for a label.
    /// </summary>
    public class SelectableLabelStyle
    {
		public Color SelectionBackgroundColor = Color.RGB(0.2,0.2,0.8);
		public Color SelectionTextColor = Color.RGB(0.8,0.8,0.8);
        public TextAlign HorizontalAlign = TextAlign.Left;
        public TextAlign VerticalAlign = TextAlign.Top;
        public TextWrap Wrap = TextWrap.Wrap;
        public Font Font = Font.Default;
    }
}
