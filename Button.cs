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
            Context.DrawSkinPart(Skin.Default.GetPart(0, 0, 32, 32), new Rectangle(this.Size));
        }

        private string _Text;
    }
}