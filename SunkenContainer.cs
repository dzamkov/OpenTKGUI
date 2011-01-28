using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// A simple container that uses shading to give the client a sunken appearance. The client should be bordered on all sides for this to look right.
    /// </summary>
    public class SunkenContainer : SingleContainer
    {
        public SunkenContainer(Control Client)
            : this(new SunkenContainerStyle(), Client)
        {

        }

        public SunkenContainer(SunkenContainerStyle Style, Control Client)
            : base(Client)
        {
            this._Style = Style;
        }

        public override void Render(GUIRenderContext Context)
        {
            base.Render(Context);
            Context.DrawSurface(this._Style.Skin.GetSurface(this._Style.Shading, this.Size), new Point(0.0, 0.0));
        }

        private SunkenContainerStyle _Style;
    }

    /// <summary>
    /// Gives styling options for a sunken container.
    /// </summary>
    public class SunkenContainerStyle
    {
        public Skin Skin = Skin.Default;
        public SkinArea Shading = new SkinArea(96, 112, 16, 16);
    }
}