using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// An alignment of content on an axis.
    /// </summary>
    public enum Align
    {
        Left = 0,
        Center = 1,
        Right = 2,
        Top = 0,
        Bottom = 2
    }

    /// <summary>
    /// A container that aligns a fixed-sized control in an area.
    /// </summary>
    public class AlignContainer : Control
    {
        public AlignContainer(Control Client, Point ClientSize, Align HorizontalAlign, Align VerticalAlign)
        {
            this._Client = Client;
            this._ClientSize = ClientSize;
            this._VerticalAlign = VerticalAlign;
            this._HorizontalAlign = HorizontalAlign;
        }

        public override void Render(GUIRenderContext Context)
        {
            Context.PushTranslate(this._ClientOffset);
            this._Client.Render(Context);
            Context.Pop();
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            this._Client.Update(Context.CreateChildContext(this._Client, this._ClientOffset), Time);
        }

        protected override void OnResize(Point Size)
        {
            double width = this._ClientSize.X;
            double height = this._ClientSize.Y;
            if (width > Size.X)
            {
                width = Size.X;
            }
            if (height > Size.Y)
            {
                height = Size.Y;
            }
            this.ResizeChild(this._Client, new Point(width, height));
        }

        private Point _ClientOffset
        {
            get
            {
                Point fullsize = this.Size;
                Point clientsize = this._Client.Size;
                double x;
                double y;
                switch (this._HorizontalAlign)
                {
                    case Align.Center: x = Math.Round(fullsize.X * 0.5 - clientsize.X * 0.5); break;
                    case Align.Right: x = fullsize.X - clientsize.X; break;
                    default: x = 0.0; break;
                }
                switch (this._VerticalAlign)
                {
                    case Align.Center: y = Math.Round(fullsize.Y * 0.5 - clientsize.Y * 0.5); break;
                    case Align.Bottom: y = fullsize.Y - clientsize.Y; break;
                    default: y = 0.0; break;
                }
                return new Point(x, y);
            }
        }

        private Control _Client;
        private Point _ClientSize;
        private Align _VerticalAlign;
        private Align _HorizontalAlign;
    }

}