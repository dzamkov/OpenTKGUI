using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// A control a user can use to input text.
    /// </summary>
    public class Textbox : Control
    {
        public override void Render(GUIRenderContext Context)
        {
            Skin s = Skin.Default;
            Context.DrawSkinPart(s.GetPart(96, 0, 32, 32), new Rectangle(this.Size));
        }

        public override void Update(GUIContext Context, double Time)
        {
            
        }
    }
}