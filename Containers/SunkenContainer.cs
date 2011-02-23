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
            : this(SunkenContainerStyle.Default, Client)
        {

        }

        public SunkenContainer(SunkenContainerStyle Style, Control Client)
            : base(Client)
        {
            this._Style = Style;
        }

        public override void Render(RenderContext Context)
        {
            base.Render(Context);
            Context.DrawSurface(this._Style.Shading, new Rectangle(this.Size));
        }

        private SunkenContainerStyle _Style;
    }

    /// <summary>
    /// Gives styling options for a sunken container.
    /// </summary>
    public class SunkenContainerStyle
    {
        public SunkenContainerStyle()
        {

        }

        public SunkenContainerStyle(Skin Skin)
        {
            this.Shading = Skin.GetStretchableSurface(new SkinArea(96, 112, 16, 16));
        }

        public static readonly SunkenContainerStyle Default = new SunkenContainerStyle(Skin.Default);

        public Surface Shading;
    }
}