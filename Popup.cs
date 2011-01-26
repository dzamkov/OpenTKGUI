using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// A hovering control that shows possible options in vertically descending order. (A context menu).
    /// </summary>
    public class Popup : LayerControl
    {
        public Popup(IEnumerable<MenuItem> Items)
            : this(new PopupStyle(), Items)
        {
        }

        public Popup(PopupStyle Style, IEnumerable<MenuItem> Items)
        {
            this._Style = Style;
            this._Items = new List<MenuItem>(Items);
            this._CreateSamples();
            this.Size = new Point(this._Width, this._Height) + new Point(this._Style.Margin, this._Style.Margin) * 2.0;
        }

        public override void Render(GUIRenderContext Context)
        {
            Context.DrawSurface(Skin.Default.GetSurface(new SkinArea(96, 80, 16, 16), this.Size));
            Rectangle inner = new Rectangle(this.Size).Margin(this._Style.Margin);
            double y = inner.Location.Y;
            for (int t = 0; t < this._Items.Count; t++)
            {
                double height = this._Style.ItemHeight;
                Rectangle itemrect = new Rectangle(inner.Location.X, y, inner.Size.X, height);
                Context.DrawText(this._Style.TextColor, this._Samples[t], itemrect);
                y += height;
            }
        }

        private void _CreateSamples()
        {
            this._Samples = new List<TextSample>(this._Items.Count);
            foreach (MenuItem mi in this._Items)
            {
                this._Samples.Add(this._Style.Font.CreateSample(mi.Text, null, TextAlign.Left, TextAlign.Center, TextWrap.Ellipsis));
            }
        }

        /// <summary>
        /// Gets the target width of the inner area of the popup.
        /// </summary>
        private double _Width
        {
            get
            {
                double max = 0.0;
                for (int t = 0; t < this._Items.Count; t++)
                {
                    TextSample samp = this._Samples[t];
                    max = Math.Max(max, samp.Size.X);
                }
                return max;
            }
        }

        /// <summary>
        /// Gets the target height of the inner area of the popup.
        /// </summary>
        private double _Height
        {
            get
            {
                return this._Style.ItemHeight * this._Items.Count;
            }
        }

        private PopupStyle _Style;
        private List<MenuItem> _Items;
        private List<TextSample> _Samples;
    }

    /// <summary>
    /// Gives styling options for a popup.
    /// </summary>
    public class PopupStyle
    {
        public Skin Skin = Skin.Default;
        public SkinArea Back = new SkinArea(96, 80, 16, 16);
        public Font Font = Font.Default;
        public Color TextColor = Color.RGB(0.0, 0.0, 0.0);
        public double Margin = 3.0;
        public double ItemHeight = 20.0;
    }
}