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
            double y = 0.0;
            double width = 0.0;
            this._Items = new List<_Item>();
            foreach (MenuItem mi in Items)
            {
                _Item i = new _Item(Style, mi, y);
                Point size = i.Size;
                y += size.Y;
                width = Math.Max(width, size.X);
                this._Items.Add(i);
            }
            this.Size = new Point(width, y) + new Point(this._Style.Margin, this._Style.Margin) * 2.0;
        }

        public override void Render(GUIRenderContext Context)
        {
            Context.DrawSurface(Skin.Default.GetSurface(new SkinArea(96, 80, 16, 16), this.Size));
            Rectangle inner = new Rectangle(this.Size).Margin(this._Style.Margin);
            PopupStyle style = this._Style;
            foreach (_Item i in this._Items)
            {
                i.Render(Context, style, new Rectangle(0.0, i.Y, inner.Size.X, i.Size.Y) + inner.Location);
            }
        }

        /// <summary>
        /// Information about a item.
        /// </summary>
        private class _Item
        {
            public _Item(PopupStyle Style, MenuItem Source, double Y)
            {
                this.Source = Source;
                this.Y = Y;

                TextMenuItem tmi = Source as TextMenuItem;
                if (tmi != null)
                {
                    this.Sample = Style.Font.CreateSample(tmi.Text, null, TextAlign.Left, TextAlign.Center, TextWrap.Ellipsis);
                }

                CommandMenuItem cmi = Source as CommandMenuItem;
                if (cmi != null)
                {
                    this.Size = new Point(this.Sample.Size.X, Style.StandardItemHeight);
                }
            }

            /// <summary>
            /// The menuitem associated with this item.
            /// </summary>
            public MenuItem Source;

            /// <summary>
            /// The location of the top of the item not accounting for padding.
            /// </summary>
            public double Y;

            /// <summary>
            /// The size of the item.
            /// </summary>
            public Point Size;

            /// <summary>
            /// The text sample, if any, associated with this item.
            /// </summary>
            public TextSample Sample;

            /// <summary>
            /// Renders the item to the specified location.
            /// </summary>
            public void Render(GUIRenderContext Context, PopupStyle Style, Rectangle Area)
            {
                CommandMenuItem cmi = Source as CommandMenuItem;
                if (cmi != null)
                {
                    Context.DrawText(Style.TextColor, this.Sample, Area);
                }
            }
        }

        private PopupStyle _Style;
        private List<_Item> _Items;
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
        public double StandardItemHeight = 20.0;
    }
}