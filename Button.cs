using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// A clickable button with text.
    /// </summary>
    public class Button : Control
    {
        public Button(string Text)
        {
            this._Text = Text;
        }

        public override void Render(GUIRenderContext Context)
        {
            Skin s = Skin.Default;
            Context.DrawSkinPart(
                Color.Mix(Color.RGB(1.0, 1.0, 1.0), Color.RGB(0.9, 0.9, 1.0), this._MouseOverHighlight / _HighlightMax),
                Skin.Default.GetPart(0, 0, 32, 32), new Rectangle(this.Size));
        }

        public override void Update(GUIContext Context, double Time)
        {
            MouseState ms = Context.MouseState;
            if (ms != null)
            {
                // Mouse is over button
                this._MouseOverHighlight = Math.Min(this._MouseOverHighlight + Time, _HighlightMax);
            }
            else
            {
                // Mouse is not over button
                this._MouseOverHighlight = Math.Max(this._MouseOverHighlight - Time, 0.0);
            }
        }

        private const double _HighlightMax = 0.1;

        private double _MouseOverHighlight;
        private string _Text;
    }
}