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
        public Textbox()
        {
            this._Text = "";
            this._MakeTextSample();
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

        public override void Render(GUIRenderContext Context)
        {
            Skin s = Skin.Default;
            Context.DrawSurface(s.GetSurface(96, 0, 32, 32, this.Size));

            Rectangle inner = new Rectangle(this.Size).Pad(_Padding);
            Context.PushClip(inner);

            // Draw text
            Context.DrawText(Color.RGB(0.0, 0.0, 0.0), this._TextSample, inner.Location);

            
            // Draw selection
            if (this._Selection != null)
            {
                int starti;
                int endi;
                this._Selection.Order(out starti, out endi);
                Rectangle[] charbounds = this._TextSample.CharacterBounds;
                if (endi - starti > 0)
                {
                    double startx = inner.Location.X + _SelectionX(charbounds, starti);
                    double endx = inner.Location.X + _SelectionX(charbounds, endi);
                    Rectangle clip = new Rectangle(startx, inner.Location.Y, endx - startx, inner.Size.Y);
                    Context.PushClip(clip);
                    Context.DrawSolid(Color.RGB(0.0, 0.5, 1.0), clip);
                    Context.DrawText(Color.RGB(1.0, 1.0, 1.0), this._TextSample, inner.Location);
                    Context.Pop();
                }

                // Draw cursor
                if (_CursorFlashTime > 0.0)
                {
                    int curi = this._Selection.Start;
                    double cursx = inner.Location.X + _SelectionX(charbounds, curi);
                    Context.DrawSolid(Color.RGB(0.0, 0.0, 0.0), new Rectangle(cursx, inner.Location.Y, 1.0, inner.Size.Y));
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
                if (ms.IsButtonDown(MouseButton.Left))
                {
                    if (!this._MouseDown)
                    {
                        Context.CaptureMouse();
                        Context.CaptureKeyboard();
                        this._Selection = new TextSelection(this._SelectedIndex(ms.Position));
                        this._MouseDown = true;
                    }
                    else
                    {
                        this._Selection.End = this._SelectedIndex(ms.Position);
                    }
                }
                else
                {
                    if (this._MouseDown)
                    {
                        Context.ReleaseMouse();
                        this._MouseDown = false;
                    }
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
            while (this._CursorFlashTime > _CursorFlashRate)
            {
                this._CursorFlashTime -= _CursorFlashRate * 2.0;
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

                    if (TextSample.ValidChar(c))
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
            this._TextSample = new SystemFont("Verdana", 16.0, true).GetSample(this._Text);
        }

        /// <summary>
        /// Gets the index of the selected text at the specified relative position.
        /// </summary>
        private int _SelectedIndex(Point Point)
        {
            double x = Point.X;
            x -= _Padding;
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
        private static double _SelectionX(Rectangle[] CharacterBounds, int Index)
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

        private const double _Padding = 4.0;
        private const double _CursorFlashRate = 0.1;

        private double _CursorFlashTime;
        private bool _MouseDown;
        private TextSelection _Selection;
        private string _Text;
        private TextSample _TextSample;
    }

    /// <summary>
    /// Handler for a text changed event from a textbox.
    /// </summary>
    public delegate void TextChangedHandler(string Text);

    /// <summary>
    /// Handler for a text entered event from a textbox.
    /// </summary>
    public delegate void TextEnteredHandler(string Text);

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
}