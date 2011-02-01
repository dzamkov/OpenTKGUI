using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// A single-lined control a user can use to input text.
    /// </summary>
    public class Textbox : Control
    {
        public Textbox(TextboxStyle Style)
        {
            this._Style = Style;
            this._Text = "";
            this._MakeTextSample();
        }

        public Textbox()
            : this(new TextboxStyle())
        {

        }

        /// <summary>
        /// Gets or sets the text in this textbox.
        /// </summary>
        public string Text
        {
            get
            {
                return this._Text;
            }
            set
            {
                this._TryChangeText(value);
            }
        }
		
		/// <summary>
		/// Gets or sets the password mask for this textbox.
		/// </summary>
		public string PasswordMask
		{
			get
			{
				return this._PasswordMask;
			}
			set
			{
				this._PasswordMask = value;
			}
		}
				

        /// <summary>
        /// Gets if the specified character is valid in a textbox.
        /// </summary>
        public static bool ValidChar(char C)
        {
            int i = (int)C;
            if (i >= 32 && i <= 126)
            {
                return true;
            }
            if (i > 255)
            {
                return true;
            }
            return false;
        }

        public override void Render(GUIRenderContext Context)
        {
            Skin s = this._Style.Skin;
            Context.DrawSurface(s.GetSurface(this._Style.Textbox, this.Size));

            Rectangle inner = new Rectangle(this.Size).Margin(this._Style.InteriorMargin);
            Context.PushClip(inner);

            // Draw text
            Point texloc = new Point(inner.Location.X, inner.Location.Y + inner.Size.Y / 2.0 - this._TextSample.Size.Y / 2.0);
            Context.DrawText(this._Style.TextColor, this._TextSample, texloc);

            
            // Draw selection
            if (this._Selection != null)
            {
                int starti;
                int endi;
                this._Selection.Order(out starti, out endi);
                Rectangle[] charbounds = this._TextSample.CharacterBounds;
                if (endi - starti > 0)
                {
                    double startx = inner.Location.X + SelectionX(charbounds, starti);
                    double endx = inner.Location.X + SelectionX(charbounds, endi);
                    Rectangle clip = new Rectangle(startx, inner.Location.Y, endx - startx, inner.Size.Y);
                    Context.PushClip(clip);
                    Context.DrawSolid(this._Style.SelectionBackgroundColor, clip);
                    Context.DrawText(this._Style.SelectionTextColor, this._TextSample, texloc);
                    Context.Pop();
                }

                // Draw cursor
                if (_CursorFlashTime > 0.0)
                {
                    int curi = this._Selection.Start;
                    double cursx = inner.Location.X + SelectionX(charbounds, curi);
                    Context.DrawSolid(this._Style.CursorColor, new Rectangle(cursx, inner.Location.Y, 1.0, inner.Size.Y));
                }
            }

            Context.Pop();
        }

        public override void Update(GUIControlContext Context, double Time)
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
            TextSelection ts = this._Selection;
            if (ts != null && !Context.HasKeyboard)
            {
                this._Selection = ts = null;
                if (this.TextEntered != null)
                {
                    this.TextEntered(this._Text);
                }
            }

            // Flash the cursor
            this._CursorFlashTime += Time;
            double cfr = this._Style.CursorFlashRate;
            while (this._CursorFlashTime > cfr)
            {
                this._CursorFlashTime -= cfr * 2.0;
            }

            // Update text
            KeyboardState ks = Context.KeyboardState;
            if (ks != null && ts != null)
            {
                bool changed = false;
                foreach (char c in ks.Presses)
                {
                    int starti;
                    int endi;
                    ts.Order(out starti, out endi);

                    if (c == '\b')
                    {
                        if (endi - starti > 0)
                        {
                            if(this._TryChangeText(this._Text.Substring(0, starti) + this._Text.Substring(endi, this._Text.Length - endi)))
                            {
                                this._Selection = new TextSelection(starti);
                                changed = true;
                            }
                        }
                        else
                        {
                            if (starti > 0)
                            {
                                if (this._TryChangeText(this._Text.Substring(0, starti - 1) + this._Text.Substring(starti, this._Text.Length - starti)))
                                {
                                    this._Selection = new TextSelection(starti - 1);
                                    changed = true;
                                }
                            }
                        }
                        continue;
                    }

                    if (ValidChar(c))
                    {
                        if (this._TryChangeText(this._Text.Substring(0, starti) + c + this._Text.Substring(endi, this._Text.Length - endi)))
                        {
                            this._Selection = new TextSelection(starti + 1);
                            changed = true;
                        }
                    }
                }
                if (changed)
                {
                    this._CursorFlashTime = 0.0;
                    this._TextSample.Dispose();
                    this._MakeTextSample();
                }

                // Navigation
                foreach (KeyEvent ke in ks.Events)
                {
                    if (ke.Type == ButtonEventType.Down)
                    {
                        int ss = this._Selection.Start;
                        if (ke.Key == Key.Left)
                        {
                            if (ss > 0)
                            {
                                this._Selection = new TextSelection(ss - 1);
                            }
                        }
                        if (ke.Key == Key.Right)
                        {
                            if (ss < this._Text.Length)
                            {
                                this._Selection = new TextSelection(ss + 1);
                            }
                        }
                    }
                }

                // Enter?
                if (ks.IsKeyDown(Key.Enter))
                {
                    this._Selection = null;
                    Context.ReleaseKeyboard();

                    if (this.TextEntered != null)
                    {
                        this.TextEntered(this._Text);
                    }
                }
            }
        }

        private void _MakeTextSample()
        {
			string text = this._Text;
			if(this._PasswordMask != null && this._PasswordMask.Length != 0)
			{
				text = "";
				for(int i = 0; i < this._Text.Length; i++)
					text += this._PasswordMask;
			}
			this._TextSample = this._Style.Font.CreateSample(text);
        }

        /// <summary>
        /// Gets the index of the selected text at the specified relative position.
        /// </summary>
        private int _SelectedIndex(Point Point)
        {
            double x = Point.X;
            x -= this._Style.InteriorMargin;
            Rectangle[] charbounds = this._TextSample.CharacterBounds;
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

        /// <summary>
        /// Gets the x coordinate of the selection when its at the specified index.
        /// </summary>
        public static double SelectionX(Rectangle[] CharacterBounds, int Index)
        {
            if (Index < CharacterBounds.Length)
            {
                return CharacterBounds[Index].Location.X;
            }
            else
            {
                if (CharacterBounds.Length == 0)
                {
                    return 0.0;
                }
                else
                {
                    Rectangle r = CharacterBounds[CharacterBounds.Length - 1];
                    return r.Location.X + r.Size.X;
                }
            }
        }

        protected override void OnDispose()
        {
            if (this._TextSample != null)
            {
                this._TextSample.Dispose();
            }
        }

        /// <summary>
        /// Tries changing the text to the specified new string. Returns true if sucsessful.
        /// </summary>
        private bool _TryChangeText(string New)
        {
            this._Text = New;
            this._TextSample.Dispose();
            this._MakeTextSample();
            if (this.TextChanged != null)
            {
                this.TextChanged.Invoke(New);
            }
            return true;
        }

        /// <summary>
        /// An event that is fired whenever the text shown on the textbox is changed.
        /// </summary>
        public event TextChangedHandler TextChanged;

        /// <summary>
        /// An event that is fired whenever the user enters a new text string into the textbox. (After they press enter or take keyboard focus.)
        /// </summary>
        public event TextEnteredHandler TextEntered;

        private double _CursorFlashTime;
        private bool _MouseDrag;
        private TextSelection _Selection;
        private string _Text;
		private string _PasswordMask;
        private TextSample _TextSample;
        private TextboxStyle _Style;
    }


    /// <summary>
    /// Represents a selection of text given by the user.
    /// </summary>
    public class TextSelection
    {
        public TextSelection()
        {

        }

        public TextSelection(int Start, int End)
        {
            this.Start = Start;
            this.End = End;
        }

        public TextSelection(int Select)
        {
            this.Start = this.End = Select;
        }

        /// <summary>
        /// Gets the ordered start and end indices of the selection.
        /// </summary>
        public void Order(out int Start, out int End)
        {
            if (this.Start > this.End)
            {
                Start = this.End;
                End = this.Start;
            }
            else
            {
                Start = this.Start;
                End = this.End;
            }
        }

        /// <summary>
        /// The character the start (the first point selected, not the first index) of the selection is before.
        /// </summary>
        public int Start;

        /// <summary>
        /// The character the end of the selection is before.
        /// </summary>
        public int End;
    }

    /// <summary>
    /// Gives styling options for a textbox.
    /// </summary>
    public sealed class TextboxStyle
    {
        public Skin Skin = Skin.Default;
        public SkinArea Textbox = new SkinArea(48, 0, 16, 16);
        public double CursorFlashRate = 0.5;
        public double InteriorMargin = 4.0;
        public Color CursorColor = Color.RGB(0.0, 0.0, 0.0);
        public Color SelectionBackgroundColor = Color.RGB(0.0, 0.5, 1.0);
        public Color SelectionTextColor = Color.RGB(1.0, 1.0, 1.0);
        public Color TextColor = Color.RGB(0.0, 0.0, 0.0);
        public Font Font = Font.Default;
    }
}