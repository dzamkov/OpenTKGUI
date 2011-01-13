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
            this._Texture = -1;
        }

        public override void Render(GUIRenderContext Context)
        {
            if (this._Texture < 0)
            {
                this._Texture = Image.ToMonochrome<LightImage>(
                    new LightImage(
                        new Point(-1.0, -0.5), new HeightImage(this.Size).MakeStatic())
                    ).MakeTexture();
            }
            Context.DrawTexture(this._Texture, new Rectangle(this.Size));
        }

        /// <summary>
        /// An image giving the relative heights at parts of the button.
        /// </summary>
        public class HeightImage : Image<double>
        {
            public HeightImage(ImageSize Size)
            {
                this._Size = Size;
            }

            public override ImageSize Size
            {
                get
                {
                    return this._Size;
                }
            }

            public override double Get(int X, int Y)
            {
                Point p = new Point(
                    X / (double)this._Size.Width,
                    Y / (double)this._Size.Height);
                double cdis = 1.8;
                double crad = new Point(0.5 - cdis, 0.5).Length; double cradsq = crad * crad;
                double cmax = 0.5 - cdis + crad;
                double ya = p.Y * (1.0 - p.Y) * 4.0;
                if ((p - new Point(0.5 - cdis, 0.5)).SquareLength < cradsq)
                {
                    return (p.X / cmax) * ya;
                }
                if ((p - new Point(0.5 + cdis, 0.5)).SquareLength < cradsq)
                {
                    return ((1.0 - p.X) / cmax) * ya;
                }
                return ya;
            }

            private ImageSize _Size;
        }

        private int _Texture;
        private string _Text;
    }
}