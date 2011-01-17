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
            this._CursorPosition = 1;
            this._Text = "Test Test";
        }

        public override void Render(GUIRenderContext Context)
        {
            Skin s = Skin.Default;
            Context.DrawSkinPart(s.GetPart(96, 0, 32, 32), new Rectangle(this.Size));

            Rectangle inner = new Rectangle(this.Size).Pad(_Padding);
            Context.PushClip(inner);

            // Draw text
            if (this._Sample == null)
            {
                this._Sample = new SystemFont("Verdana", 17.0, true).GetSample(this._Text);
            }
            Context.DrawText(Color.RGB(0.0, 0.0, 0.0), this._Sample, inner.Location);

            // Draw flashy cursor thing
            if (this._CursorPosition.HasValue)
            {
                int cp = this._CursorPosition.Value;
                if (this._CursorFlashTime < 0)
                {
                    double cursx;
                    if (cp == 0)
                    {
                        cursx = 0;
                    }
                    else
                    {
                        cursx = this._Sample.CharacterBounds[cp].Location.X;
                    }
                    Context.DrawSolid(Color.RGB(0.0, 0.0, 0.0), new Rectangle(inner.Location.X + cursx, inner.Location.Y, 1.0, inner.Size.Y));
                }
            }

            Context.Pop();
        }

        public override void Update(GUIContext Context, double Time)
        {
            // Cursor flashing
            if (this._CursorPosition.HasValue)
            {
                this._CursorFlashTime += Time;
                while (this._CursorFlashTime > _CursorFlashRate)
                {
                    this._CursorFlashTime -= _CursorFlashRate * 2.0;
                }
            }

            // Text updates
            Context.KeyboardFocus = this;
            KeyboardState ks = Context.KeyboardState;
            foreach (char k in ks.Presses)
            {
                
            }
        }

        private const double _Padding = 4.0;
        private const double _CursorFlashRate = 0.1;

        private double _CursorFlashTime;
        private int? _CursorPosition; // Cursor is before this character.
        private string _Text;
        private TextSample _Sample;
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
        /// The character the start of the selection is before.
        /// </summary>
        public int Start;

        /// <summary>
        /// The character the end of the selection is before.
        /// </summary>
        public int End;
    }
}